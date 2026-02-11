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
        public InvoiceTransactionController(InvoiceTransactionManager manager)
        {
            _manager = manager;
        }

        [HttpGet("getallinvoicesbycompany")]
        // public async Task<IActionResult> GetAllInvoicesByCompany(int companyId, string? searchTerm = null, int pageNumber = 1, int pageSize = 10)
        public async Task<IActionResult> GetAllInvoicesByCompany(int? companyId = null, string? companyCode = null, string? searchTerm = null, int pageNumber = 1, int pageSize = 10)
        {
            var result = await _manager.GetAllInvoicesByCompany(companyId, pageNumber, pageSize);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("getinvoicebyinvoicenumber")]
        public async Task<IActionResult> GetInvoiceByInvoiceNumber(int companyId, string invoiceNumber, int pageNumber = 1, int pageSize = 10)
        {
            var result = await _manager.GetInvoiceByInvoiceNumber(companyId, invoiceNumber, pageNumber, pageSize);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("getinvoiceitemsbyinvoicenumber")]
        public async Task<IActionResult> GetInvoiceItemsByInvoiceNumber(int companyId, string invoiceNumber, int pageNumber = 1, int pageSize = 10)
        {
            var result = await _manager.GetInvoiceByInvoiceNumber(companyId, invoiceNumber, pageNumber, pageSize);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("getinvoicesbycustomercode")]
        public async Task<IActionResult> GetInvoicesByCustomerCode(int companyId, string customerCode, int pageNumber = 1, int pageSize = 10)
        {
            var result = await _manager.GetInvoicesByCustomerCode(companyId, customerCode, pageNumber, pageSize);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("getinvoicesbydaterange")]
        public async Task<IActionResult> GetInvoicesByDateRange([FromBody] DateRangeDto model)
        {
            var result = await _manager.GetInvoicesByDateRange(model);
            return StatusCode(result.StatusCode, result);
        }
        [HttpPut("updateinvoicebyirn")]
        public async Task<IActionResult> UpdateInvoiceByIRNAsync(string irn, [FromBody] PaymentStatusDto model)
        {
            var result = await _manager.UpdateInvoiceByIRNAsync(irn, model);
            return StatusCode(result.StatusCode, result);
        }
        [HttpGet("downloadInvoice")]
        public async Task<IActionResult> DownloadInvoiceByNumber(int companyid, string invoiceNumber)
        {
            var result = await _manager.DownloadInvoiceByNumber(companyid, invoiceNumber);
            return StatusCode(result.StatusCode, result);
        }

      

    }
}