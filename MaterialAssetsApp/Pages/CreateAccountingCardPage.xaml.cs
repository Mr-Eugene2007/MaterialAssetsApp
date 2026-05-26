using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace MaterialAssetsApp.Pages
{
    public partial class CreateAccountingCardPage : Page
    {
        private MaterialAssetsEntities _context;

        public CreateAccountingCardPage()
        {
            InitializeComponent();
            _context = new MaterialAssetsEntities();

            LoadModels();
            LoadConditions();
            LoadDepartments();
            LoadEmployees();
        }

        private void LoadModels()
        {
            cbModel.ItemsSource = _context.AssetModels
                                         .OrderBy(m => m.ModelName)
                                         .ToList();
        }

        private void LoadConditions()
        {
            cbCondition.ItemsSource = _context.AssetConditions
                                              .OrderBy(c => c.ConditionName)
                                              .ToList();
        }

        private void LoadDepartments()
        {
            cbDepartment.ItemsSource = _context.Departments
                                               .OrderBy(d => d.DepartmentName)
                                               .ToList();
        }

        private void LoadEmployees()
        {
            cbResponsible.ItemsSource =
            cbHolder.ItemsSource =
                _context.Employees
                        .OrderBy(e => e.LastName)
                        .Select(e => new
                        {
                            e.EmployeeID,
                            FullName = e.LastName + " " + e.FirstName +
                                       (string.IsNullOrEmpty(e.MiddleName) ? "" : " " + e.MiddleName)
                        })
                        .ToList();
        }

        private void cbDepartment_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbDepartment.SelectedValue == null)
            {
                cbRoom.ItemsSource = null;
                return;
            }

            int depId = (int)cbDepartment.SelectedValue;

            cbRoom.ItemsSource = _context.Rooms
                                         .Where(r => r.DepartmentID == depId)
                                         .OrderBy(r => r.RoomNumber)
                                         .ToList();
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (cbModel.SelectedValue == null ||
                string.IsNullOrWhiteSpace(txtAssetName.Text) ||
                string.IsNullOrWhiteSpace(txtInventoryNumber.Text) ||
                cbCondition.SelectedValue == null ||
                cbDepartment.SelectedValue == null ||
                cbResponsible.SelectedValue == null)
            {
                MessageBox.Show("Заполните обязательные поля.",
                                "Ошибка",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
                return;
            }

            try
            {
                var card = new AccountingCard
                {
                    ModelID = (int)cbModel.SelectedValue,
                    AssetName = txtAssetName.Text.Trim(),
                    InventoryNumber = txtInventoryNumber.Text.Trim(),
                    SerialNumber = string.IsNullOrWhiteSpace(txtSerialNumber.Text)
                                    ? null
                                    : txtSerialNumber.Text.Trim(),
                    ManufactureDate = dpManufactureDate.SelectedDate,
                    CommissionDate = dpCommissionDate.SelectedDate,
                    ConditionID = (int)cbCondition.SelectedValue,
                    DepartmentID = (int)cbDepartment.SelectedValue,
                    RoomID = cbRoom.SelectedValue as int?,
                    ResponsibleEmployeeID = (int)cbResponsible.SelectedValue,
                    CurrentHolderID = cbHolder.SelectedValue as int?,
                    Notes = null
                };

                _context.AccountingCards.Add(card);
                _context.SaveChanges();

                MessageBox.Show("Учетная карточка успешно создана.",
                                "Успех",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Ошибка при сохранении карточки:\n" + ex.Message,
                                "Ошибка",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
            }
        }

        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            cbModel.SelectedIndex = -1;
            txtAssetName.Clear();
            txtInventoryNumber.Clear();
            txtSerialNumber.Clear();
            dpManufactureDate.SelectedDate = null;
            dpCommissionDate.SelectedDate = null;
            cbCondition.SelectedIndex = -1;
            cbDepartment.SelectedIndex = -1;
            cbRoom.ItemsSource = null;
            cbResponsible.SelectedIndex = -1;
            cbHolder.SelectedIndex = -1;
        }
    }
}
