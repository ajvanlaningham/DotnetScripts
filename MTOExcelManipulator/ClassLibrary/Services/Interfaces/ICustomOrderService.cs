using static OrdersByCustomerResponse;

namespace ClassLibrary.Services.Interfaces
{
    public interface ICustomOrderService
    {
        Task<List<OrderDetail>> GetOrdersByCustomerIdAsync(string customerGid);
    }
}
