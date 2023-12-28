namespace LaundryOaxWebAPI.Models
{
    public class OrderResult
    {
        public bool Success { get; set; }
        public int? OrderId { get; set; }
        public string ErrorMessage { get; set; }
    }
}
