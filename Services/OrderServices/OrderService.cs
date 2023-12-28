using LaundryOaxWebAPI.Models;
using LaundryOaxWebAPI.Services.DiscountServices;
using LaundryOaxWebAPI.Services.PaymentServices;
using LaundryOaxWebAPI.Services.UserServices;

namespace LaundryOaxWebAPI.Services.OrderServices

{
    public class OrderService: IOrderService
    {
        private static readonly Dictionary<Guid, Orders> _orderService = new();
        private readonly IValidateUser validateUser1;
        private readonly IPaymentService paymentService1;
        private readonly IDiscountService discountService1;

        public OrderService(IValidateUser validateUser, IPaymentService paymentService, IDiscountService discountService) {
            validateUser1= validateUser;
            paymentService1 = paymentService;
            discountService1 = discountService;
        
        }
        public void PlaceOrder(Orders request)
        {
            var userValidationResult = validateUser1.ValidateUser(request.UserId);

            if (!userValidationResult.IsValid)
            {
                // Handle invalid user, perhaps throw an exception or log the error
                throw new InvalidOperationException("Invalid user. Unable to place the order.");
            }

            var paymentResult = paymentService1.ProcessPayment(request.UserId, orderTotal);

            if (!paymentResult.Success)
            {
                // Handle payment failure, perhaps throw an exception or log the error
                throw new InvalidOperationException($"Payment failed. Reason: {paymentResult.ErrorMessage}");
            }

            _orderService.Add(request.OrderId, request);
            

        }
    }
}
