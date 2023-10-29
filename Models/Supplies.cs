using Microsoft.EntityFrameworkCore;

namespace LaundryOaxWebAPI.Models
{
    [Keyless]
    public class Supplies
    {
  
        public int SupplyId { get; set; }
        public string SupplyName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public long Total { get; set; }     

   

    }
}
