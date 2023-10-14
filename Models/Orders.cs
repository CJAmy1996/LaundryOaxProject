namespace LaundryOaxWebAPI.Models
{
    public class Orders
    {

        public Guid OrderId { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public string Service { get; set; } = string.Empty;

        public DateTime date { get; set; } = DateTime.MinValue;

        public long total { get; set; } = long.MinValue;
    }
}
