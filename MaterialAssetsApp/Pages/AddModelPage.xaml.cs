using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace MaterialAssetsApp.Pages
{
    public partial class AddModelPage : Page
    {
        private MaterialAssetsEntities _context;

        public AddModelPage()
        {
            InitializeComponent();
            _context = new MaterialAssetsEntities();
            LoadTypes();
        }

        private void LoadTypes()
        {
            cbType.ItemsSource = _context.AssetTypes
                                         .OrderBy(t => t.TypeName)
                                         .ToList();
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (cbType.SelectedItem == null)
            {
                MessageBox.Show("Выберите тип.",
                                "Ошибка",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtBrand.Text))
            {
                MessageBox.Show("Введите бренд.",
                                "Ошибка",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtModelName.Text))
            {
                MessageBox.Show("Введите название модели.",
                                "Ошибка",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
                return;
            }

            try
            {
                var model = new AssetModel
                {
                    AssetTypeID = (int)cbType.SelectedValue,
                    Brand = txtBrand.Text.Trim(),
                    ModelName = txtModelName.Text.Trim()
                };

                _context.AssetModels.Add(model);
                _context.SaveChanges();

                MessageBox.Show("Модель успешно добавлена.",
                                "Успех",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Ошибка при сохранении модели:\n" + ex.Message,
                                "Ошибка",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
            }
        }

        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            cbType.SelectedIndex = -1;
            txtBrand.Clear();
            txtModelName.Clear();
        }
    }
}
