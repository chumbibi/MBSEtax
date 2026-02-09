 
using MBSWeb.Managers;
using MBSWeb.Models.Dto;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PdfSharpCore;

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

        [HttpGet("getcodewithpagination")]
        public async Task<IActionResult> GetStateAndLgaByCityWithPaginationAsync( [FromQuery] string? city, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _manager.GetStateAndLgaByCityAsync(city, pageNumber, pageSize);
            return StatusCode(result.StatusCode, result);
        }

    }
}
