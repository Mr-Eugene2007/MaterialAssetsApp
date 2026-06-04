using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace MaterialAssetsApp.Pages
{
    public partial class EditMovementPage : Page
    {
        private readonly MaterialAssetsEntities _context;
        private readonly int _movementId;
        private readonly int _cardId;
        private bool _isLoading = false;
        private List<dynamic> _allEmployees;

        public EditMovementPage(int movementId, int cardId)
        {
            InitializeComponent();
            _context = new MaterialAssetsEntities();
            _movementId = movementId;
            _cardId = cardId;

            LoadReferences();
            LoadMovement();
        }

        // Загрузка справочников
        private void LoadReferences()
        {
            cbDepartment.ItemsSource = _context.Departments
                .OrderBy(d => d.DepartmentName)
                .ToList();

            _allEmployees = _context.Employees
                .OrderBy(e => e.LastName)
                .Select(e => new
                {
                    e.EmployeeID,
                    FullName = e.LastName + " " + e.FirstName +
                               (e.MiddleName != null ? " " + e.MiddleName : "") +
                               " (СНИЛС: " + e.SNILS + ")"
                })
                .ToList()
                .Cast<dynamic>()
                .ToList();

            cbHolder.ItemsSource = _allEmployees;
            cbTransferredBy.ItemsSource = _allEmployees;
        }

        private void txtSearchHolder_TextChanged(object sender, TextChangedEventArgs e)
        {
            FilterEmployees(txtSearchHolder.Text, cbHolder);
        }

        private void txtSearchTransferred_TextChanged(object sender, TextChangedEventArgs e)
        {
            FilterEmployees(txtSearchTransferred.Text, cbTransferredBy);
        }

        private void cbHolder_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbHolder.SelectedValue != null)
            {
                txtSearchHolder.TextChanged -= txtSearchHolder_TextChanged;
                txtSearchHolder.Clear();
                txtSearchHolder.TextChanged += txtSearchHolder_TextChanged;
                cbHolder.ItemsSource = _allEmployees;
            }
        }

        private void cbTransferredBy_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbTransferredBy.SelectedValue != null)
            {
                txtSearchTransferred.TextChanged -= txtSearchTransferred_TextChanged;
                txtSearchTransferred.Clear();
                txtSearchTransferred.TextChanged += txtSearchTransferred_TextChanged;
                cbTransferredBy.ItemsSource = _allEmployees;
            }
        }

        private void FilterEmployees(string search, ComboBox comboBox)
        {
            if (string.IsNullOrWhiteSpace(search))
            {
                comboBox.ItemsSource = _allEmployees;
                return;
            }

            string lower = search.ToLower();
            comboBox.ItemsSource = _allEmployees
                .Where(emp => ((string)emp.FullName).ToLower().Contains(lower))
                .ToList();

            comboBox.IsDropDownOpen = true;
        }

        // Загрузка записи
        private void LoadMovement()
        {
            var m = _context.AssetMovements
                .FirstOrDefault(x => x.MovementID == _movementId);

            if (m == null)
            {
                MessageBox.Show("Запись перемещения не найдена.");
                GoBack();
                return;
            }

            _isLoading = true;

            dpMovementDate.SelectedDate = m.MovementDate;

            cbDepartment.SelectedValue = m.DepartmentID;

            var rooms = _context.Rooms
                .Where(r => r.DepartmentID == m.DepartmentID)
                .OrderBy(r => r.RoomNumber)
                .ToList();

            cbRoom.ItemsSource = rooms;
            cbRoom.SelectedValue = m.RoomID;

            cbHolder.SelectedValue = m.HolderEmployeeID;
            cbTransferredBy.SelectedValue = m.TransferredByID;
            cbCondition.SelectedValue = m.ConditionID;
            txtNotes.Text = m.Notes;

            _isLoading = false;
        }

        // При смене подразделения — подгружаем кабинеты
        private void cbDepartment_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isLoading)
                return;

            if (cbDepartment.SelectedValue == null)
            {
                cbRoom.ItemsSource = null;
                cbRoom.SelectedItem = null;
                return;
            }

            int deptId = (int)cbDepartment.SelectedValue;

            var rooms = _context.Rooms
                .Where(r => r.DepartmentID == deptId)
                .OrderBy(r => r.RoomNumber)
                .ToList();

            cbRoom.ItemsSource = rooms;
            cbRoom.SelectedItem = null;
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (dpMovementDate.SelectedDate == null)
            {
                MessageBox.Show("Укажите дату перемещения.", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (cbDepartment.SelectedValue == null)
            {
                MessageBox.Show("Выберите подразделение.", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (cbHolder.SelectedValue == null)
            {
                MessageBox.Show("Выберите держателя.", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (cbCondition.SelectedValue == null)
            {
                MessageBox.Show("Выберите состояние.", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var movement = _context.AssetMovements
                    .FirstOrDefault(x => x.MovementID == _movementId);

                if (movement == null)
                {
                    MessageBox.Show("Запись не найдена.");
                    return;
                }

                movement.MovementDate = dpMovementDate.SelectedDate.Value;
                movement.DepartmentID = (int)cbDepartment.SelectedValue;
                movement.RoomID = cbRoom.SelectedValue != null ? (int?)cbRoom.SelectedValue : null;
                movement.HolderEmployeeID = (int)cbHolder.SelectedValue;
                movement.TransferredByID = cbTransferredBy.SelectedValue != null
                                                ? (int?)cbTransferredBy.SelectedValue
                                                : null;
                movement.ConditionID = (int)cbCondition.SelectedValue;
                movement.Notes = string.IsNullOrWhiteSpace(txtNotes.Text) ? null : txtNotes.Text;

                bool isLastMovement = !_context.AssetMovements
                    .Any(x => x.CardID == _cardId && x.SequenceNumber > movement.SequenceNumber);

                if (isLastMovement)
                {
                    var card = _context.AccountingCards
                        .FirstOrDefault(c => c.CardID == _cardId);

                    if (card != null)
                    {
                        card.DepartmentID = movement.DepartmentID;
                        card.RoomID = movement.RoomID;
                        card.CurrentHolderID = movement.HolderEmployeeID;
                        card.ConditionID = movement.ConditionID;
                    }
                }

                _context.SaveChanges();

                MessageBox.Show("Изменения сохранены.",
                    "Успешно", MessageBoxButton.OK, MessageBoxImage.Information);

                GoBack();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при сохранении:\n" + ex.Message,
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            GoBack();
        }

        private void GoBack()
        {
            var main = Application.Current.MainWindow as MainWindow;
            if (main != null && main.MainFrame.CanGoBack)
                main.MainFrame.GoBack();
            else
                NavigationService?.GoBack();
        }


    }
}
