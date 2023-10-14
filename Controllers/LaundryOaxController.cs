using LaundryOaxWebAPI.Data;
using LaundryOaxWebAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LaundryOaxWebAPI.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    public class LaundryOaxController : Controller
    {

        private readonly LaundryOaxDBContext laundryOaxDBContext1;
        public LaundryOaxController(LaundryOaxDBContext laundryOaxDBContext)
        {
            laundryOaxDBContext1 = laundryOaxDBContext;
        }

        [HttpGet]
        [Route("orders")]
        public async Task<IActionResult> GetAllOrders()
        {
            var orders = await laundryOaxDBContext1.Customers.ToListAsync();
            return Ok(orders);
        }

        [HttpPost]
        [Route("add-orders")]
        public async Task<IActionResult> AddOrders([FromBody] Customers customerRequest)
        {
            if (customerRequest != null)
            {
                customerRequest.Id = Guid.NewGuid();
                await laundryOaxDBContext1.Customers.AddAsync(customerRequest);
                await laundryOaxDBContext1.SaveChangesAsync();

                return Ok(customerRequest);
            }
            else
            {
                return BadRequest("Invalid customer data.");
            }
        }
    }
}
    
