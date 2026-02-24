using MBSWeb.Models.Dto;

namespace MBSWeb.Services.Interfaces
{
    public interface ICustomers
    {
 
        Task<MBSResponse> UpdateCustomerAsync(string customerCode, CustomerDto model);
        Task<MBSResponse> DeleteCustomerAsync(string customerCode);
        Task<MBSResponse> GetCustomerByIdAsync(string customerCode);
        Task<MBSResponse> SearchForCustomerByNameAsync(string customerName);
        Task<MBSResponse> SearchForCustomerByPhoneAsync(string phone);
        Task<MBSResponse> SearchForCustomerByEmailAsync(string email);
        Task<MBSResponse> SearchForCustomerByIRNAsync(string irn);
        Task<MBSResponse> SearchForCustomerByTINAsync(string tin);
        Task<MBSResponse> SearchCustomersAsync(string? searchTerm, int pageNumber = 1, int pageSize = 20);
        //Task<MBSResponse> SearchCustomersAsync(string searchTerm);
        Task<MBSResponse> GetAllCustomersAsync();
    }
}
