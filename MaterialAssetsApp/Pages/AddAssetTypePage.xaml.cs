using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace MaterialAssetsApp.Pages
{
    public partial class AddAssetTypePage : Page
    {
        private MaterialAssetsEntities _context;
        private AssetType _selectedType;

        public AddAssetTypePage()
        {
            InitializeComponent();
            _context = new MaterialAssetsEntities();

            LoadTypes();
        }

        private void LoadTypes()
        {
            dgTypes.ItemsSource = _context.AssetTypes
                .OrderBy(t => t.TypeName)
                .ToList();
        }

        private void dgTypes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedType = dgTypes.SelectedItem as AssetType;

            if (_selectedType != null)
            {
                txtTypeName.Text = _selectedType.TypeName;
            }
        }

        private void BtnAddNew_Click(object sender, RoutedEventArgs e)
        {
            dgTypes.SelectedItem = null;
            _selectedType = null;

            txtTypeName.Clear();
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtTypeName.Text))
            {
                MessageBox.Show("Введите название типа.");
                return;
            }

            // Редактирование
            if (_selectedType != null)
            {
                _selectedType.TypeName = txtTypeName.Text.Trim();

                _context.SaveChanges();
                LoadTypes();

                MessageBox.Show("Тип обновлён.");
                return;
            }

            // Добавление
            var newType = new AssetType
            {
                TypeName = txtTypeName.Text.Trim()
            };

            _context.AssetTypes.Add(newType);
            _context.SaveChanges();

            LoadTypes();
            MessageBox.Show("Тип добавлен.");
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedType == null)
            {
                MessageBox.Show("Выберите тип для удаления.");
                return;
            }

            // Проверяем наличие моделей этого типа
            bool hasModels = _context.AssetModels.Any(m => m.AssetTypeID == _selectedType.AssetTypeID);
            if (hasModels)
            {
                MessageBox.Show("Невозможно удалить тип: к нему привязаны модели.\nСначала удалите или перенесите все модели этого типа.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (MessageBox.Show("Удалить выбранный тип?",
                                "Подтверждение",
                                MessageBoxButton.YesNo,
                                MessageBoxImage.Question) != MessageBoxResult.Yes)
                return;

            _context.AssetTypes.Remove(_selectedType);
            _context.SaveChanges();

            LoadTypes();
            txtTypeName.Clear();
            _selectedType = null;

            MessageBox.Show("Тип удалён.");
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            _context = new MaterialAssetsEntities(); // свежий контекст
            LoadTypes(); // или нужный метод загрузки
        }
    }
}
