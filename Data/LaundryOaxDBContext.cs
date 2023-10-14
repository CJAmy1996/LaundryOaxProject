using LaundryOaxWebAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace LaundryOaxWebAPI.Data
{
    public class LaundryOaxDBContext : DbContext
    {
        public LaundryOaxDBContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<Customers> Customers {get; set;}
        



    }
}
