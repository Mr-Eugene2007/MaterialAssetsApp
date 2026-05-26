using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MaterialAssetsApp.Pages
{
    public partial class MovementHistoryPage : Page
    {
        private readonly MaterialAssetsEntities _context;
        private readonly int _cardId;

        public MovementHistoryPage(int cardId)
        {
            InitializeComponent();
            _context = new MaterialAssetsEntities();
            _cardId = cardId;

            LoadMovements();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LoadMovements();
        }


        private void LoadMovements()
        {
            if (_cardId <= 0)
            {
                MessageBox.Show("Некорректный ID карточки. История перемещений недоступна.");
                return;
            }

            var raw = _context.AssetMovements
                .Where(m => m.CardID == _cardId)
                .OrderBy(m => m.SequenceNumber)
                .Select(m => new
                {
                    m.MovementID,
                    m.SequenceNumber,
                    m.MovementDate,
                    DepartmentName = m.Department != null ? m.Department.DepartmentName : "-",
                    RoomNumber = m.Room != null ? m.Room.RoomNumber : "-",
                    HolderName = m.Employee != null
                        ? m.Employee.LastName + " " + m.Employee.FirstName
                        : "-",
                    ConditionName = m.AssetCondition != null ? m.AssetCondition.ConditionName : "-",
                    m.Notes
                })
                .ToList();

            var data = raw.Select(m => new
            {
                m.MovementID,
                m.SequenceNumber,
                MovementDate = m.MovementDate.ToShortDateString(),
                m.DepartmentName,
                m.RoomNumber,
                m.HolderName,
                m.ConditionName,
                m.Notes
            }).ToList();

            dgMovements.ItemsSource = data;
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (dgMovements.SelectedItem == null)
            {
                MessageBox.Show("Выберите запись.");
                return;
            }

            dynamic row = dgMovements.SelectedItem;
            int movementId = row.MovementID;

            ((MainWindow)Application.Current.MainWindow)
                .MainFrame.Navigate(new EditMovementPage(movementId, _cardId));
        }

        private void dgMovements_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (dgMovements.SelectedItem == null)
                return;

            dynamic row = dgMovements.SelectedItem;
            int movementId = row.MovementID;

            ((MainWindow)Application.Current.MainWindow)
                .MainFrame.Navigate(new EditMovementPage(movementId, _cardId));
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            ((MainWindow)Application.Current.MainWindow)
                .MainFrame.Navigate(new AddMovementPage(_cardId));
        }



        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            var main = Application.Current.MainWindow as MainWindow;
            if (main != null && main.MainFrame.CanGoBack)
                main.MainFrame.GoBack();
            else
                NavigationService?.GoBack();


        }
    }
}
