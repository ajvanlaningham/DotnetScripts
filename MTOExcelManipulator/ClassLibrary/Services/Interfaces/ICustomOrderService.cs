namespace ClassLibrary.Services.Interfaces
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using static ClassLibrary.Classes.GQLObjects.OrdersByCustomerResponse;

    public interface ICustomOrderService
    {
        Task<List<OrderDetail>> GetOrdersByCustomerIdAsync(string customerGid);
    }
}
