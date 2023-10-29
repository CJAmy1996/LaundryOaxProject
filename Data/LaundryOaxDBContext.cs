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
        public DbSet<Orders> Orders { get; set;}

        public DbSet<UserLogin> UserLogin { get; set;}

        public DbSet<Purchases> Purchases { get; set;}

        public DbSet<Supplies> Supplies { get; set;}



    }
}
