 
using MBSWeb.Managers;
using MBSWeb.Models.Dto;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MBSWeb.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BusinessLocationsController : ControllerBase
    {
        private readonly BusinessLocationsManager _manager;
        public BusinessLocationsController(BusinessLocationsManager manager)
        {
            _manager = manager;
        }

         
        [HttpGet("getstateandlgacodebyCity")]
        public async Task<IActionResult> GetStateAndLgaByCityAsync([FromQuery] string city)
        {
            var result = await _manager.GetStateAndLgaByCityAsync(city);
            return StatusCode(result.StatusCode, result);
        }
         
    }
}
