using ClassLibrary.Services.Interfaces;
using OfficeOpenXml;
using OfficeOpenXml.Table;
using System.Reflection;

namespace ClassLibrary.Services.Implementations
{
    public class ExcelReaderService : IExcelReaderService
    {
        public List<T> MapDataToObjects<T>(IEnumerable<IDictionary<string, object>> data) where T : new()
        {
            var result = new List<T>();
            var properties = typeof(T).GetProperties();

            foreach (var dataRow in data)
            {
                var obj = new T();
                foreach (var prop in properties)
                {
                    var attribute = prop.GetCustomAttribute<ExcelColumnAttribute>();
                    var columnName = attribute?.Name ?? prop.Name;

                    if (dataRow.ContainsKey(columnName))
                    {
                        var value = dataRow[columnName];
                        if (value != null && !string.IsNullOrWhiteSpace(value.ToString()))
                        {
                            try
                            {
                                var targetType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
                                var safeValue = Convert.ChangeType(value, targetType);
                                prop.SetValue(obj, safeValue);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Error converting column '{columnName}' value '{value}' to type '{prop.PropertyType.Name}': {ex.Message}");
                            }
                        }
                    }
                }
                result.Add(obj);
            }

            return result;
        }

        public List<T> ReadExcelFile<T>(string filePath, string sheetName = null) where T : new()
        {
            var dataRows = ReadExcelToDataRows(filePath, sheetName);
            return MapDataToObjects<T>(dataRows);
        }

        private IEnumerable<IDictionary<string, object>> ReadExcelToDataRows(string filePath, string sheetName)
        {
            var dataRows = new List<IDictionary<string, object>>();

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var package = new ExcelPackage(new FileInfo(filePath));

            ExcelWorksheet worksheet = null;

            if (!string.IsNullOrEmpty(sheetName))
            {
                worksheet = package.Workbook.Worksheets.FirstOrDefault(ws => ws.Name.Equals(sheetName, StringComparison.OrdinalIgnoreCase));
                if (worksheet == null)
                    throw new ArgumentException($"Worksheet '{sheetName}' not found in the Excel file.");
            }
            else
            {
                worksheet = package.Workbook.Worksheets.FirstOrDefault();
                if (worksheet == null)
                    throw new InvalidOperationException("No worksheets found in the Excel file.");
            }

            var headerRow = new List<string>();
            foreach (var cell in worksheet.Cells[1, 1, 1, worksheet.Dimension.End.Column])
            {
                headerRow.Add(cell.Text);
            }

            for (int rowNum = 2; rowNum <= worksheet.Dimension.End.Row; rowNum++)
            {
                var row = new Dictionary<string, object>();
                for (int col = 1; col < worksheet.Dimension.End.Column; col++)
                {
                    var header = headerRow[col - 1];
                    var cell = worksheet.Cells[rowNum, col];
                    row[header] = cell.Value;
                }
                dataRows.Add(row);
            }

            return dataRows;
        }

        [AttributeUsage(AttributeTargets.Property)]
        public class ExcelColumnAttribute : Attribute
        {
            public string Name { get; set; }
            public ExcelColumnAttribute(string name) => Name = name;
        }

    }
}
