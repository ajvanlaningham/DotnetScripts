using ClassLibrary.Services.Interfaces;
using OfficeOpenXml;
using System.Reflection;
using static ClassLibrary.Services.Implementations.ExcelReaderService;

namespace ClassLibrary.Services.Implementations
{
    public class ExcelWriterService : IExcelWriterService
    {
        public void WriteExcelFile<T>(string filePath, IEnumerable<T> data, string sheetName = "Sheet1")
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data), "Data cannot be null.");

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var package = new ExcelPackage();

            var worksheet = package.Workbook.Worksheets.Add(sheetName);

            var properties = typeof(T).GetProperties();

            var headers = properties.Select(prop =>
            {
                var attribute = prop.GetCustomAttribute<ExcelColumnAttribute>();
                return attribute?.Name ?? prop.Name;
            }).ToList();

            for (int col = 1; col <= headers.Count; col++)
            {
                worksheet.Cells[1, col].Value = headers[col - 1];
                worksheet.Cells[1, col].Style.Font.Bold = true;
            }

            int row = 2;
            foreach (var item in data)
            {
                for (int col = 1; col <= properties.Length; col++)
                {
                    var prop = properties[col - 1];
                    var value = prop.GetValue(item, null);

                    worksheet.Cells[row, col].Value = value;
                }
                row++;
            }

            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

            var directory = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var fi = new FileInfo(filePath);
            package.SaveAs(fi);
        }

        public void WriteExcelFile(string filePath, List<Dictionary<string, object>> data, string sheetName = "Sheet1")
        {
            if (data == null || data.Count == 0)
                throw new ArgumentException("Data cannot be null or empty.", nameof(data));

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var package = new ExcelPackage();

            var worksheet = package.Workbook.Worksheets.Add(sheetName);

            var headers = data.SelectMany(dict => dict.Keys).Distinct().ToList();

            for (int col = 0; col < headers.Count; col++)
            {
                worksheet.Cells[1, col + 1].Value = headers[col];
                worksheet.Cells[1, col + 1].Style.Font.Bold = true;
            }

            for (int row = 0; row < data.Count; row++)
            {
                var rowData = data[row];
                for (int col = 0; col < headers.Count; col++)
                {
                    string header = headers[col];
                    if (rowData.TryGetValue(header, out var value))
                    {
                        worksheet.Cells[row + 2, col + 1].Value = value;
                    }
                }
            }

            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

            var directory = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var fileInfo = new FileInfo(filePath);
            package.SaveAs(fileInfo);
        }

        public void WriteSheet(string filePath, string sheetName, List<Dictionary<string, object>> data)
        {
            if (data == null || data.Count == 0)
                throw new ArgumentException("Data cannot be null or empty.", nameof(data));

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            FileInfo fileInfo = new FileInfo(filePath);
            bool fileExists = fileInfo.Exists;

            using var package = fileExists ? new ExcelPackage(fileInfo) : new ExcelPackage();

            string finalSheetName = sheetName;
            int sheetIndex = 1;
            while (package.Workbook.Worksheets.Any(ws => ws.Name == finalSheetName))
            {
                finalSheetName = $"{sheetName}_{sheetIndex}";
                sheetIndex++;
            }

            var worksheet = package.Workbook.Worksheets.Add(finalSheetName);

            var headers = data.SelectMany(dict => dict.Keys).Distinct().ToList();

            for (int col = 0; col < headers.Count; col++)
            {
                worksheet.Cells[1, col + 1].Value = headers[col];
                worksheet.Cells[1, col + 1].Style.Font.Bold = true;
            }

            for (int row = 0; row < data.Count; row++)
            {
                var rowData = data[row];
                for (int col = 0; col < headers.Count; col++)
                {
                    string header = headers[col];
                    if (rowData.TryGetValue(header, out var value))
                    {
                        worksheet.Cells[row + 2, col + 1].Value = value;
                    }
                }
            }

            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

            var directory = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            package.SaveAs(fileInfo);
        }
    }
}
