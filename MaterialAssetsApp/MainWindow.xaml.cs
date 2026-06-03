using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace MaterialAssetsApp
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            LoadCurrentEmployee();

        }

        private void BtnCreateCard_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new Pages.CreateAccountingCardPage());
        }

        private void BtnAddEmployee_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new Pages.AddEmployeePage());
        }

        private void BtnAddDepartment_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new Pages.AddDepartmentPage());
        }

        private void BtnAddRoom_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new Pages.AddRoomPage());
        }

        private void BtnAddAssetType_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new Pages.AddAssetTypePage());
        }

        private void BtnAddModel_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new Pages.AddModelPage());
        }

        private void BtnAddPosition_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new Pages.AddPositionPage());
        }

        private void BtnSearchEmployee_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new Pages.SearchEmployeePage());
        }

        private void BtnSearchRoom_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new Pages.SearchRoomPage());
        }

        private void BtnSearchCard_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new Pages.SearchCardPage());
        }

        private void BtnSearchDepartment_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new Pages.SearchDepartmentPage());
        }

        // В конце LoadCurrentEmployee — восстанавливаем сохранённый выбор
        private void LoadCurrentEmployee()
        {
            var context = new MaterialAssetsEntities();
            cbCurrentEmployee.ItemsSource = context.Employees
                .OrderBy(e => e.LastName)
                .Select(e => new
                {
                    e.EmployeeID,
                    FullName = e.LastName + " " + e.FirstName
                })
                .ToList();

            // Восстанавливаем последний выбор
            if (AppSettings.LastEmployeeID != 0)
                cbCurrentEmployee.SelectedValue = AppSettings.LastEmployeeID;
        }

        // При смене — сохраняем
        private void cbCurrentEmployee_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbCurrentEmployee.SelectedValue == null) return;

            CurrentSession.EmployeeID = (int)cbCurrentEmployee.SelectedValue;

            AppSettings.LastEmployeeID = CurrentSession.EmployeeID;
            Properties.Settings.Default.Save();
        }
    }
}
