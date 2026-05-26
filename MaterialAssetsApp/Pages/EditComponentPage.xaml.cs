using System.Windows;
using System.Windows.Controls;
using System.Linq;

namespace MaterialAssetsApp.Pages
{
    public partial class EditComponentPage : Page
    {
        private readonly MaterialAssetsEntities _context;
        private AssetComponent _comp;

        public EditComponentPage(int componentId)
        {
            InitializeComponent();
            _context = new MaterialAssetsEntities();

            _comp = _context.AssetComponents.First(c => c.ComponentID == componentId);

            txtName.Text = _comp.ComponentName;
            txtNumber.Text = _comp.ComponentNumber;
            txtQuantity.Text = _comp.Quantity.ToString();
            txtNotes.Text = _comp.Notes;
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(txtQuantity.Text, out int qty) || qty <= 0)
            {
                MessageBox.Show("Количество должно быть числом > 0.");
                return;
            }

            _comp.ComponentName = txtName.Text;
            _comp.ComponentNumber = txtNumber.Text;
            _comp.Quantity = qty;
            _comp.Notes = txtNotes.Text;

            _context.SaveChanges();

            ((MainWindow)Application.Current.MainWindow).MainFrame.GoBack();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            ((MainWindow)Application.Current.MainWindow).MainFrame.GoBack();
        }
    }
}
