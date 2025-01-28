using ClassLibrary.Classes.GQLObjects;

namespace ClassLibrary.Services.Interfaces
{
    public interface IFinancialReportService
    {
        public Task<List<OrderTransaction>> RunFinancialConsolidationReport();
    }
}
