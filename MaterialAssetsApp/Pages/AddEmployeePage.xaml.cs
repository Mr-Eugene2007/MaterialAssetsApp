using System;
using System.Windows;
using System.Windows.Controls;

namespace MaterialAssetsApp.Pages
{
    public partial class AddEmployeePage : Page
    {
        private MaterialAssetsEntities _context;

        public AddEmployeePage()
        {
            InitializeComponent();
            _context = new MaterialAssetsEntities();
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Простая проверка обязательных полей
                if (string.IsNullOrWhiteSpace(txtLastName.Text))
                {
                    MessageBox.Show("Заполните фамилию.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(txtFirstName.Text))
                {
                    MessageBox.Show("Заполните имя.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!dpBirthDate.SelectedDate.HasValue)
                {
                    MessageBox.Show("Укажите дату рождения.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(txtSNILS.Text))
                {
                    MessageBox.Show("Укажите СНИЛС.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!dpHireDate.SelectedDate.HasValue)
                {
                    MessageBox.Show("Укажите дату приема на работу.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var employee = new Employee
                {
                    LastName = txtLastName.Text.Trim(),
                    FirstName = txtFirstName.Text.Trim(),
                    MiddleName = string.IsNullOrWhiteSpace(txtMiddleName.Text)
                                    ? null
                                    : txtMiddleName.Text.Trim(),
                    BirthDate = dpBirthDate.SelectedDate.Value,
                    SNILS = txtSNILS.Text.Trim(),   // Можно позже добавить проверку формата
                    INN = null,                   // Необязательное, можно добавить поле позже
                    PhoneMobile = string.IsNullOrWhiteSpace(txtPhoneMobile.Text)
                                    ? null
                                    : txtPhoneMobile.Text.Trim(),
                    PhoneWork = null,                  // Можно добавить поле в UI позже
                    Email = string.IsNullOrWhiteSpace(txtEmail.Text)
                                    ? null
                                    : txtEmail.Text.Trim(),
                    IsActive = true,
                    HireDate = dpHireDate.SelectedDate.Value,
                    DismissDate = null,
                    // PassportID обязателен по БД, поэтому пока можно:
                    // 1) либо сделать отдельную страницу для паспорта
                    // 2) либо временно использовать "заглушку" (например, паспорт "не указан")
                    // Здесь я предполагаю, что у тебя есть запись паспорта с ID = 1 как "заглушка".
                    PassportID = 1
                };

                _context.Employees.Add(employee);
                _context.SaveChanges();

                MessageBox.Show("Сотрудник успешно добавлен.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при сохранении сотрудника:\n" + ex.Message,
                                "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            txtLastName.Clear();
            txtFirstName.Clear();
            txtMiddleName.Clear();
            txtSNILS.Clear();
            txtPhoneMobile.Clear();
            txtEmail.Clear();
            dpBirthDate.SelectedDate = null;
            dpHireDate.SelectedDate = null;
        }
    }
}
