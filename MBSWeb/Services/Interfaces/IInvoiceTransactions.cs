using MBSWeb.Models.Dto;

namespace MBSWeb.Services.Interfaces
{
    public interface IInvoiceTransactions
    {
        Task<MBSResponse> GetInvoiceByInvoiceNumber(int companyid, string  InvoiceNumber);
         Task<MBSResponse> GetInvoiceItemsByInvoiceNumber(int companyid, string InvoiceNumber);
        Task<MBSResponse> GetAllInvoicesByCompany(int companyid);
        
        Task<MBSResponse> GetInvoicesByCustomerCode(int companyid, string customerCode);
        Task<MBSResponse> GetInvoicesByDateRange(DateRangeDto model);
        Task<MBSResponse> UpdateInvoiceByIRNAsync(string irn, PaymentStatusDto model);

        Task<MBSResponse> DownloadInvoiceByNumber(int companyid, string invoiceNumber);

    }
}
