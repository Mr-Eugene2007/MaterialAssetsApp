using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MaterialAssetsApp.Pages
{
    public partial class SearchCardPage : Page
    {
        private MaterialAssetsEntities _context;

        public SearchCardPage()
        {
            InitializeComponent();
            _context = new MaterialAssetsEntities();

            LoadTypes();
            LoadModels();
            LoadDepartments();
            LoadAllCards();
        }

        private void LoadTypes()
        {
            cbType.ItemsSource = _context.AssetTypes
                                         .OrderBy(t => t.TypeName)
                                         .ToList();
        }

        private void LoadModels()
        {
            cbModel.ItemsSource = _context.AssetModels
                                          .OrderBy(m => m.ModelName)
                                          .ToList();
        }

        private void LoadDepartments()
        {
            cbDepartment.ItemsSource = _context.Departments
                                               .OrderBy(d => d.DepartmentName)
                                               .ToList();
        }

        private void LoadAllCards()
        {
            dgCards.ItemsSource = _context.vw_AssetCurrentLocation
                                          .OrderBy(c => c.InventoryNumber)
                                          .ToList();
        }

        private void BtnSearch_Click(object sender, RoutedEventArgs e)
        {
            string inv = txtInvNumber.Text.Trim().ToLower();
            string name = txtAssetName.Text.Trim().ToLower();
            int? typeId = cbType.SelectedValue as int?;
            int? modelId = cbModel.SelectedValue as int?;
            int? depId = cbDepartment.SelectedValue as int?;

            var query = _context.vw_AssetCurrentLocation.AsQueryable();

            if (!string.IsNullOrWhiteSpace(inv))
                query = query.Where(x => x.InventoryNumber.ToLower().Contains(inv));

            if (!string.IsNullOrWhiteSpace(name))
                query = query.Where(x => x.AssetName.ToLower().Contains(name));

            if (typeId != null)
                query = query.Where(x => x.AssetTypeID == typeId);

            if (modelId != null)
                query = query.Where(x => x.ModelID == modelId);

            if (depId != null)
                query = query.Where(x => x.DepartmentID == depId);

            dgCards.ItemsSource = query.ToList();
        }

        private void dgCards_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (dgCards.SelectedItem == null)
                return;

            dynamic row = dgCards.SelectedItem;
            int cardId = row.CardID;

            NavigationService.Navigate(new EditAccountingCardPage(cardId));
        }

    }
}
