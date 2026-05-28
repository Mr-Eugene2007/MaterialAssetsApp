using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace MaterialAssetsApp.Pages
{
    public partial class AddRoomPage : Page
    {
        private readonly MaterialAssetsEntities _context;
        private Room _selectedRoom;

        public AddRoomPage()
        {
            InitializeComponent();
            _context = new MaterialAssetsEntities();

            LoadDepartments();
            LoadRooms();
        }

        private void LoadDepartments()
        {
            cbDepartment.ItemsSource = _context.Departments
                .OrderBy(d => d.DepartmentName)
                .ToList();
        }

        private void LoadRooms()
        {
            dgRooms.ItemsSource = _context.Rooms
                .OrderBy(r => r.RoomNumber)
                .ToList();
        }

        private void dgRooms_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedRoom = dgRooms.SelectedItem as Room;

            if (_selectedRoom != null)
            {
                cbDepartment.SelectedValue = _selectedRoom.DepartmentID;
                txtRoomNumber.Text = _selectedRoom.RoomNumber;
                txtDescription.Text = _selectedRoom.Description;
            }
        }

        private void BtnAddNew_Click(object sender, RoutedEventArgs e)
        {
            dgRooms.SelectedItem = null;
            _selectedRoom = null;

            cbDepartment.SelectedIndex = -1;
            txtRoomNumber.Clear();
            txtDescription.Clear();
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (cbDepartment.SelectedValue == null)
            {
                MessageBox.Show("Выберите подразделение.");
                return;
            }

            if (string.IsNullOrWhiteSpace(txtRoomNumber.Text))
            {
                MessageBox.Show("Введите номер кабинета.");
                return;
            }

            // Редактирование
            if (_selectedRoom != null)
            {
                _selectedRoom.DepartmentID = (int)cbDepartment.SelectedValue;
                _selectedRoom.RoomNumber = txtRoomNumber.Text.Trim();
                _selectedRoom.Description = txtDescription.Text.Trim();

                _context.SaveChanges();
                LoadRooms();

                MessageBox.Show("Кабинет обновлён.");
                return;
            }

            // Добавление
            var newRoom = new Room
            {
                DepartmentID = (int)cbDepartment.SelectedValue,
                RoomNumber = txtRoomNumber.Text.Trim(),
                Description = txtDescription.Text.Trim()
            };

            _context.Rooms.Add(newRoom);
            _context.SaveChanges();

            LoadRooms();
            MessageBox.Show("Кабинет добавлен.");
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedRoom == null)
            {
                MessageBox.Show("Выберите кабинет для удаления.");
                return;
            }

            if (MessageBox.Show("Удалить выбранный кабинет?",
                                "Подтверждение",
                                MessageBoxButton.YesNo,
                                MessageBoxImage.Question) != MessageBoxResult.Yes)
                return;

            _context.Rooms.Remove(_selectedRoom);
            _context.SaveChanges();

            LoadRooms();

            cbDepartment.SelectedIndex = -1;
            txtRoomNumber.Clear();
            txtDescription.Clear();

            _selectedRoom = null;

            MessageBox.Show("Кабинет удалён.");
        }
    }
}
