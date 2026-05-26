using System;
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

            var employees = _context.Employees
                .Select(e => new
                {
                    e.EmployeeID,
                    FullName = e.LastName + " " + e.FirstName +
                               (string.IsNullOrEmpty(e.MiddleName) ? "" : " " + e.MiddleName)
                })
                .OrderBy(e => e.FullName)
                .ToList();

            cbResponsible.ItemsSource = employees;
            cbHolder.ItemsSource = employees;
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
            cbCondition.SelectedValue = _card.ConditionID;
            cbDepartment.SelectedValue = _card.DepartmentID;

            LoadRooms(_card.DepartmentID);
            cbRoom.SelectedValue = _card.RoomID;

            cbResponsible.SelectedValue = _card.ResponsibleEmployeeID;
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

            _card.ModelID = (int)cbModel.SelectedValue;
            _card.AssetName = txtAssetName.Text.Trim();
            _card.InventoryNumber = txtInventoryNumber.Text.Trim();
            _card.SerialNumber = txtSerialNumber.Text.Trim();
            _card.ManufactureDate = dpManufactureDate.SelectedDate;
            _card.CommissionDate = dpCommissionDate.SelectedDate;
            _card.ConditionID = (int)cbCondition.SelectedValue;
            _card.DepartmentID = (int)cbDepartment.SelectedValue;
            _card.RoomID = cbRoom.SelectedValue as int?;
            _card.ResponsibleEmployeeID = (int)cbResponsible.SelectedValue;
            _card.CurrentHolderID = cbHolder.SelectedValue as int?;

            _context.SaveChanges();

            MessageBox.Show("Изменения сохранены.",
                            "Успех",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);

            LoadComponents(); // на случай, если что-то изменилось через движения
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
                // ОСНОВНЫЕ ДАННЫЕ
                // ───────────────────────────────────────────────
                Add("Номер карточки", _card.CardID.ToString());
                Add("Инвентарный номер", _card.InventoryNumber);
                Add("Название", _card.AssetName);
                Add("Тип", _card.AssetModel?.AssetType?.TypeName ?? "-");
                Add("Модель", _card.AssetModel?.ModelName ?? "-");
                Add("Серийный номер", _card.SerialNumber ?? "-");
                Add("Подразделение", _card.Department?.DepartmentName ?? "-");
                Add("Кабинет", _card.Room?.RoomNumber ?? "-");

                Add("Ответственный",
                    _card.Employee != null
                        ? $"{_card.Employee.LastName} {_card.Employee.FirstName}"
                        : "-");

                Add("Текущий держатель",
                    _card.Employee1 != null
                        ? $"{_card.Employee1.LastName} {_card.Employee1.FirstName}"
                        : "-");

                Add("Состояние", _card.AssetCondition?.ConditionName ?? "-");
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

                // Заголовки
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
                // ИСТОРИЯ ПЕРЕМЕЩЕНИЙ
                // ───────────────────────────────────────────────

                Word.Paragraph p2 = doc.Paragraphs.Add();
                p2.Range.Text = "\nИСТОРИЯ ПЕРЕМЕЩЕНИЙ";
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
                table.Cell(1, 5).Range.Text = "Держатель";
                table.Cell(1, 6).Range.Text = "Передал";
                table.Cell(1, 7).Range.Text = "Состояние";

                int row = 2;
                foreach (var m in movements)
                {
                    table.Cell(row, 1).Range.Text = m.SequenceNumber.ToString();
                    table.Cell(row, 2).Range.Text = m.MovementDate.ToShortDateString();
                    table.Cell(row, 3).Range.Text = m.Department?.DepartmentName ?? "-";
                    table.Cell(row, 4).Range.Text = m.Room?.RoomNumber ?? "-";

                    table.Cell(row, 5).Range.Text =
                        m.Employee != null
                            ? $"{m.Employee.LastName} {m.Employee.FirstName}"
                            : "-";

                    table.Cell(row, 6).Range.Text =
                        m.Employee1 != null
                            ? $"{m.Employee1.LastName} {m.Employee1.FirstName}"
                            : "-";

                    table.Cell(row, 7).Range.Text = m.AssetCondition?.ConditionName ?? "-";

                    row++;
                }

                // ───────────────────────────────────────────────
                // СОХРАНЕНИЕ
                // ───────────────────────────────────────────────

                string path = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                    $"Карточка_{_card.InventoryNumber}.docx");

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
