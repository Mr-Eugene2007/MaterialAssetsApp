using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MaterialAssetsApp.Pages
{
    public partial class SearchRoomPage : Page
    {
        private MaterialAssetsEntities _context;

        public SearchRoomPage()
        {
            InitializeComponent();
            _context = new MaterialAssetsEntities();

            LoadDepartments();
            LoadAllRooms();
        }

        private void LoadDepartments()
        {
            cbDepartment.ItemsSource = _context.Departments
                                               .OrderBy(d => d.DepartmentName)
                                               .ToList();
        }

        private void LoadAllRooms()
        {
            var data = from r in _context.Rooms
                       join d in _context.Departments on r.DepartmentID equals d.DepartmentID
                       orderby d.DepartmentName, r.RoomNumber
                       select new
                       {
                           r.RoomID,
                           r.RoomNumber,
                           DepartmentName = d.DepartmentName,
                           r.Description
                       };

            dgRooms.ItemsSource = data.ToList();
        }

        private void BtnSearch_Click(object sender, RoutedEventArgs e)
        {
            string roomNumber = txtRoomNumber.Text.Trim().ToLower();
            int? depId = cbDepartment.SelectedValue as int?;

            var query = from r in _context.Rooms
                        join d in _context.Departments on r.DepartmentID equals d.DepartmentID
                        select new
                        {
                            r.RoomID,
                            r.RoomNumber,
                            DepartmentName = d.DepartmentName,
                            r.Description,
                            r.DepartmentID
                        };

            if (!string.IsNullOrWhiteSpace(roomNumber))
                query = query.Where(x => x.RoomNumber.ToLower().Contains(roomNumber));

            if (depId != null)
                query = query.Where(x => x.DepartmentID == depId);

            dgRooms.ItemsSource = query.ToList();
        }

        private void dgRooms_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (dgRooms.SelectedItem == null)
                return;

            dynamic row = dgRooms.SelectedItem;
            int roomId = row.RoomID;

            ((MainWindow)Application.Current.MainWindow)
                .MainFrame.Navigate(new CardListPage("room", roomId));
        }

        private void BtnReset_Click(object sender, RoutedEventArgs e)
        {
            txtRoomNumber.Clear();
            cbDepartment.SelectedIndex = -1;
            LoadAllRooms();
        }

    }
}
