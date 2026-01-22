using MBSWeb.Managers;
using MBSWeb.Models.Dto;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MBSWeb.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CompaniesController : ControllerBase
    {
        private readonly CompanyManager _manager;
        public CompaniesController(CompanyManager manager)
        {
            _manager = manager; 
        }

        [HttpPost("createcompany")]
        public async Task<IActionResult> CreateCompanyAsync([FromBody] CompanyDto model)
        {
            var result = await _manager.CreateCompanyAsync(model);
            return StatusCode(result.StatusCode, result);   
        }
        [HttpDelete("deletecompany")]
        public async Task<IActionResult> DeleteCompanyAsync([FromQuery] string companyId)
        {
            var result = await _manager.DeleteCompanyAsync(companyId);
            return StatusCode(result.StatusCode, result);
        }
        [HttpGet("getallcompanies")]
        public async Task<IActionResult> GetAllCompaniesAsync()
        {
            var result = await _manager.GetAllCompaniesAsync();
            return StatusCode(result.StatusCode, result);
        }
        [HttpGet("getcompanybyid")]
        public async Task<IActionResult> GetCompanyByIdAsync([FromQuery] string companyId)
        {
            var result = await _manager.GetCompanyByIdAsync(companyId);
            return StatusCode(result.StatusCode, result);
        }
        [HttpPut("updatecompany")]
        public async Task<IActionResult> UpdateCompanyAsync([FromQuery] string companyId, [FromBody] CompanyDto model)
        {
            var result = await _manager.UpdateCompanyAsync(companyId, model);
            return StatusCode(result.StatusCode, result);
        }
    }
}
