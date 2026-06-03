using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace MaterialAssetsApp.Pages
{
    public partial class MassMoveSelectedPage : Page
    {
        private readonly MaterialAssetsEntities _context;
        private readonly List<int> _cardIds;


        public MassMoveSelectedPage(List<int> cardIds)
        {
            InitializeComponent();
            _context = new MaterialAssetsEntities();
            _cardIds = cardIds;

            LoadEmployees();
            LoadDepartments();
        }

        private void LoadEmployees()
        {
            cbEmployee.ItemsSource = _context.Employees
                .Select(e => new
                {
                    e.EmployeeID,
                    FullName = e.LastName + " " + e.FirstName
                })
                .OrderBy(e => e.FullName)
                .ToList();
        }

        private void LoadDepartments()
        {
            cbDepartment.ItemsSource = _context.Departments
                .OrderBy(d => d.DepartmentName)
                .ToList();
        }

        private void cbDepartment_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbDepartment.SelectedValue == null) return;

            int depId = (int)cbDepartment.SelectedValue;

            cbRoom.ItemsSource = _context.Rooms
                .Where(r => r.DepartmentID == depId)
                .OrderBy(r => r.RoomNumber)
                .ToList();
        }

        private void BtnMove_Click(object sender, RoutedEventArgs e)
        {
            bool changeHolder = cbEmployee.SelectedValue != null;
            bool changeDepartment = cbDepartment.SelectedValue != null;
            bool changeRoom = cbRoom.SelectedValue != null;

            if (!changeHolder && !changeDepartment && !changeRoom)
            {
                MessageBox.Show("Выберите хотя бы одно поле для изменения.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int? newHolder = changeHolder ? (int)(cbEmployee.SelectedValue) : (int?)null;
            int? newDep = changeDepartment ? (int)(cbDepartment.SelectedValue) : (int?)null;
            int? newRoom = changeRoom ? (int)(cbRoom.SelectedValue) : (int?)null;

            foreach (int cardId in _cardIds)
            {
                var card = _context.AccountingCards.First(c => c.CardID == cardId);

                int nextSeq = _context.AssetMovements
                    .Where(m => m.CardID == cardId)
                    .Select(m => (int?)m.SequenceNumber)
                    .Max() ?? 0;

                nextSeq++;

                var movement = new AssetMovement
                {
                    CardID = cardId,
                    SequenceNumber = nextSeq,
                    MovementDate = DateTime.Now,
                    DepartmentID = newDep.GetValueOrDefault(card.DepartmentID),
                    RoomID = newRoom.HasValue ? newRoom.Value : card.RoomID,
                    HolderEmployeeID = newHolder.GetValueOrDefault(card.CurrentHolderID ?? 0),
                    TransferredByID = card.CurrentHolderID,
                    ConditionID = card.ConditionID,
                    Notes = "Выборочное перемещение"
                };

                _context.AssetMovements.Add(movement);

                // Обновляем карточку
                if (newHolder.HasValue)
                    card.CurrentHolderID = newHolder.Value;

                if (newDep.HasValue)
                    card.DepartmentID = newDep.Value;

                if (newRoom.HasValue)
                    card.RoomID = newRoom.Value;
            }

            _context.SaveChanges();

            MessageBox.Show("Перемещение выполнено.",
                "Готово", MessageBoxButton.OK, MessageBoxImage.Information);
        }

    }
}
