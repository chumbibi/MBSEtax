using MBSWeb.Models.Dto;
using MBSWeb.Services.Interfaces;

namespace MBSWeb.Managers
{
    public class CustomerManager
    {
        private readonly ICustomers _customer;
        public CustomerManager(ICustomers customer)
        {
            _customer = customer;
        }

        public async Task<MBSResponse> DeleteCustomerAsync(string customerCode)
        {
            var result = await _customer.DeleteCustomerAsync(customerCode);
            return result;
        }

        public async Task<MBSResponse> GetAllCustomersAsync()
        {
            var result = await _customer.GetAllCustomersAsync();
            return result;
        }

        public async Task<MBSResponse> GetCustomerByIdAsync(string customerCode)
        {
            var result = await _customer.GetCustomerByIdAsync(customerCode);
            return result;
        }

        public async Task<MBSResponse> SearchForCustomerByEmailAsync(string email)
        {
            var result = await _customer.SearchForCustomerByEmailAsync(email);
            return result;
        }

        public async Task<MBSResponse> SearchForCustomerByIRNAsync(string irn)
        {
            var result = await _customer.SearchForCustomerByIRNAsync(irn);
            return result;
        }

        public async Task<MBSResponse> SearchForCustomerByNameAsync(string customerName)
        {
            var result = await _customer.SearchForCustomerByNameAsync(customerName);
            return result;
        }

        public async Task<MBSResponse> SearchForCustomerByPhoneAsync(string phone)
        {
            var result = await _customer.SearchForCustomerByPhoneAsync(phone);
            return result;
        }

        public async Task<MBSResponse> SearchForCustomerByTINAsync(string tin)
        {
            var result = await _customer.SearchForCustomerByTINAsync(tin);
            return result;
        }

        public async Task<MBSResponse> UpdateCustomerAsync(string customerCode, CustomerDto model)
        {
            var result = await _customer.UpdateCustomerAsync(customerCode, model);
            return result;
        }

        public async Task<MBSResponse> SearchCustomersAsync(string? searchTerm)
        {
            var result = await _customer.SearchCustomersAsync(searchTerm);
            return result;
        }
    }
}