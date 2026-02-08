using MBSWeb.Managers;
using MBSWeb.Models.Dto;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MBSWeb.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InvoiceTransactionController : ControllerBase
    {
        private readonly InvoiceTransactionManager _manager;
        public InvoiceTransactionController(InvoiceTransactionManager manager   )
        {
            _manager = manager;
        }

        [HttpGet("getallinvoicesbycompany")]
        public async Task<IActionResult> GetAllInvoicesByCompany(int companyid)
        {
            var result = await _manager.GetAllInvoicesByCompany(companyid);
            return StatusCode(result.StatusCode, result);   
        }

        [HttpGet("getinvoicebyinvoicenumber")]
        public async Task<IActionResult> GetInvoiceByInvoiceNumber(int companyid, string InvoiceNumber)
        {
            var result = await _manager.GetInvoiceByInvoiceNumber(companyid, InvoiceNumber);
            return StatusCode(result.StatusCode, result);   
        }

        [HttpGet("getinvoiceitemsbyinvoicenumber")]
        public async Task<MBSResponse> GetInvoiceItemsByInvoiceNumber(int companyid, string InvoiceNumber)
        {
            var result = await _manager.GetInvoiceItemsByInvoiceNumber(companyid, InvoiceNumber);
            return result;  
        }

        [HttpGet("getinvoicesbycustomercode")]
        public async Task<MBSResponse> GetInvoicesByCustomerCode(int companyid, string customerCode)
        {
            var result = await _manager.GetInvoicesByCustomerCode(companyid, customerCode);
            return result;  
        }

        [HttpPost("getinvoicesbydaterange")] 
        public async Task<MBSResponse> GetInvoicesByDateRange([FromBody] DateRangeDto model)
        {
            var result = await _manager.GetInvoicesByDateRange(model);
            return result;  
        }
        [HttpPut("updateinvoicebyirn")]
        public async Task<MBSResponse> UpdateInvoiceByIRNAsync(string irn, [FromBody]PaymentStatusDto model)
        {
            var result = await _manager.UpdateInvoiceByIRNAsync(irn, model);
            return result;  
        }
    }
}
