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
    }
}
