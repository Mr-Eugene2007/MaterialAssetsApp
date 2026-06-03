using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MaterialAssetsApp.Pages
{
    public partial class AddEmployeePage : Page
    {
        private readonly MaterialAssetsEntities _context;
        private Employee _selectedEmployee;

        public AddEmployeePage()
        {
            InitializeComponent();
            _context = new MaterialAssetsEntities();

            LoadEmployees();

            // Подписываемся на ввод СНИЛС
            txtSNILS.TextChanged += TxtSNILS_TextChanged;
            txtPhoneMobile.TextChanged += TxtPhoneMobile_TextChanged;


        }

        private void LoadEmployees()
        {
            dgEmployees.ItemsSource = _context.Employees
                .OrderBy(e => e.LastName)
                .ThenBy(e => e.FirstName)
                .ToList();
        }

        private void dgEmployees_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedEmployee = dgEmployees.SelectedItem as Employee;

            if (_selectedEmployee != null)
            {
                txtLastName.Text = _selectedEmployee.LastName;
                txtFirstName.Text = _selectedEmployee.FirstName;
                txtMiddleName.Text = _selectedEmployee.MiddleName;

                dpBirthDate.SelectedDate = _selectedEmployee.BirthDate;
                txtSNILS.Text = _selectedEmployee.SNILS;

                txtPhoneMobile.Text = _selectedEmployee.PhoneMobile;
                txtEmail.Text = _selectedEmployee.Email;

                dpHireDate.SelectedDate = _selectedEmployee.HireDate;
            }
        }

        private void BtnAddNew_Click(object sender, RoutedEventArgs e)
        {
            dgEmployees.SelectedItem = null;
            _selectedEmployee = null;

            txtLastName.Clear();
            txtFirstName.Clear();
            txtMiddleName.Clear();
            dpBirthDate.SelectedDate = null;
            txtSNILS.Clear();
            txtPhoneMobile.Clear();
            txtEmail.Clear();
            dpHireDate.SelectedDate = null;
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtLastName.Text) ||
                string.IsNullOrWhiteSpace(txtFirstName.Text) ||
                dpBirthDate.SelectedDate == null ||
                string.IsNullOrWhiteSpace(txtSNILS.Text) ||
                dpHireDate.SelectedDate == null)
            {
                MessageBox.Show("Заполните обязательные поля.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string snils = txtSNILS.Text.Trim();

            // Редактирование
            if (_selectedEmployee != null)
            {
                bool exists = _context.Employees.Any(emp => emp.SNILS == snils && emp.EmployeeID != _selectedEmployee.EmployeeID);
                if (exists)
                {
                    MessageBox.Show("Сотрудник с таким СНИЛС уже существует.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                _selectedEmployee.LastName = txtLastName.Text.Trim();
                _selectedEmployee.FirstName = txtFirstName.Text.Trim();
                _selectedEmployee.MiddleName = txtMiddleName.Text.Trim();
                _selectedEmployee.BirthDate = dpBirthDate.SelectedDate.Value;
                _selectedEmployee.SNILS = snils;
                _selectedEmployee.PhoneMobile = txtPhoneMobile.Text.Trim();
                _selectedEmployee.Email = txtEmail.Text.Trim();
                _selectedEmployee.HireDate = dpHireDate.SelectedDate.Value;
                _context.SaveChanges();
                LoadEmployees();
                MessageBox.Show("Сотрудник обновлён.");
                return;
            }

            // Добавление
            bool snilsExists = _context.Employees.Any(emp => emp.SNILS == snils);
            if (snilsExists)
            {
                MessageBox.Show("Сотрудник с таким СНИЛС уже существует.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var employee = new Employee
            {
                LastName = txtLastName.Text.Trim(),
                FirstName = txtFirstName.Text.Trim(),
                MiddleName = string.IsNullOrWhiteSpace(txtMiddleName.Text) ? null : txtMiddleName.Text.Trim(),
                BirthDate = dpBirthDate.SelectedDate.Value,
                SNILS = snils,
                PhoneMobile = string.IsNullOrWhiteSpace(txtPhoneMobile.Text) ? null : txtPhoneMobile.Text.Trim(),
                Email = string.IsNullOrWhiteSpace(txtEmail.Text) ? null : txtEmail.Text.Trim(),
                HireDate = dpHireDate.SelectedDate.Value,
                IsActive = true,
                PassportID = 1
            };

            _context.Employees.Add(employee);
            _context.SaveChanges();
            LoadEmployees();
            MessageBox.Show("Сотрудник добавлен.");
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedEmployee == null)
            {
                MessageBox.Show("Выберите сотрудника для удаления.");
                return;
            }

            if (MessageBox.Show("Удалить выбранного сотрудника?",
                                "Подтверждение",
                                MessageBoxButton.YesNo,
                                MessageBoxImage.Question) != MessageBoxResult.Yes)
                return;

            _context.Employees.Remove(_selectedEmployee);
            _context.SaveChanges();

            LoadEmployees();
            BtnAddNew_Click(null, null);

            MessageBox.Show("Сотрудник удалён.");
        }

        // ───────────────────────────────────────────────
        // АВТОФОРМАТИРОВАНИЕ СНИЛС
        // ───────────────────────────────────────────────
        private void TxtSNILS_TextChanged(object sender, TextChangedEventArgs e)
        {
            string digits = Regex.Replace(txtSNILS.Text, @"\D", ""); // оставляем только цифры

            if (digits.Length > 11)
                digits = digits.Substring(0, 11);

            string formatted = digits;

            if (digits.Length >= 3)
                formatted = digits.Insert(3, "-");

            if (digits.Length >= 6)
                formatted = formatted.Insert(7, "-");

            if (digits.Length >= 9)
                formatted = formatted.Insert(11, "-");

            if (digits.Length == 11)
                formatted = formatted.Insert(14, " ");

            txtSNILS.TextChanged -= TxtSNILS_TextChanged;
            txtSNILS.Text = formatted;
            txtSNILS.CaretIndex = txtSNILS.Text.Length;
            txtSNILS.TextChanged += TxtSNILS_TextChanged;
        }

        private void TxtPhoneMobile_TextChanged(object sender, TextChangedEventArgs e)
        {
            string digits = Regex.Replace(txtPhoneMobile.Text, @"\D", ""); // только цифры

            // Ограничиваем максимум 11 цифр (формат РФ)
            if (digits.Length > 11)
                digits = digits.Substring(0, 11);

            string formatted = digits;

            if (digits.StartsWith("8"))
                digits = "7" + digits.Substring(1);

            if (digits.Length >= 1)
                formatted = "+7";

            if (digits.Length >= 2)
                formatted += " (" + digits.Substring(1, Math.Min(3, digits.Length - 1));

            if (digits.Length >= 4)
                formatted += ") " + digits.Substring(4 - 1, Math.Min(3, digits.Length - 4 + 1));

            if (digits.Length >= 7)
                formatted += "-" + digits.Substring(7 - 1, Math.Min(2, digits.Length - 7 + 1));

            if (digits.Length >= 9)
                formatted += "-" + digits.Substring(9 - 1, Math.Min(2, digits.Length - 9 + 1));

            txtPhoneMobile.TextChanged -= TxtPhoneMobile_TextChanged;
            txtPhoneMobile.Text = formatted;
            txtPhoneMobile.CaretIndex = txtPhoneMobile.Text.Length;
            txtPhoneMobile.TextChanged += TxtPhoneMobile_TextChanged;
        }

    }
}
