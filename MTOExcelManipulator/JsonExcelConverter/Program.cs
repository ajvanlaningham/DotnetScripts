using ClassLibrary.Services.Implementations;
using System.Text.Json;

namespace JsonExcelConverter
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("What file do you want to convert?");
            string filepath = Console.ReadLine()?.Trim('"'); // Remove surrounding quotes

            try
            {
                if (string.IsNullOrWhiteSpace(filepath))
                {
                    Console.WriteLine("No file path was provided.");
                    return;
                }

                if (!File.Exists(filepath))
                {
                    Console.WriteLine("The specified file does not exist.");
                    return;
                }

                string jsonContent = File.ReadAllText(filepath);

                var objects = JsonSerializer.Deserialize<List<Dictionary<string, object>>>(jsonContent);

                if (objects == null || objects.Count == 0)
                {
                    Console.WriteLine("The JSON file is empty or invalid.");
                    return;
                }

                ExcelWriterService service = new ExcelWriterService();

                string destinationPath = Path.GetDirectoryName(filepath);
                string destinationFile = Path.Combine(destinationPath, "JsonConvertedReport.xlsx");

                service.WriteExcelFile(destinationFile, objects);

                Console.WriteLine($"File converted successfully and saved to {destinationFile}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }
    }
}
