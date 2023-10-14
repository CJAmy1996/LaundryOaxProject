namespace LaundryOaxWebAPI.Models
{
    public class UserLogin
    {
        public Guid UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Password { get; set; }   
    }
}
