using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace MaterialAssetsApp.Pages
{
    public partial class AddDepartmentPage : Page
    {
        private MaterialAssetsEntities _context;

        public AddDepartmentPage()
        {
            InitializeComponent();
            _context = new MaterialAssetsEntities();
            LoadParentDepartments();
        }

        private void LoadParentDepartments()
        {
            // Подразделение может быть без родителя, поэтому ComboBox необязателен
            var departments = _context.Departments
                                      .OrderBy(d => d.DepartmentName)
                                      .ToList();

            cbParentDepartment.ItemsSource = departments;
            cbParentDepartment.SelectedIndex = -1;
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtDepartmentName.Text))
            {
                MessageBox.Show("Введите название подразделения.",
                                "Ошибка",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
                return;
            }

            try
            {
                var dep = new Department
                {
                    DepartmentName = txtDepartmentName.Text.Trim(),
                    ParentID = cbParentDepartment.SelectedItem is Department parent
                                ? (int?)parent.DepartmentID
                                : null
                };

                _context.Departments.Add(dep);
                _context.SaveChanges();

                MessageBox.Show("Подразделение успешно добавлено.",
                                "Успех",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);

                LoadParentDepartments(); // обновим список, чтобы новое тоже появилось
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Ошибка при сохранении подразделения:\n" + ex.Message,
                                "Ошибка",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
            }
        }

        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            txtDepartmentName.Clear();
            cbParentDepartment.SelectedIndex = -1;
        }
    }
}
