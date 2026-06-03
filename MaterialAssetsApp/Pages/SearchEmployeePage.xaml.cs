using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MaterialAssetsApp.Pages
{
    public partial class SearchEmployeePage : Page
    {
        private MaterialAssetsEntities _context;

        public SearchEmployeePage()
        {
            InitializeComponent();
            _context = new MaterialAssetsEntities();

            LoadDepartments();
            LoadPositions();
            LoadAllEmployees();
        }

        private void LoadDepartments()
        {
            cbDepartment.ItemsSource = _context.Departments
                                               .OrderBy(d => d.DepartmentName)
                                               .ToList();
        }

        private void LoadPositions()
        {
            cbPosition.ItemsSource = _context.Positions
                                             .OrderBy(p => p.PositionName)
                                             .ToList();
        }

        private void LoadAllEmployees()
        {
            var data = (from e in _context.Employees
                        join ep in _context.EmployeePositions on e.EmployeeID equals ep.EmployeeID into epJoin
                        from ep in epJoin.Where(x => x.EndDate == null).DefaultIfEmpty()
                        join d in _context.Departments on ep.DepartmentID equals d.DepartmentID into dJoin
                        from d in dJoin.DefaultIfEmpty()
                        join p in _context.Positions on ep.PositionID equals p.PositionID into pJoin
                        from p in pJoin.DefaultIfEmpty()
                        select new
                        {
                            e.EmployeeID,
                            FullName = e.LastName + " " + e.FirstName +
                                       (string.IsNullOrEmpty(e.MiddleName) ? "" : " " + e.MiddleName),
                            e.PhoneMobile,
                            e.Email,
                            DepartmentName = d != null ? d.DepartmentName : "",
                            PositionName = p != null ? p.PositionName : ""
                        }).ToList();

            dgEmployees.ItemsSource = data;
        }

        private void BtnSearch_Click(object sender, RoutedEventArgs e)
        {
            string name = txtName.Text.Trim().ToLower();
            string phone = txtPhone.Text.Trim().ToLower();
            int? depId = cbDepartment.SelectedValue as int?;
            int? posId = cbPosition.SelectedValue as int?;

            var query =
                from emp in _context.Employees
                join ep in _context.EmployeePositions on emp.EmployeeID equals ep.EmployeeID into epJoin
                from empPos in epJoin.Where(x => x.EndDate == null).DefaultIfEmpty()
                join d in _context.Departments on empPos.DepartmentID equals d.DepartmentID into dJoin
                from dep in dJoin.DefaultIfEmpty()
                join p in _context.Positions on empPos.PositionID equals p.PositionID into pJoin
                from pos in pJoin.DefaultIfEmpty()
                select new
                {
                    emp.EmployeeID,
                    FullName = emp.LastName + " " + emp.FirstName +
                               (string.IsNullOrEmpty(emp.MiddleName) ? "" : " " + emp.MiddleName),
                    emp.PhoneMobile,
                    emp.Email,
                    DepartmentID = dep != null ? dep.DepartmentID : (int?)null,
                    DepartmentName = dep != null ? dep.DepartmentName : "",
                    PositionID = pos != null ? pos.PositionID : (int?)null,
                    PositionName = pos != null ? pos.PositionName : ""
                };


            // Фильтры
            if (!string.IsNullOrWhiteSpace(name))
                query = query.Where(x => x.FullName.ToLower().Contains(name));

            if (!string.IsNullOrWhiteSpace(phone))
                query = query.Where(x => x.PhoneMobile != null &&
                                         x.PhoneMobile.ToLower().Contains(phone));

            if (depId != null)
                query = query.Where(x => x.DepartmentID == depId);

            if (posId != null)
                query = query.Where(x => x.PositionID == posId);

            dgEmployees.ItemsSource = query.ToList();
        }

        private void dgEmployees_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (dgEmployees.SelectedItem == null)
                return;

            dynamic row = dgEmployees.SelectedItem;
            int employeeId = row.EmployeeID;

            ((MainWindow)Application.Current.MainWindow)
                .MainFrame.Navigate(new CardListPage("employee", employeeId));
        }

        private void BtnReset_Click(object sender, RoutedEventArgs e)
        {
            txtName.Clear();
            txtPhone.Clear();
            cbDepartment.SelectedIndex = -1;
            cbPosition.SelectedIndex = -1;
            LoadAllEmployees();
        }

    }
}
