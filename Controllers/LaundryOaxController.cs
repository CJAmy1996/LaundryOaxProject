using LaundryOaxWebAPI.Data;
using LaundryOaxWebAPI.Models;
using LaundryOaxWebAPI.Services.OrderServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Metadata.Ecma335;

namespace LaundryOaxWebAPI.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    public class LaundryOaxController : Controller
    {

        private readonly LaundryOaxDBContext laundryOaxDBContext1;
        private readonly OrderService orderService1;
        public LaundryOaxController(LaundryOaxDBContext laundryOaxDBContext, OrderService orderService)
        {
            orderService1 = orderService;
            laundryOaxDBContext1 = laundryOaxDBContext;
        }
       

        
        [HttpGet]
        [Route("orders")]
        public async Task<IActionResult> GetAllOrders()
        {
            var orders = await laundryOaxDBContext1.Customers.ToListAsync();
           
            return Ok(orders);
        }

        [HttpGet]
        [Route("{id:guid}")]

        public async Task<IActionResult> GetOrder([FromRoute] Guid id) { 
        var getOrder = await laundryOaxDBContext1.Customers.ToListAsync();
        return Ok(getOrder);
            }

        
        [HttpPut]
        [Route("{id:Guid}")]

        public async Task<IActionResult> UpdateCustomer([FromRoute] Guid id, Customers updateCustomers)
        {
            var customer = await laundryOaxDBContext1.Customers.FindAsync(id);

            if(customer == null)
            {
                return NotFound();

            }
        customer.Id =  updateCustomers.Id;
        customer.Name= updateCustomers.Name;
        customer.Phone=  updateCustomers.Phone;
        customer.Service = updateCustomers.Service;
        customer.date = updateCustomers.date;
        customer.total = updateCustomers.total;

            await laundryOaxDBContext1.SaveChangesAsync();
            return Ok(customer);
        }

        [HttpPost]
        [Route("add-orders")]
        public async Task<IActionResult> AddOrders([FromBody] Orders orderRequest)
        {
            if (orderRequest != null)
            {
                orderRequest.OrderId= Guid.NewGuid();
                orderRequest.date = DateTime.UtcNow;


                await laundryOaxDBContext1.Orders.AddAsync(orderRequest);
                await laundryOaxDBContext1.SaveChangesAsync();

                return Ok(orderRequest);
            }
            else
            {
                return BadRequest("Invalid customer data.");
            }
        }
       
        
        [Authorize]
        [HttpDelete]
        [Route("{id:guid}")]

        public async Task<IActionResult> DeleteOrder([FromRoute] Guid id)
        {
            var deleteOrder = await laundryOaxDBContext1.Customers.ToListAsync();
            return Ok(deleteOrder);
        }
    }
}
    
