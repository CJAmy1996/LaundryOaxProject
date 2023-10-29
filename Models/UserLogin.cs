using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace LaundryOaxWebAPI.Models
{
    public class UserLogin
    {
        [Key]
        public Guid UserId { get; set; } 
        public String Email { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
