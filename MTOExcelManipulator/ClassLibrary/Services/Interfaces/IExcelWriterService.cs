using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary.Services.Interfaces
{
    public  interface IExcelWriterService
    {
        void WriteExcelFile<T>(string filePath, IEnumerable<T> data, string sheetName = "Sheet1");
    }
}
