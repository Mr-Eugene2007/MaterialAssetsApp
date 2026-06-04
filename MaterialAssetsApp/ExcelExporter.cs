using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using ClosedXML.Excel;

namespace MaterialAssetsApp
{
    public static class ExcelExporter
    {
        public static void Export(System.Windows.Controls.DataGrid grid, string sheetName)
        {
            if (grid.ItemsSource == null)
            {
                MessageBox.Show("Нет данных для экспорта.");
                return;
            }

            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "Excel файл (*.xlsx)|*.xlsx",
                FileName = sheetName,
                DefaultExt = ".xlsx"
            };

            if (dialog.ShowDialog() != true)
                return;

            try
            {
                using (var wb = new XLWorkbook())
                {
                    var ws = wb.Worksheets.Add(sheetName);

                    // Заголовки из колонок DataGrid
                    var columns = grid.Columns
                        .Where(c => c is System.Windows.Controls.DataGridTextColumn)
                        .Cast<System.Windows.Controls.DataGridTextColumn>()
                        .ToList();

                    for (int i = 0; i < columns.Count; i++)
                    {
                        var header = columns[i].Header?.ToString() ?? "";
                        ws.Cell(1, i + 1).Value = header;
                        ws.Cell(1, i + 1).Style.Font.Bold = true;
                        ws.Cell(1, i + 1).Style.Fill.BackgroundColor = XLColor.LightGray;
                    }

                    // Данные
                    var items = grid.ItemsSource.Cast<object>().ToList();
                    for (int row = 0; row < items.Count; row++)
                    {
                        var item = items[row];
                        for (int col = 0; col < columns.Count; col++)
                        {
                            var binding = columns[col].Binding as System.Windows.Data.Binding;
                            if (binding == null) continue;

                            var value = item.GetType()
                                .GetProperty(binding.Path.Path)?
                                .GetValue(item)?.ToString() ?? "";

                            ws.Cell(row + 2, col + 1).Value = value;
                        }
                    }

                    ws.Columns().AdjustToContents();
                    wb.SaveAs(dialog.FileName);
                }

                MessageBox.Show("Файл сохранён:\n" + dialog.FileName,
                    "Готово", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Ошибка экспорта:\n" + ex.Message,
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}