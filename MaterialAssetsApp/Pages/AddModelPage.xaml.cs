using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace MaterialAssetsApp.Pages
{
    public partial class AddModelPage : Page
    {
        private readonly MaterialAssetsEntities _context;
        private AssetModel _selectedModel;

        public AddModelPage()
        {
            InitializeComponent();
            _context = new MaterialAssetsEntities();

            LoadTypes();
            LoadModels();
        }

        private void LoadTypes()
        {
            var list = _context.AssetTypes
                .OrderBy(t => t.TypeName)
                .ToList();

            var items = new System.Collections.Generic.List<TypeItem>();
            items.Add(new TypeItem { AssetTypeID = null, TypeName = "— Не выбрано —" });
            items.AddRange(list.Select(t => new TypeItem
            {
                AssetTypeID = t.AssetTypeID,
                TypeName = t.TypeName
            }));

            cbType.ItemsSource = items;
            cbType.SelectedIndex = 0;
        }

        private class TypeItem
        {
            public int? AssetTypeID { get; set; }
            public string TypeName { get; set; }
        }

        private void LoadModels()
        {
            dgModels.ItemsSource = _context.AssetModels
                .OrderBy(m => m.ModelName)
                .ToList();
        }

        private void dgModels_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedModel = dgModels.SelectedItem as AssetModel;

            if (_selectedModel != null)
            {
                cbType.SelectedValue = _selectedModel.AssetTypeID;
                txtBrand.Text = _selectedModel.Brand;
                txtModelName.Text = _selectedModel.ModelName;
            }
        }

        private void BtnAddNew_Click(object sender, RoutedEventArgs e)
        {
            dgModels.SelectedItem = null;
            _selectedModel = null;

            cbType.SelectedIndex = -1;
            txtBrand.Clear();
            txtModelName.Clear();
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (cbType.SelectedValue == null || (cbType.SelectedValue as int?) == null)
            {
                MessageBox.Show("Выберите тип.");
                return;
            }


            if (string.IsNullOrWhiteSpace(txtModelName.Text))
            {
                MessageBox.Show("Введите название модели.");
                return;
            }

            // Редактирование
            if (_selectedModel != null)
            {
                _selectedModel.AssetTypeID = (int)cbType.SelectedValue;
                _selectedModel.Brand = txtBrand.Text.Trim();
                _selectedModel.ModelName = txtModelName.Text.Trim();

                _context.SaveChanges();
                LoadModels();

                MessageBox.Show("Модель обновлена.");
                return;
            }

            // Добавление
            var newModel = new AssetModel
            {
                AssetTypeID = (int)cbType.SelectedValue,
                Brand = txtBrand.Text.Trim(),
                ModelName = txtModelName.Text.Trim()
            };

            _context.AssetModels.Add(newModel);
            _context.SaveChanges();

            LoadModels();
            MessageBox.Show("Модель добавлена.");
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedModel == null)
            {
                MessageBox.Show("Выберите модель для удаления.");
                return;
            }

            if (MessageBox.Show("Удалить выбранную модель?",
                                "Подтверждение",
                                MessageBoxButton.YesNo,
                                MessageBoxImage.Question) != MessageBoxResult.Yes)
                return;

            _context.AssetModels.Remove(_selectedModel);
            _context.SaveChanges();

            LoadModels();

            cbType.SelectedIndex = -1;
            txtBrand.Clear();
            txtModelName.Clear();

            _selectedModel = null;

            MessageBox.Show("Модель удалена.");
        }
    }
}
