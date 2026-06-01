using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace MaterialAssetsApp.Pages
{
    public partial class CardListPage : Page
    {
        private readonly MaterialAssetsEntities _context;
        private readonly string _mode;
        private readonly int _id;

        public CardListPage(string mode, int id)
        {
            InitializeComponent();
            _context = new MaterialAssetsEntities();
            _mode = mode;
            _id = id;

            LoadCards();
        }

        private void LoadCards()
        {
            IQueryable<AccountingCard> query = _context.AccountingCards;

            switch (_mode)
            {
                case "employee":
                    txtTitle.Text = "Карточки сотрудника";
                    query = query.Where(c => c.CurrentHolderID == _id);
                    break;

                case "department":
                    txtTitle.Text = "Карточки подразделения";
                    query = query.Where(c => c.DepartmentID == _id);
                    break;

                case "room":
                    txtTitle.Text = "Карточки кабинета";
                    query = query.Where(c => c.RoomID == _id);
                    break;
            }

            var data = query
                .Select(c => new
                {
                    c.CardID,
                    c.InventoryNumber,
                    c.AssetName,
                    TypeName = c.AssetModel.AssetType.TypeName,
                    ModelName = c.AssetModel.ModelName,
                    DepartmentName = c.Department.DepartmentName,
                    RoomNumber = c.Room.RoomNumber,
                    HolderName = c.Employee1.LastName + " " + c.Employee1.FirstName,
                    ConditionName = c.AssetCondition.ConditionName
                })
                .ToList();

            dgCards.ItemsSource = data;
        }

        private void BtnOpenMassMove_Click(object sender, RoutedEventArgs e)
        {
            ((MainWindow)Application.Current.MainWindow)
                .MainFrame.Navigate(new MassMovePage());
        }


        private void dgCards_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (dgCards.SelectedItem == null)
                return;

            dynamic row = dgCards.SelectedItem;
            int cardId = row.CardID;

            ((MainWindow)Application.Current.MainWindow)
                .MainFrame.Navigate(new EditAccountingCardPage(cardId));
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            ((MainWindow)Application.Current.MainWindow)
                .MainFrame.GoBack();
        }
    }
}
