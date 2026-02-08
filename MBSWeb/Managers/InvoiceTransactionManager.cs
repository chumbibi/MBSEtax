using MBSWeb.Models.Dto;
using MBSWeb.Services.Interfaces;

namespace MBSWeb.Managers
{
    public class InvoiceTransactionManager 
    {
        private readonly IInvoiceTransactions _invoice;
        public InvoiceTransactionManager(IInvoiceTransactions invoice   )
        {
            _invoice = invoice;
        }

        public async Task<MBSResponse> GetAllInvoicesByCompany(int companyId, int pageNumber = 1, int pageSize = 10)
        {
            var result = await _invoice.GetAllInvoicesByCompany(companyId,pageNumber,pageSize);
            return result;
        }

        public async Task<MBSResponse> GetInvoiceItemsByInvoiceNumber(int companyId, string invoiceNumber, int pageNumber = 1,int pageSize = 10)
        {
           var result = await _invoice.GetInvoiceByInvoiceNumber(companyId, invoiceNumber,pageNumber,pageSize);
            return result;  
        }

        public async Task<MBSResponse> GetInvoiceByInvoiceNumber(int companyId, string invoiceNumber, int pageNumber = 1, int pageSize = 10)
        {
          var result = await _invoice.GetInvoiceByInvoiceNumber(companyId, invoiceNumber, pageNumber,pageSize);
            return result;  
        }

        public async Task<MBSResponse> GetInvoicesByCustomerCode(int companyId, string customerCode, int pageNumber = 1, int pageSize = 10)
        {
           var result = await _invoice.GetInvoicesByCustomerCode(companyId, customerCode,pageNumber,pageSize);
            return result;  
        }

        public async Task<MBSResponse> GetInvoicesByDateRange(DateRangeDto model, int pageNumber = 1, int pageSize = 10)
        {
            var result = await _invoice.GetInvoicesByDateRange(model,pageNumber,pageSize);
            return result;  
        }

        public async Task<MBSResponse> UpdateInvoiceByIRNAsync(string irn, PaymentStatusDto model)
        {
            var result = await _invoice.UpdateInvoiceByIRNAsync(irn, model);
            return result;
        }
    }
}
