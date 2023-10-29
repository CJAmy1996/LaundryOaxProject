using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace LaundryOaxWebAPI.Models
{
    
    public class Purchases
    {
        [Key]
        public Guid PurchaseId { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public string Service { get; set; } = string.Empty;

        public DateTime date { get; set; } = DateTime.MinValue;

        public long total { get; set; } = long.MinValue;
    }
}
