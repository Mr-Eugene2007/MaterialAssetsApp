using System.Windows;

namespace MaterialAssetsApp
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // Можно загрузить стартовую страницу, например, пустую или "Главная"
            // MainFrame.Navigate(new Pages.HomePage());
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
    }
}
