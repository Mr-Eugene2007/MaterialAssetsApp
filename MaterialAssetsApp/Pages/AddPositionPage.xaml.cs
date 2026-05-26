using System.Windows;
using System.Windows.Controls;

namespace MaterialAssetsApp.Pages
{
    public partial class AddPositionPage : Page
    {
        private MaterialAssetsEntities _context;

        public AddPositionPage()
        {
            InitializeComponent();
            _context = new MaterialAssetsEntities();
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtPositionName.Text))
            {
                MessageBox.Show("Введите название должности.",
                                "Ошибка",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
                return;
            }

            try
            {
                var pos = new Position
                {
                    PositionName = txtPositionName.Text.Trim()
                };

                _context.Positions.Add(pos);
                _context.SaveChanges();

                MessageBox.Show("Должность успешно добавлена.",
                                "Успех",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Ошибка при сохранении должности:\n" + ex.Message,
                                "Ошибка",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
            }
        }

        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            txtPositionName.Clear();
        }
    }
}
