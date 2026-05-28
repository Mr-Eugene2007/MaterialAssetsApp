using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace MaterialAssetsApp.Pages
{
    public partial class AddDepartmentPage : Page
    {
        private readonly MaterialAssetsEntities _context;
        private Department _selectedDepartment;

        public AddDepartmentPage()
        {
            InitializeComponent();
            _context = new MaterialAssetsEntities();

            LoadDepartments();
            LoadParentDepartments();
        }

        private void LoadDepartments()
        {
            dgDepartments.ItemsSource = _context.Departments
                .OrderBy(d => d.DepartmentName)
                .ToList();
        }

        private void LoadParentDepartments()
        {
            cbParentDepartment.ItemsSource = _context.Departments
                .OrderBy(d => d.DepartmentName)
                .ToList();
        }

        private void dgDepartments_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedDepartment = dgDepartments.SelectedItem as Department;

            if (_selectedDepartment != null)
            {
                txtDepartmentName.Text = _selectedDepartment.DepartmentName;
                cbParentDepartment.SelectedValue = _selectedDepartment.ParentID;
            }
        }

        private void BtnAddNew_Click(object sender, RoutedEventArgs e)
        {
            dgDepartments.SelectedItem = null;
            _selectedDepartment = null;

            txtDepartmentName.Clear();
            cbParentDepartment.SelectedIndex = -1;
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtDepartmentName.Text))
            {
                MessageBox.Show("Введите название подразделения.");
                return;
            }

            // Редактирование
            if (_selectedDepartment != null)
            {
                _selectedDepartment.DepartmentName = txtDepartmentName.Text.Trim();
                _selectedDepartment.ParentID = cbParentDepartment.SelectedValue as int?;

                _context.SaveChanges();
                LoadDepartments();

                MessageBox.Show("Подразделение обновлено.");
                return;
            }

            // Добавление
            var newDep = new Department
            {
                DepartmentName = txtDepartmentName.Text.Trim(),
                ParentID = cbParentDepartment.SelectedValue as int?
            };

            _context.Departments.Add(newDep);
            _context.SaveChanges();

            LoadDepartments();
            MessageBox.Show("Подразделение добавлено.");
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedDepartment == null)
            {
                MessageBox.Show("Выберите подразделение для удаления.");
                return;
            }

            if (MessageBox.Show("Удалить выбранное подразделение?",
                                "Подтверждение",
                                MessageBoxButton.YesNo,
                                MessageBoxImage.Question) != MessageBoxResult.Yes)
                return;

            _context.Departments.Remove(_selectedDepartment);
            _context.SaveChanges();

            LoadDepartments();

            txtDepartmentName.Clear();
            cbParentDepartment.SelectedIndex = -1;

            _selectedDepartment = null;

            MessageBox.Show("Подразделение удалено.");
        }
    }
}
