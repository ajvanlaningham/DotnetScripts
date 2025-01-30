using ClassLibrary.Services.Implementations;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace JsonExcelConverter
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Enter the file or folder path to convert:");
            string path = Console.ReadLine()?.Trim('"'); // Remove surrounding quotes

            try
            {
                if (string.IsNullOrWhiteSpace(path))
                {
                    Console.WriteLine("No file or folder path was provided.");
                    return;
                }

                if (File.Exists(path))
                {
                    ConvertJsonFile(path);
                }
                else if (Directory.Exists(path))
                {
                    string[] jsonFiles = Directory.GetFiles(path, "*.json", SearchOption.TopDirectoryOnly);

                    if (jsonFiles.Length == 0)
                    {
                        Console.WriteLine("No JSON files found in the specified folder.");
                        return;
                    }

                    foreach (string file in jsonFiles)
                    {
                        ConvertJsonFile(file);
                    }
                }
                else
                {
                    Console.WriteLine("The specified path does not exist.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        static void ConvertJsonFile(string filepath)
        {
            try
            {
                Console.WriteLine($"Processing file: {filepath}");

                string jsonContent = File.ReadAllText(filepath);
                JsonNode? rootNode = JsonNode.Parse(jsonContent);

                if (rootNode == null)
                {
                    Console.WriteLine($"The JSON file '{filepath}' is empty or invalid.");
                    return;
                }

                ExcelWriterService service = new ExcelWriterService();
                string destinationPath = Path.GetDirectoryName(filepath) ?? string.Empty;
                string finalFileName = Path.GetFileNameWithoutExtension(filepath);
                string destinationFile = Path.Combine(destinationPath, $"{finalFileName}Converted.xlsx");

                if (rootNode is JsonObject rootObject)
                {
                    foreach (var property in rootObject)
                    {
                        string sheetName = property.Key;
                        var sheetData = ExtractSheetData(property.Value);

                        if (sheetData.Count > 0)
                        {
                            service.WriteSheet(destinationFile, sheetName, sheetData);
                        }
                    }
                }
                else if (rootNode is JsonArray jsonArray)
                {
                    string sheetName = "RootArray";
                    var sheetData = ExtractSheetData(rootNode);

                    if (sheetData.Count > 0)
                    {
                        service.WriteSheet(destinationFile, sheetName, sheetData);
                    }
                }

                Console.WriteLine($"File converted successfully and saved to {destinationFile}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while processing '{filepath}': {ex.Message}");
            }
        }

        static List<Dictionary<string, object>> ExtractSheetData(JsonNode? node)
        {
            var data = new List<Dictionary<string, object>>();

            try
            {
                if (node is JsonArray arrayNode)
                {
                    foreach (JsonNode? item in arrayNode)
                    {
                        var row = new Dictionary<string, object>();
                        PopulateRow(item, row);
                        data.Add(row);
                    }
                }
                else if (node is JsonObject objNode)
                {
                    bool listFound = false;
                    foreach (var property in objNode)
                    {
                        try
                        {
                            if (property.Value is JsonArray nestedArray)
                            {
                                listFound = true;
                                foreach (JsonNode? arrayItem in nestedArray)
                                {
                                    var row = new Dictionary<string, object>();
                                    PopulateRow(arrayItem, row);
                                    data.Add(row);
                                }
                            }
                            else if (property.Value is JsonValue valueNode && valueNode.TryGetValue<string>(out string? stringValue))
                            {
                                JsonNode? parsedNode = TryParseJson(stringValue);
                                if (parsedNode is JsonArray parsedArray)
                                {
                                    listFound = true;
                                    foreach (var arrayItem in parsedArray)
                                    {
                                        var row = new Dictionary<string, object>();
                                        PopulateRow(arrayItem, row);
                                        data.Add(row);
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error parsing property '{property.Key}': {ex.Message}");
                        }
                    }

                    if (!listFound)
                    {
                        var row = new Dictionary<string, object>();
                        PopulateRow(objNode, row);
                        data.Add(row);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error extracting sheet data: {ex.Message}");
            }

            return data;
        }

        static void PopulateRow(JsonNode? node, Dictionary<string, object> row, string prefix = "")
        {
            if (node == null)
                return;

            if (node is JsonValue valueNode)
            {
                var value = valueNode.GetValue<object>();

                // Only attempt JSON parsing if the value is a string AND looks like JSON
                if (value is string stringValue && (stringValue.StartsWith("{") || stringValue.StartsWith("[")))
                {
                    JsonNode? parsedNode = TryParseJson(stringValue);
                    if (parsedNode != null)
                    {
                        PopulateRow(parsedNode, row, prefix);
                        return;
                    }
                }

                // Otherwise, just add the value as-is
                row[prefix.Trim('.')] = value;
            }
            else if (node is JsonObject objNode)
            {
                foreach (var property in objNode)
                {
                    string key = string.IsNullOrEmpty(prefix) ? property.Key : $"{prefix}.{property.Key}";
                    PopulateRow(property.Value, row, key);
                }
            }
            else if (node is JsonArray arrayNode)
            {
                var arrayValues = arrayNode.Select(item => item?.ToJsonString() ?? "null").ToList();
                row[prefix.Trim('.')] = string.Join(", ", arrayValues);
            }
        }

        /// <summary>
        /// Tries to parse a string as JSON. Returns null if parsing fails.
        /// </summary>
        static JsonNode? TryParseJson(string jsonString)
        {
            try
            {
                return JsonNode.Parse(jsonString);
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"Invalid JSON string detected: {ex.Message}: {jsonString}");
                return null;
            }
        }
    }
}