using MBSWeb.Models.Dto;

namespace MBSWeb.Services.Interfaces
{
    public interface IInvoiceTransactions
    {

        Task<MBSResponse> GetInvoiceByInvoiceNumber(int companyId, string invoiceNumber, int pageNumber = 1, int pageSize = 10);
        Task<MBSResponse> GetAllInvoicesByCompany(int companyId, int pageNumber = 1, int pageSize = 10);
        Task<MBSResponse> GetInvoiceItemsByInvoiceNumber(int companyId, string invoiceNumber, int pageNumber = 1, int pageSize = 10);
        Task<MBSResponse> GetInvoicesByCustomerCode(int companyId, string customerCode, int pageNumber = 1, int pageSize = 10);
        Task<MBSResponse> GetInvoicesByDateRange(DateRangeDto model, int pageNumber = 1, int pageSize = 10);
        Task<MBSResponse> UpdateInvoiceByIRNAsync(string irn, PaymentStatusDto model);

        Task<MBSResponse> DownloadInvoiceByNumber(int companyid, string invoiceNumber);
        

    }
}
