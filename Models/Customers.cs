namespace LaundryOaxWebAPI.Models
{
    public class Customers
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Service { get; set; } = string.Empty;

        public DateTime date { get; set; } = DateTime.MinValue;

        public long Phone { get; set; } = long.MinValue;

        public long total { get; set; } = long.MinValue;    

    }
}
