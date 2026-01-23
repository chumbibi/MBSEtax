using MBSWeb.Managers;
using MBSWeb.Models.Dto;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MBSWeb.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomersController : ControllerBase
    {
        private readonly CustomerManager _manager; 
        public CustomersController(CustomerManager manager)
        {
            _manager = manager; 
        }
        [HttpGet("deletecustomers")]
        public async Task<IActionResult> DeleteCustomerAsync([FromQuery] string customerCode)
        {
            var result = await _manager.DeleteCustomerAsync(customerCode);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("getallcustomers")]
        public async Task<IActionResult> GetAllCustomersAsync()
        {
            var result = await _manager.GetAllCustomersAsync();
            return StatusCode(result.StatusCode, result);
        }
        [HttpGet("getcustomerbyid")]
        public async Task<IActionResult> GetCustomerByIdAsync([FromQuery] string customerCode)
        {
            var result = await _manager.GetCustomerByIdAsync(customerCode);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("searchforcustomerbyemail")]
        public async Task<IActionResult> SearchForCustomerByEmailAsync([FromQuery] string email)
        {
            var result = await _manager.SearchForCustomerByEmailAsync(email);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("searchforcustomerbyirn")]
        public async Task<IActionResult> SearchForCustomerByIRNAsync(string irn)
        {
            var result = await _manager.SearchForCustomerByIRNAsync(irn);
            return StatusCode(result.StatusCode, result);   
        }

        [HttpGet("searchforcustomerbyname")]
        public async Task<IActionResult> SearchForCustomerByNameAsync([FromQuery] string customerName)
        {
            var result = await _manager.SearchForCustomerByNameAsync(customerName);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("searchforcustomerbyphone")]
        public async Task<IActionResult> SearchForCustomerByPhoneAsync([FromQuery] string phone)
        {
            var result = await _manager.SearchForCustomerByPhoneAsync(phone);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("searchforcustomerbytin")]
        public async Task<IActionResult> SearchForCustomerByTINAsync([FromQuery] string tin)
        {
            var result = await _manager.SearchForCustomerByTINAsync(tin);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPut("updatecustomer")]
        public async Task<IActionResult> UpdateCustomerAsync(string customerCode, CustomerDto model)
        {
            var result = await _manager.UpdateCustomerAsync(customerCode, model);
            return StatusCode(result.StatusCode, result);   
        }
        [HttpGet("searchcustomer")]
        public async Task<IActionResult> SearchCustomersAsync(string? searchTerm)
        {
            var result = await _manager.SearchCustomersAsync(searchTerm);
            return StatusCode(result.StatusCode, result);

        }
    }
}
