using LaundryOaxWebAPI.Data;
using LaundryOaxWebAPI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LaundryOaxWebAPI.Controllers
{
    [Route("api/users")]
    [ApiController]


    public class UsersController : ControllerBase
    {
        private readonly LaundryOaxDBContext laundryOaxDBContext1;

        [HttpPost]
        [Route("add-users")]
        public async Task<IActionResult> AddUsers([FromBody] UserLogin loginRequest)
        {
            if (loginRequest != null)
            {
                loginRequest.UserId = Guid.NewGuid();
                await laundryOaxDBContext1.UserLogin.AddAsync(loginRequest);
                await laundryOaxDBContext1.SaveChangesAsync();

                return Ok(loginRequest);
            }
            else
            {
                return BadRequest("Invalid customer data.");
            }
        }
    }
}
