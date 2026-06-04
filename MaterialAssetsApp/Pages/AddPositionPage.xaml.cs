using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace MaterialAssetsApp.Pages
{
    public partial class AddPositionPage : Page
    {
        private MaterialAssetsEntities _context;
        private Position _selectedPosition;

        public AddPositionPage()
        {
            InitializeComponent();
            _context = new MaterialAssetsEntities();

            LoadPositions();
        }

        private void LoadPositions()
        {
            dgPositions.ItemsSource = _context.Positions
                .OrderBy(p => p.PositionName)
                .ToList();
        }

        private void dgPositions_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedPosition = dgPositions.SelectedItem as Position;

            if (_selectedPosition != null)
            {
                txtPositionName.Text = _selectedPosition.PositionName;
            }
        }

        private void BtnAddNew_Click(object sender, RoutedEventArgs e)
        {
            dgPositions.SelectedItem = null;
            _selectedPosition = null;

            txtPositionName.Clear();
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtPositionName.Text))
            {
                MessageBox.Show("Введите название должности.");
                return;
            }

            // Редактирование
            if (_selectedPosition != null)
            {
                _selectedPosition.PositionName = txtPositionName.Text.Trim();

                _context.SaveChanges();
                LoadPositions();

                MessageBox.Show("Должность обновлена.");
                return;
            }

            // Добавление
            var newPosition = new Position
            {
                PositionName = txtPositionName.Text.Trim()
            };

            _context.Positions.Add(newPosition);
            _context.SaveChanges();

            LoadPositions();
            MessageBox.Show("Должность добавлена.");
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedPosition == null)
            {
                MessageBox.Show("Выберите должность для удаления.");
                return;
            }

            if (MessageBox.Show("Удалить выбранную должность?",
                                "Подтверждение",
                                MessageBoxButton.YesNo,
                                MessageBoxImage.Question) != MessageBoxResult.Yes)
                return;

            _context.Positions.Remove(_selectedPosition);
            _context.SaveChanges();

            LoadPositions();

            txtPositionName.Clear();
            _selectedPosition = null;

            MessageBox.Show("Должность удалена.");
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            _context = new MaterialAssetsEntities(); // свежий контекст
            LoadPositions(); // или нужный метод загрузки
        }
    }
}
