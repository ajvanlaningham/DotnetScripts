using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary.Services.Interfaces
{
    public interface IExcelReaderService
    {
        List<T> ReadExcelFile<T>(string filePath, string sheetName = null) where T: new();
        List<T> MapDataToObjects<T>(IEnumerable<IDictionary<string, object>> data ) where T: new();
    }
}
