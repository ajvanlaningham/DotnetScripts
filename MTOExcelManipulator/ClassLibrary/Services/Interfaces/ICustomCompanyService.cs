using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary.Services.Interfaces
{
    public interface ICustomCompanyService
    {
        Task<string> GetSiteID(long companyID, long locationID);
    }
}
