using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace MaterialAssetsApp.Pages
{
    public partial class AddMovementPage : Page
    {
        private readonly MaterialAssetsEntities _context;
        private readonly int _cardId;

        public AddMovementPage(int cardId)
        {
            InitializeComponent();
            _context = new MaterialAssetsEntities();
            _cardId = cardId;

            LoadReferences();
            dpMovementDate.SelectedDate = DateTime.Now;
        }

        private void LoadReferences()
        {
            cbDepartment.ItemsSource = _context.Departments
                .OrderBy(d => d.DepartmentName)
                .ToList();

            var employees = _context.Employees
                .OrderBy(e => e.LastName)
                .Select(e => new
                {
                    e.EmployeeID,
                    FullName = e.LastName + " " + e.FirstName +
                               (e.MiddleName != null ? " " + e.MiddleName : "")
                })
                .ToList();

            cbHolder.ItemsSource = employees;
            cbTransferredBy.ItemsSource = employees;

            cbCondition.ItemsSource = _context.AssetConditions
                .OrderBy(c => c.ConditionName)
                .ToList();

            // Автоподстановка текущего держателя в поле "Передал"
            var card = _context.AccountingCards.FirstOrDefault(c => c.CardID == _cardId);
            if (card != null)
                cbTransferredBy.SelectedValue = card.CurrentHolderID;
        }

        private void cbDepartment_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbDepartment.SelectedValue == null)
            {
                cbRoom.ItemsSource = null;
                return;
            }

            int deptId = (int)cbDepartment.SelectedValue;

            cbRoom.ItemsSource = _context.Rooms
                .Where(r => r.DepartmentID == deptId)
                .OrderBy(r => r.RoomNumber)
                .ToList();
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (dpMovementDate.SelectedDate == null ||
                cbDepartment.SelectedValue == null ||
                cbHolder.SelectedValue == null ||
                cbCondition.SelectedValue == null)
            {
                MessageBox.Show("Заполните обязательные поля.");
                return;
            }

            int nextSeq = 1;
            var last = _context.AssetMovements
                .Where(m => m.CardID == _cardId)
                .OrderByDescending(m => m.SequenceNumber)
                .FirstOrDefault();
            if (last != null)
                nextSeq = last.SequenceNumber + 1;

            var movement = new AssetMovement
            {
                CardID = _cardId,
                SequenceNumber = nextSeq,
                MovementDate = dpMovementDate.SelectedDate.Value,
                DepartmentID = (int)cbDepartment.SelectedValue,
                RoomID = cbRoom.SelectedValue != null ? (int?)cbRoom.SelectedValue : null,
                HolderEmployeeID = (int)cbHolder.SelectedValue,
                TransferredByID = cbTransferredBy.SelectedValue != null
                    ? (int?)cbTransferredBy.SelectedValue
                    : null,
                ConditionID = (int)cbCondition.SelectedValue,
                Notes = string.IsNullOrWhiteSpace(txtNotes.Text) ? null : txtNotes.Text
            };

            _context.AssetMovements.Add(movement);

            // Обновляем карточку
            var card = _context.AccountingCards.FirstOrDefault(c => c.CardID == _cardId);
            if (card != null)
            {
                card.DepartmentID = movement.DepartmentID;
                card.RoomID = movement.RoomID;
                card.CurrentHolderID = movement.HolderEmployeeID;
                card.ConditionID = movement.ConditionID;
            }

            _context.SaveChanges();
            MessageBox.Show("Запись добавлена.");

            ((MainWindow)Application.Current.MainWindow)
                .MainFrame.GoBack();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            ((MainWindow)Application.Current.MainWindow)
                .MainFrame.GoBack();
        }
    }
}
