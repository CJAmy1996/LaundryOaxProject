using LaundryOaxWebAPI.Models;

namespace LaundryOaxWebAPI.Services.PaymentServices
{
    public interface IPaymentService
    {

        PaymentResult CheckUserBalance(string userId, decimal amount);
    }
}
