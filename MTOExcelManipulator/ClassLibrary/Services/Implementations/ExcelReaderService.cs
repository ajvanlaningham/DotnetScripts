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

                    Console.WriteLine($"Checking for column '{columnName}'...");
                    Console.WriteLine($"Available keys: {string.Join(", ", dataRow.Keys.Select(k => $"'{k}'"))}");

                    if (dataRow.ContainsKey(columnName))
                    {
                        var value = dataRow[columnName];
                        if (value != null && !string.IsNullOrWhiteSpace(value.ToString()))
                        {
                            try
                            {
                                //Console.WriteLine($"Column '{columnName}' found!");
                                var targetType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
                                var safeValue = Convert.ChangeType(value, targetType);
                                prop.SetValue(obj, safeValue);
                            }
                            catch (Exception ex)
                            {
                                //Console.WriteLine($"Column '{columnName}' NOT found.");
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

        public List<SKUTags> ReadSKUTags(string filePath)
        {
            var result = new List<SKUTags>();

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var package = new ExcelPackage(new FileInfo(filePath)))
            {
                var worksheet = package.Workbook.Worksheets.First(); 
                var rowCount = worksheet.Dimension.End.Row;
                var colCount = worksheet.Dimension.End.Column;

                for (int row = 2; row <= rowCount; row++)
                {
                    var sku = worksheet.Cells[row, 1].Text?.Trim(); // Column A (1)
                    var tags = new List<string>();

                    // Columns B (2) to AA (27)
                    for (int col = 2; col <= 27; col++)
                    {
                        var tag = worksheet.Cells[row, col].Text?.Trim();
                        if (!string.IsNullOrEmpty(tag))
                        {
                            tags.Add(tag);
                        }
                    }

                    result.Add(new SKUTags
                    {
                        SKU = sku,
                        Tags = string.Join(", ", tags)
                    });
                }
            }

            return result; 
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
            for (int col = 1; col <= worksheet.Dimension.End.Column; col++)
            {
                var cell = worksheet.Cells[1, col];
                headerRow.Add(cell.Text.Trim());
            }

            for (int rowNum = 2; rowNum <= worksheet.Dimension.End.Row; rowNum++)
            {
                var row = new Dictionary<string, object>();
                for (int col = 1; col <= worksheet.Dimension.End.Column; col++)
                {
                    var header = headerRow.ElementAtOrDefault(col - 1);
                    if (header != null)  // Check for null to avoid out-of-range exceptions
                    {
                        var cell = worksheet.Cells[rowNum, col];
                        row[header] = cell?.Text; 
                    }
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


        public class SKUTags
        {
            public string SKU { get; set; }
            public string Tags { get; set; }
        }

    }
}
