using System.Windows;
using System.Windows.Controls;

namespace MaterialAssetsApp.Pages
{
    public partial class AddAssetTypePage : Page
    {
        private MaterialAssetsEntities _context;

        public AddAssetTypePage()
        {
            InitializeComponent();
            _context = new MaterialAssetsEntities();
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtTypeName.Text))
            {
                MessageBox.Show("Введите название типа.",
                                "Ошибка",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
                return;
            }

            try
            {
                var type = new AssetType
                {
                    TypeName = txtTypeName.Text.Trim()
                };

                _context.AssetTypes.Add(type);
                _context.SaveChanges();

                MessageBox.Show("Тип успешно добавлен.",
                                "Успех",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Ошибка при сохранении типа:\n" + ex.Message,
                                "Ошибка",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
            }
        }

        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            txtTypeName.Clear();
        }
    }
}
