using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace MaterialAssetsApp.Pages
{
    public partial class MassMovePage : Page
    {
        private readonly MaterialAssetsEntities _context;

        public MassMovePage()
        {
            InitializeComponent();
            _context = new MaterialAssetsEntities();

            LoadEmployees();
        }

        // ───────────────────────────────────────────────
        // ЗАГРУЗКА СОТРУДНИКОВ
        // ───────────────────────────────────────────────
        private void LoadEmployees()
        {
            var employees = _context.Employees
                .OrderBy(e => e.LastName)
                .Select(e => new
                {
                    e.EmployeeID,
                    FullName = e.LastName + " " + e.FirstName +
                               (string.IsNullOrEmpty(e.MiddleName) ? "" : " " + e.MiddleName)
                })
                .ToList();

            cbOldEmployee.ItemsSource = employees;
            cbNewEmployee.ItemsSource = employees;
        }

        // ───────────────────────────────────────────────
        // КНОПКА МАССОВОГО ПЕРЕМЕЩЕНИЯ
        // ───────────────────────────────────────────────
        private void BtnMassMove_Click(object sender, RoutedEventArgs e)
        {
            if (cbOldEmployee.SelectedValue == null || cbNewEmployee.SelectedValue == null)
            {
                MessageBox.Show("Выберите увольняемого и нового сотрудника.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int oldId = (int)cbOldEmployee.SelectedValue;
            int newId = (int)cbNewEmployee.SelectedValue;

            if (oldId == newId)
            {
                MessageBox.Show("Нельзя переместить карточки сотруднику самому себе.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            TransferAssets(oldId, newId);

            MessageBox.Show("Массовое перемещение выполнено.",
                "Готово", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // ───────────────────────────────────────────────
        // ЛОГИКА МАССОВОГО ПЕРЕМЕЩЕНИЯ
        // ───────────────────────────────────────────────
        private void TransferAssets(int oldEmployeeId, int newEmployeeId)
        {
            var cards = _context.AccountingCards
                .Where(c => c.CurrentHolderID == oldEmployeeId)
                .ToList();

            foreach (var card in cards)
            {
                // Определяем следующий порядковый номер движения
                int nextSeq = _context.AssetMovements
                    .Where(m => m.CardID == card.CardID)
                    .Select(m => (int?)m.SequenceNumber)
                    .Max() ?? 0;

                nextSeq++;

                // Создаём запись в истории движения
                var movement = new AssetMovement
                {
                    CardID = card.CardID,
                    SequenceNumber = nextSeq,
                    MovementDate = DateTime.Now,

                    DepartmentID = card.DepartmentID,
                    RoomID = card.RoomID,

                    HolderEmployeeID = newEmployeeId,   // новый держатель
                    TransferredByID = oldEmployeeId,    // кто передал

                    ConditionID = card.ConditionID,

                    Notes = "Автоматическое перемещение при увольнении сотрудника"
                };

                _context.AssetMovements.Add(movement);

                // Обновляем карточку
                card.CurrentHolderID = newEmployeeId;
            }

            _context.SaveChanges();
        }
    }
}
