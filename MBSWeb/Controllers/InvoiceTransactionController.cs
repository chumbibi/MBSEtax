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
        public async Task<IActionResult> GetAllInvoicesByCompany(int companyId, int pageNumber = 1, int pageSize = 10)
        {
            var result = await _manager.GetAllInvoicesByCompany(companyId,pageNumber,pageSize);
            return StatusCode(result.StatusCode, result);   
        }

        [HttpGet("getinvoicebyinvoicenumber")]
        public async Task<IActionResult> GetInvoiceByInvoiceNumber(int companyId, string invoiceNumber, int pageNumber = 1, int pageSize = 10)
        {
            var result = await _manager.GetInvoiceByInvoiceNumber(companyId, invoiceNumber,pageNumber,pageSize);
            return StatusCode(result.StatusCode, result);   
        }

        [HttpGet("getinvoiceitemsbyinvoicenumber")]
        public async Task<IActionResult> GetInvoiceItemsByInvoiceNumber(int companyId, string invoiceNumber, int pageNumber = 1,int pageSize = 10)
        {
            var result = await _manager.GetInvoiceByInvoiceNumber(companyId, invoiceNumber, pageNumber, pageSize);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("getinvoicesbycustomercode")]
        public async Task<MBSResponse> GetInvoicesByCustomerCode(int companyId, string customerCode, int pageNumber = 1, int pageSize = 10)
        {
            var result = await _manager.GetInvoicesByCustomerCode(companyId, customerCode,pageNumber,pageSize);
            return result;  
        }

        [HttpPost("getinvoicesbydaterange")]
        public async Task<MBSResponse> GetInvoicesByDateRange(DateRangeDto model, int pageNumber = 1, int pageSize = 10)
        {
            var result = await _manager.GetInvoicesByDateRange(model);
            return result;  
        }
        [HttpPut("updateinvoicebyirn")]
        public async Task<MBSResponse> UpdateInvoiceByIRNAsync(string irn, PaymentStatusDto model)
        {
            var result = await _manager.UpdateInvoiceByIRNAsync(irn, model);
            return result;  
        }
    }
}
