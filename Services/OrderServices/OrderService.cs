using LaundryOaxWebAPI.Data;
using LaundryOaxWebAPI.Models;
namespace LaundryOaxWebAPI.Services.OrderServices;
public sealed class OrderService(LaundryOaxDBContext db) : IOrderService
{
    public void PlaceOrder(Orders request)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (request.total < 0 || string.IsNullOrWhiteSpace(request.ServiceName))
            throw new ArgumentException("A service and nonnegative total are required.");
        if (request.OrderId == Guid.Empty) request.OrderId = Guid.NewGuid();
        request.date = DateTime.UtcNow;
        db.Orders.Add(request);
        db.SaveChanges();
    }
}
