using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MaterialAssetsApp.Pages
{
    public partial class SearchDepartmentPage : Page
    {
        private MaterialAssetsEntities _context;

        public SearchDepartmentPage()
        {
            InitializeComponent();
            _context = new MaterialAssetsEntities();

            LoadParentDepartments();
            LoadAllDepartments();
        }

        private void LoadParentDepartments()
        {
            cbParent.ItemsSource = _context.Departments
                                           .OrderBy(d => d.DepartmentName)
                                           .ToList();
        }

        private void LoadAllDepartments()
        {
            var data =
                from d in _context.Departments
                join p in _context.Departments on d.ParentID equals p.DepartmentID into parentJoin
                from parent in parentJoin.DefaultIfEmpty()
                select new
                {
                    d.DepartmentID,
                    d.DepartmentName,
                    ParentName = parent != null ? parent.DepartmentName : "",
                    RoomCount = _context.Rooms.Count(r => r.DepartmentID == d.DepartmentID)
                };

            dgDepartments.ItemsSource = data.ToList();
        }

        private void BtnSearch_Click(object sender, RoutedEventArgs e)
        {
            string name = txtDepartmentName.Text.Trim().ToLower();
            int? parentId = cbParent.SelectedValue as int?;

            var query =
                from d in _context.Departments
                join p in _context.Departments on d.ParentID equals p.DepartmentID into parentJoin
                from parent in parentJoin.DefaultIfEmpty()
                select new
                {
                    d.DepartmentID,
                    d.DepartmentName,
                    ParentID = parent != null ? parent.DepartmentID : (int?)null,
                    ParentName = parent != null ? parent.DepartmentName : "",
                    RoomCount = _context.Rooms.Count(r => r.DepartmentID == d.DepartmentID)
                };

            if (!string.IsNullOrWhiteSpace(name))
                query = query.Where(x => x.DepartmentName.ToLower().Contains(name));

            if (parentId != null)
                query = query.Where(x => x.ParentID == parentId);

            dgDepartments.ItemsSource = query.ToList();
        }

        private void dgDepartments_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (dgDepartments.SelectedItem == null)
                return;

            dynamic row = dgDepartments.SelectedItem;
            int departmentId = row.DepartmentID;

            ((MainWindow)Application.Current.MainWindow)
                .MainFrame.Navigate(new CardListPage("department", departmentId));
        }

        private void BtnReset_Click(object sender, RoutedEventArgs e)
        {
            txtDepartmentName.Clear();
            cbParent.SelectedIndex = -1;
            LoadAllDepartments();
        }

        private void BtnExport_Click(object sender, RoutedEventArgs e)
        {
            ExcelExporter.Export(dgDepartments, "Подразделения");
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            _context = new MaterialAssetsEntities(); // свежий контекст
            LoadAllDepartments(); // или нужный метод загрузки
        }

    }
}
