using System.Collections.Generic;
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
                .Select(c => new CardListItem
                {
                    IsSelected = false,
                    CardID = c.CardID,
                    InventoryNumber = c.InventoryNumber,
                    AssetName = c.AssetName,
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

        public class CardListItem
        {
            public bool IsSelected { get; set; }
            public int CardID { get; set; }
            public string InventoryNumber { get; set; }
            public string AssetName { get; set; }
            public string TypeName { get; set; }
            public string ModelName { get; set; }
            public string DepartmentName { get; set; }
            public string RoomNumber { get; set; }
            public string HolderName { get; set; }
            public string ConditionName { get; set; }
        }

        private void BtnMoveSelected_Click(object sender, RoutedEventArgs e)
        {
            var selected = dgCards.SelectedItems
                .Cast<CardListItem>()
                .Select(x => x.CardID)
                .ToList();

            if (!selected.Any())
            {
                MessageBox.Show("Выберите хотя бы одну карточку.");
                return;
            }

            ((MainWindow)Application.Current.MainWindow)
                .MainFrame.Navigate(new MassMoveSelectedPage(selected));
        }

    }
}
