using LaundryOaxWebAPI.Models;

namespace LaundryOaxWebAPI.Services.PaymentServices
{
    public class PaymentService : IPaymentService
    {
        private readonly Dictionary<string, decimal> userBalances = new Dictionary<string, decimal>();
        public PaymentResult CheckUserBalance(string userId, decimal amount)
        {

            if (userBalances.TryGetValue(userId, out decimal userBalance))
            {
                // Check if the user has enough balance
                if (userBalance >= amount)
                {
                    // Sufficient balance, return success
                    return new PaymentResult { IsValid = true };
                }
                else
                {
                    // Insufficient balance, return failure with an error message
                    return new PaymentResult { IsValid = false, ErrorMessage = "Insufficient balance." };
                }
            }
            else
            {
                // User not found, return failure with an error message
                return new PaymentResult { IsValid = false, ErrorMessage = "User not found." };
            }
        }
}
}
