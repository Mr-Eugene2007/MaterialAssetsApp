using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Word = Microsoft.Office.Interop.Word;

namespace MaterialAssetsApp.Pages
{
    public partial class EditAccountingCardPage : Page
    {
        private readonly MaterialAssetsEntities _context;
        private AccountingCard _card;
        private List<dynamic> _allEmployees;

        public EditAccountingCardPage(int cardId)
        {
            InitializeComponent();
            _context = new MaterialAssetsEntities();

            LoadLists();
            LoadCard(cardId);
            LoadComponents();
        }

        private void LoadLists()
        {
            cbModel.ItemsSource = _context.AssetModels
                .OrderBy(m => m.ModelName)
                .ToList();

            cbCondition.ItemsSource = _context.AssetConditions
                .OrderBy(c => c.ConditionName)
                .ToList();

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

            cbResponsible.ItemsSource = _allEmployees;
            cbHolder.ItemsSource = _allEmployees;
        }

        private void txtSearchResponsible_TextChanged(object sender, TextChangedEventArgs e)
        {
            FilterEmployees(txtSearchResponsible.Text, cbResponsible);
        }

        private void txtSearchHolder_TextChanged(object sender, TextChangedEventArgs e)
        {
            FilterEmployees(txtSearchHolder.Text, cbHolder);
        }

        private void cbResponsible_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbResponsible.SelectedValue != null)
            {
                txtSearchResponsible.TextChanged -= txtSearchResponsible_TextChanged;
                txtSearchResponsible.Clear();
                txtSearchResponsible.TextChanged += txtSearchResponsible_TextChanged;
                cbResponsible.ItemsSource = _allEmployees;
            }
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

        private void LoadCard(int id)
        {
            _card = _context.AccountingCards.First(c => c.CardID == id);

            cbModel.SelectedValue = _card.ModelID;
            txtAssetName.Text = _card.AssetName;
            txtInventoryNumber.Text = _card.InventoryNumber;
            txtSerialNumber.Text = _card.SerialNumber;
            dpManufactureDate.SelectedDate = _card.ManufactureDate;
            dpCommissionDate.SelectedDate = _card.CommissionDate;

            dpDecommissionDate.SelectedDate = _card.DecommissionDate;  

            cbCondition.SelectedValue = _card.ConditionID;
            cbDepartment.SelectedValue = _card.DepartmentID;

            LoadRooms(_card.DepartmentID);
            cbRoom.SelectedValue = _card.RoomID;

            cbResponsible.SelectedValue = CurrentSession.EmployeeID;
            cbHolder.SelectedValue = _card.CurrentHolderID;
        }


        private void LoadRooms(int departmentId)
        {
            cbRoom.ItemsSource = _context.Rooms
                .Where(r => r.DepartmentID == departmentId)
                .OrderBy(r => r.RoomNumber)
                .ToList();
        }

        private void cbDepartment_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbDepartment.SelectedValue == null)
                return;

            int depId = (int)cbDepartment.SelectedValue;
            LoadRooms(depId);
        }

        // ---------------- Комплектующие ----------------

        private void LoadComponents()
        {
            if (_card == null)
                return;

            var components = _context.AssetComponents
                .Where(c => c.CardID == _card.CardID)
                .ToList();

            dgComponents.ItemsSource = components;
        }

        private void BtnAddComponent_Click(object sender, RoutedEventArgs e)
        {
            ((MainWindow)Application.Current.MainWindow)
                .MainFrame.Navigate(new AddComponentPage(_card.CardID, LoadComponents));
        }

        private void BtnDeleteComponent_Click(object sender, RoutedEventArgs e)
        {
            if (dgComponents.SelectedItem == null)
            {
                MessageBox.Show("Выберите компонент.");
                return;
            }

            var comp = dgComponents.SelectedItem as AssetComponent;
            if (comp == null)
                return;

            if (MessageBox.Show("Удалить компонент?", "Подтверждение",
                    MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                return;

            _context.AssetComponents.Remove(comp);
            _context.SaveChanges();
            LoadComponents();
        }

        private void dgComponents_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (dgComponents.SelectedItem == null)
                return;

            var comp = dgComponents.SelectedItem as AssetComponent;
            if (comp == null)
                return;

            ((MainWindow)Application.Current.MainWindow)
                .MainFrame.Navigate(new EditComponentPage(comp.ComponentID));
        }

        // ---------------- Основная карточка ----------------

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (_card == null)
                return;

            int? newHolderId = cbHolder.SelectedValue as int?;
            int? newDepId = cbDepartment.SelectedValue as int?;
            int? newRoomId = cbRoom.SelectedValue as int?;
            int? newConditionId = cbCondition.SelectedValue as int?;

            bool holderChanged = newHolderId != _card.CurrentHolderID;
            bool depChanged = newDepId != (int?)_card.DepartmentID;
            bool roomChanged = newRoomId != _card.RoomID;
            bool conditionChanged = newConditionId != (int?)_card.ConditionID;

            // Сохраняем старого держателя ДО изменений
            int? oldHolderId = _card.CurrentHolderID;

            _card.ModelID = (int)cbModel.SelectedValue;
            _card.AssetName = txtAssetName.Text.Trim();
            _card.InventoryNumber = txtInventoryNumber.Text.Trim();
            _card.SerialNumber = txtSerialNumber.Text.Trim();
            _card.ManufactureDate = dpManufactureDate.SelectedDate;
            _card.CommissionDate = dpCommissionDate.SelectedDate;
            _card.DecommissionDate = dpDecommissionDate.SelectedDate;
            _card.ConditionID = newConditionId ?? _card.ConditionID;
            _card.DepartmentID = newDepId ?? _card.DepartmentID;
            _card.RoomID = newRoomId;
            _card.ResponsibleEmployeeID = CurrentSession.EmployeeID;
            _card.CurrentHolderID = newHolderId;

            if (holderChanged || depChanged || roomChanged || conditionChanged)
            {
                int nextSeq = _context.AssetMovements
                    .Where(m => m.CardID == _card.CardID)
                    .Select(m => (int?)m.SequenceNumber)
                    .Max() ?? 0;
                nextSeq++;

                var movement = new AssetMovement
                {
                    CardID = _card.CardID,
                    SequenceNumber = nextSeq,
                    MovementDate = DateTime.Now,
                    DepartmentID = _card.DepartmentID,
                    RoomID = _card.RoomID,
                    HolderEmployeeID = newHolderId ?? oldHolderId ?? 0, // используем oldHolderId
                    TransferredByID = oldHolderId, // кто передал — старый держатель
                    ConditionID = _card.ConditionID,
                    Notes = "Изменение карточки"
                };

                _context.AssetMovements.Add(movement);
            }

            _context.SaveChanges();

            MessageBox.Show("Изменения сохранены.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            LoadComponents();
        }


        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.GoBack();
        }

        private void BtnViewMovements_Click(object sender, RoutedEventArgs e)
        {
            if (_card == null)
                return;

            ((MainWindow)Application.Current.MainWindow)
                .MainFrame.Navigate(new MovementHistoryPage(_card.CardID));
        }

        private void BtnPrint_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // ───────────────────────────────────────────────
                // ЗАГРУЗКА ДАННЫХ
                // ───────────────────────────────────────────────

                var movements = _context.AssetMovements
                    .Where(m => m.CardID == _card.CardID)
                    .OrderBy(m => m.SequenceNumber)
                    .ToList();

                var components = _context.AssetComponents
                    .Where(c => c.CardID == _card.CardID)
                    .ToList();

                // ───────────────────────────────────────────────
                // WORD
                // ───────────────────────────────────────────────

                var word = new Word.Application();
                word.Visible = false;

                var doc = word.Documents.Add();

                // ───────────────────────────────────────────────
                // ЗАГОЛОВОК
                // ───────────────────────────────────────────────
                Word.Paragraph p = doc.Paragraphs.Add();
                p.Range.Text = "УЧЁТНАЯ КАРТОЧКА ТЕХНИЧЕСКОГО СРЕДСТВА";
                p.Range.Font.Size = 20;
                p.Range.Font.Bold = 1;
                p.Range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                p.Range.InsertParagraphAfter();

                // Метод для вывода строки
                void Add(string title, string value)
                {
                    Word.Paragraph pr = doc.Paragraphs.Add();
                    pr.Range.Font.Size = 12;
                    pr.Range.Font.Bold = 0;
                    pr.Range.Text = $"{title}: {value}";
                    pr.Range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphLeft;
                    pr.Range.InsertParagraphAfter();
                }

                // ───────────────────────────────────────────────
                // ОСНОВНЫЕ ДАННЫЕ (исправлено)
                // ───────────────────────────────────────────────
                Add("Номер карточки", _card.CardID.ToString());
                Add("Инвентарный номер", _card.InventoryNumber);
                Add("Название", _card.AssetName);
                Add("Тип", _card.AssetModel?.AssetType?.TypeName ?? "-");
                Add("Модель", _card.AssetModel?.ModelName ?? "-");
                Add("Серийный номер", _card.SerialNumber ?? "-");

                Add("Ответственный",
                _card.Employee != null
                    ? $"{_card.Employee.LastName} {_card.Employee.FirstName} {_card.Employee.MiddleName}".Trim()
                    : "-");

                Add("Текущий держатель",
                    _card.Employee1 != null
                        ? $"{_card.Employee1.LastName} {_card.Employee1.FirstName} {_card.Employee1.MiddleName}".Trim()
                        : "-");

                Add("Дата выпуска", _card.ManufactureDate?.ToShortDateString() ?? "-");
                Add("Дата ввода в эксплуатацию", _card.CommissionDate?.ToShortDateString() ?? "-");
                Add("Дата списания", _card.DecommissionDate?.ToShortDateString() ?? "-");

                // ───────────────────────────────────────────────
                // КОМПЛЕКТУЮЩИЕ
                // ───────────────────────────────────────────────

                Word.Paragraph pComp = doc.Paragraphs.Add();
                pComp.Range.Text = "\nКОМПЛЕКТУЮЩИЕ";
                pComp.Range.Font.Size = 16;
                pComp.Range.Font.Bold = 1;
                pComp.Range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                pComp.Range.InsertParagraphAfter();

                Word.Table compTable = doc.Tables.Add(pComp.Range, components.Count + 1, 4);
                compTable.Range.Font.Size = 11;
                compTable.Borders.Enable = 1;
                compTable.Rows.Alignment = Word.WdRowAlignment.wdAlignRowCenter;

                compTable.Cell(1, 1).Range.Text = "Название";
                compTable.Cell(1, 2).Range.Text = "Номер";
                compTable.Cell(1, 3).Range.Text = "Кол-во";
                compTable.Cell(1, 4).Range.Text = "Примечание";

                int r = 2;
                foreach (var c in components)
                {
                    compTable.Cell(r, 1).Range.Text = c.ComponentName;
                    compTable.Cell(r, 2).Range.Text = c.ComponentNumber ?? "-";
                    compTable.Cell(r, 3).Range.Text = c.Quantity.ToString();
                    compTable.Cell(r, 4).Range.Text = c.Notes ?? "-";
                    r++;
                }

                // ───────────────────────────────────────────────
                // ИСТОРИЯ ПЕРЕМЕЩЕНИЙ — НОВАЯ СТРАНИЦА
                // ───────────────────────────────────────────────

                doc.Words.Last.InsertBreak(Word.WdBreakType.wdPageBreak);

                Word.Paragraph p2 = doc.Paragraphs.Add();
                p2.Range.Text = "ИСТОРИЯ ПЕРЕМЕЩЕНИЙ";
                p2.Range.Font.Size = 16;
                p2.Range.Font.Bold = 1;
                p2.Range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                p2.Range.InsertParagraphAfter();

                Word.Table table = doc.Tables.Add(p2.Range, movements.Count + 1, 7);
                table.Range.Font.Size = 11;
                table.Borders.Enable = 1;
                table.Rows.Alignment = Word.WdRowAlignment.wdAlignRowCenter;

                table.Cell(1, 1).Range.Text = "№";
                table.Cell(1, 2).Range.Text = "Дата";
                table.Cell(1, 3).Range.Text = "Подразделение";
                table.Cell(1, 4).Range.Text = "Кабинет";
                table.Cell(1, 5).Range.Text = "Получатель";
                table.Cell(1, 6).Range.Text = "Кто передал";
                table.Cell(1, 7).Range.Text = "Подпись";

                int row = 2;
                foreach (var m in movements)
                {
                    table.Cell(row, 1).Range.Text = m.SequenceNumber.ToString();
                    table.Cell(row, 2).Range.Text = m.MovementDate.ToShortDateString();
                    table.Cell(row, 3).Range.Text = m.Department?.DepartmentName ?? "-";
                    table.Cell(row, 4).Range.Text = m.Room?.RoomNumber ?? "-";

                    // Получатель (держатель)
                    table.Cell(row, 5).Range.Text =
                        m.Employee != null
                            ? $"{m.Employee.LastName} {m.Employee.FirstName} {m.Employee.MiddleName}".Trim()
                            : "-";

                    // Кто передал
                    table.Cell(row, 6).Range.Text =
                        m.Employee1 != null
                            ? $"{m.Employee1.LastName} {m.Employee1.FirstName} {m.Employee1.MiddleName}".Trim()
                            : "-";

                    // Подпись — пустая
                    table.Cell(row, 7).Range.Text = "";

                    row++;
                }

                // ───────────────────────────────────────────────
                // СОХРАНЕНИЕ
                // ───────────────────────────────────────────────

                string folder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                "Учётные карточки");

                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);

                string path = Path.Combine(folder, $"Карточка_{_card.InventoryNumber}.docx");

                doc.SaveAs2(path);
                doc.Close();
                word.Quit();

                MessageBox.Show("Файл сохранён на рабочем столе:\n" + path,
                                "Готово", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка печати:\n" + ex.Message);
            }
        }


    }
}
