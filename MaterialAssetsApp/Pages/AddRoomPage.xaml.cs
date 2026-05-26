using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace MaterialAssetsApp.Pages
{
    public partial class AddRoomPage : Page
    {
        private MaterialAssetsEntities _context;

        public AddRoomPage()
        {
            InitializeComponent();
            _context = new MaterialAssetsEntities();
            LoadDepartments();
        }

        private void LoadDepartments()
        {
            var deps = _context.Departments
                               .OrderBy(d => d.DepartmentName)
                               .ToList();

            cbDepartment.ItemsSource = deps;
            cbDepartment.SelectedIndex = deps.Any() ? 0 : -1;
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (cbDepartment.SelectedItem == null)
            {
                MessageBox.Show("Выберите подразделение.",
                                "Ошибка",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtRoomNumber.Text))
            {
                MessageBox.Show("Укажите номер кабинета.",
                                "Ошибка",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
                return;
            }

            try
            {
                var dep = (Department)cbDepartment.SelectedItem;

                var room = new Room
                {
                    DepartmentID = dep.DepartmentID,
                    RoomNumber = txtRoomNumber.Text.Trim(),
                    Description = string.IsNullOrWhiteSpace(txtDescription.Text)
                                    ? null
                                    : txtDescription.Text.Trim()
                };

                _context.Rooms.Add(room);
                _context.SaveChanges();

                MessageBox.Show("Кабинет успешно добавлен.",
                                "Успех",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Ошибка при сохранении кабинета:\n" + ex.Message,
                                "Ошибка",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
            }
        }

        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            if (cbDepartment.Items.Count > 0)
                cbDepartment.SelectedIndex = 0;

            txtRoomNumber.Clear();
            txtDescription.Clear();
        }
    }
}
