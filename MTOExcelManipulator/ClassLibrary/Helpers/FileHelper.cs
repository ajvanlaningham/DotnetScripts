using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary.Helpers
{
    public static class FileHelper
    {
        public static List<string> GetAllJsonFiles(string directory)
        {
            if (Directory.Exists(directory))
            {
                return new List<string>(Directory.GetFiles(directory, "*.json", SearchOption.AllDirectories));
            }
            else
            {
                Console.WriteLine($"Directory not found: {directory}");
                return new List<string>();
            }
        }
    }
}
