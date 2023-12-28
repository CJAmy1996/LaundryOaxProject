using LaundryOaxWebAPI.Models;

namespace LaundryOaxWebAPI.Services.OrderServices
{
    public interface IOrderService
    {
        void PlaceOrder(Orders request);

    }
}