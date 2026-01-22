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

        public async Task<MBSResponse> GetAllInvoicesByCompany(int companyid)
        {
            var result = await _invoice.GetAllInvoicesByCompany(companyid);
            return result;
        }

        public async Task<MBSResponse> GetInvoiceByInvoiceNumber(int companyid, string InvoiceNumber)
        {
           var result = await _invoice.GetInvoiceByInvoiceNumber(companyid, InvoiceNumber);
            return result;  
        }

        public async Task<MBSResponse> GetInvoiceItemsByInvoiceNumber(int companyid, string InvoiceNumber)
        {
          var result = await _invoice.GetInvoiceItemsByInvoiceNumber(companyid, InvoiceNumber);
            return result;  
        }

        public async Task<MBSResponse> GetInvoicesByCustomerCode(int companyid, string customerCode)
        {
           var result = await _invoice.GetInvoicesByCustomerCode(companyid, customerCode);
            return result;  
        }

        public async Task<MBSResponse> GetInvoicesByDateRange(DateRangeDto model)
        {
            var result = await _invoice.GetInvoicesByDateRange(model);
            return result;  
        }

        public async Task<MBSResponse> UpdateInvoiceByIRNAsync(string irn, PaymentStatusDto model)
        {
            var result = await _invoice.UpdateInvoiceByIRNAsync(irn, model);
            return result;
        }
    }
}
