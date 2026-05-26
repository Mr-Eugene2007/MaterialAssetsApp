using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Word = Microsoft.Office.Interop.Word;

namespace MaterialAssetsApp.Pages
{
    public partial class EditAccountingCardPage : Page
    {
        private MaterialAssetsEntities _context;
        private AccountingCard _card;

        public EditAccountingCardPage(int cardId)
        {
            InitializeComponent();
            _context = new MaterialAssetsEntities();

            LoadLists();
            LoadCard(cardId);
        }

        private void LoadLists()
        {
            cbModel.ItemsSource = _context.AssetModels.OrderBy(m => m.ModelName).ToList();
            cbCondition.ItemsSource = _context.AssetConditions.OrderBy(c => c.ConditionName).ToList();
            cbDepartment.ItemsSource = _context.Departments.OrderBy(d => d.DepartmentName).ToList();

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

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
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
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }

        private void BtnViewMovements_Click(object sender, RoutedEventArgs e)
        {
            ((MainWindow)Application.Current.MainWindow)
                .MainFrame.Navigate(new MovementHistoryPage(_card.CardID));
        }


        private void BtnPrint_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Загружаем историю перемещений
                var movements = _context.AssetMovements
                    .Where(m => m.CardID == _card.CardID)
                    .OrderBy(m => m.SequenceNumber)
                    .Select(m => new
                    {
                        m.SequenceNumber,
                        m.MovementDate,
                        Department = m.Department.DepartmentName,
                        Room = m.Room != null ? m.Room.RoomNumber : "",
                        Holder = m.Employee.LastName + " " + m.Employee.FirstName +
                                 (string.IsNullOrEmpty(m.Employee.MiddleName) ? "" : " " + m.Employee.MiddleName),
                        TransferredBy = m.Employee1 != null
                            ? m.Employee1.LastName + " " + m.Employee1.FirstName +
                              (string.IsNullOrEmpty(m.Employee1.MiddleName) ? "" : " " + m.Employee1.MiddleName)
                            : "",
                        Condition = m.AssetCondition.ConditionName,
                        m.Notes
                    })
                    .ToList();

                // Создаём Word
                var word = new Word.Application();
                word.Visible = false;

                var doc = word.Documents.Add();

                // ───────────────────────────────────────────────
                // Заголовок — по центру
                // ───────────────────────────────────────────────
                Word.Paragraph p = doc.Paragraphs.Add();
                p.Range.Text = "УЧЁТНАЯ КАРТОЧКА ТЕХНИЧЕСКОГО СРЕДСТВА";
                p.Range.Font.Size = 20;
                p.Range.Font.Bold = 1;
                p.Range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                p.Range.InsertParagraphAfter();

                // ───────────────────────────────────────────────
                // Метод Add — всё слева
                // ───────────────────────────────────────────────
                void Add(string title, string value)
                {
                    Word.Paragraph pr = doc.Paragraphs.Add();
                    pr.Range.Font.Size = 12;
                    pr.Range.Font.Bold = 0;
                    pr.Range.Text = $"{title}: {value}";
                    pr.Range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphLeft;
                    pr.Range.InsertParagraphAfter();
                }

                // Основные данные
                Add("Номер карточки", _card.CardID.ToString());
                Add("Инвентарный номер", _card.InventoryNumber);
                Add("Название", _card.AssetName);
                Add("Тип", _card.AssetModel.AssetType.TypeName);
                Add("Модель", _card.AssetModel.ModelName);
                Add("Серийный номер", _card.SerialNumber ?? "-");
                Add("Подразделение", _card.Department.DepartmentName);
                Add("Кабинет", _card.Room?.RoomNumber ?? "-");
                Add("Ответственный", _card.Employee.LastName + " " + _card.Employee.FirstName);
                Add("Текущий держатель", _card.Employee1 != null
                    ? _card.Employee1.LastName + " " + _card.Employee1.FirstName
                    : "-");
                Add("Состояние", _card.AssetCondition.ConditionName);
                Add("Дата выпуска", _card.ManufactureDate?.ToShortDateString() ?? "-");
                Add("Дата ввода в эксплуатацию", _card.CommissionDate?.ToShortDateString() ?? "-");
                Add("Дата списания", _card.DecommissionDate?.ToShortDateString() ?? "-");

                // ───────────────────────────────────────────────
                // Заголовок таблицы — по центру
                // ───────────────────────────────────────────────
                Word.Paragraph p2 = doc.Paragraphs.Add();
                p2.Range.Text = "\nИСТОРИЯ ПЕРЕМЕЩЕНИЙ";
                p2.Range.Font.Size = 16;
                p2.Range.Font.Bold = 1;
                p2.Range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                p2.Range.InsertParagraphAfter();

                // ───────────────────────────────────────────────
                // Таблица — по центру
                // ───────────────────────────────────────────────
                Word.Table table = doc.Tables.Add(p2.Range, movements.Count + 1, 7);
                table.Range.Font.Size = 11;
                table.Borders.Enable = 1;

                // Центрируем таблицу
                table.Rows.Alignment = Word.WdRowAlignment.wdAlignRowCenter;

                // Заголовки
                table.Cell(1, 1).Range.Text = "№";
                table.Cell(1, 2).Range.Text = "Дата";
                table.Cell(1, 3).Range.Text = "Подразделение";
                table.Cell(1, 4).Range.Text = "Кабинет";
                table.Cell(1, 5).Range.Text = "Держатель";
                table.Cell(1, 6).Range.Text = "Передал";
                table.Cell(1, 7).Range.Text = "Состояние";

                // Заполнение строк
                int row = 2;
                foreach (var m in movements)
                {
                    table.Cell(row, 1).Range.Text = m.SequenceNumber.ToString();
                    table.Cell(row, 2).Range.Text = m.MovementDate.ToShortDateString();
                    table.Cell(row, 3).Range.Text = m.Department;
                    table.Cell(row, 4).Range.Text = m.Room;
                    table.Cell(row, 5).Range.Text = m.Holder;
                    table.Cell(row, 6).Range.Text = m.TransferredBy;
                    table.Cell(row, 7).Range.Text = m.Condition;
                    row++;
                }

                // Сохранение
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
