using MBSWeb.Data;
using MBSWeb.Models.Dto;
using MBSWeb.Models.Entities;
using MBSWeb.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MBSWeb.Services.Repositories
{
    public class CustomersRepository : ICustomers
    {
        private readonly ApplicationDbContext _context;

        public CustomersRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<MBSResponse> GetAllCustomersAsync()
        {
            try
            {
                var customers = await _context.Customers
                    .OrderBy(c => c.CustomerName)
                    .ToListAsync();

                return Success("Customers retrieved successfully", customers);
            }
            catch (Exception ex)
            {
                return Fail($"Failed to retrieve customers: {ex.Message}");
            }
        }

        public async Task<MBSResponse> GetCustomerByIdAsync(string customerCode)
        {
            try
            {
                var customer = await _context.Customers
                    .FirstOrDefaultAsync(c => c.CustomerCode == customerCode);

                if (customer == null)
                    return Fail("Customer not found");

                return Success("Customer retrieved successfully", customer);
            }
            catch (Exception ex)
            {
                return Fail($"Failed to retrieve customer: {ex.Message}");
            }
        }

        public async Task<MBSResponse> UpdateCustomerAsync(string customerCode, CustomerDto model)
        {
            try
            {
                var customer = await _context.Customers
                    .FirstOrDefaultAsync(c => c.CustomerCode == customerCode);

                if (customer == null)
                    return Fail("Customer not found");

                customer.CustomerName = model.CustomerName;
                customer.BusinessDescription = model.BusinessDescription;
                customer.Email = model.Email;
                customer.CustomerAddress = model.CustomerAddress;
                customer.City = model.City;
                customer.Country = model.Country;
                customer.CountryCode = model.CountryCode;
                customer.PostalZone = model.PostalZone;
                customer.Street = model.Street;
                customer.Telephone = model.Telephone;
                customer.TIN = model.TIN;
                customer.ActiveStatus = model.ActiveStatus;

                await _context.SaveChangesAsync();

                return Success("Customer updated successfully", customer);
            }
            catch (Exception ex)
            {
                return Fail($"Failed to update customer: {ex.Message}");
            }
        }

        public async Task<MBSResponse> DeleteCustomerAsync(string customerCode)
        {
            try
            {
                var customer = await _context.Customers
                    .FirstOrDefaultAsync(c => c.CustomerCode == customerCode);

                if (customer == null)
                    return Fail("Customer not found");

                _context.Customers.Remove(customer);
                await _context.SaveChangesAsync();

                return Success("Customer deleted successfully");
            }
            catch (Exception ex)
            {
                return Fail($"Failed to delete customer: {ex.Message}");
            }
        }

        public async Task<MBSResponse> SearchForCustomerByNameAsync(string customerName)
        {
            try
            {
                var customers = await _context.Customers
                    .Where(c => c.CustomerName!.Contains(customerName))
                    .ToListAsync();

                return Success("Customers retrieved successfully", customers);
            }
            catch (Exception ex)
            {
                return Fail($"Search failed: {ex.Message}");
            }
        }

        public async Task<MBSResponse> SearchForCustomerByPhoneAsync(string phone)
        {
            try
            {
                var customers = await _context.Customers
                    .Where(c => c.Telephone == phone)
                    .ToListAsync();

                return Success("Customers retrieved successfully", customers);
            }
            catch (Exception ex)
            {
                return Fail($"Search failed: {ex.Message}");
            }
        }

        public async Task<MBSResponse> SearchForCustomerByEmailAsync(string email)
        {
            try
            {
                var customers = await _context.Customers
                    .Where(c => c.Email == email)
                    .ToListAsync();

                return Success("Customers retrieved successfully", customers);
            }
            catch (Exception ex)
            {
                return Fail($"Search failed: {ex.Message}");
            }
        }

        public async Task<MBSResponse> SearchForCustomerByTINAsync(string tin)
        {
            try
            {
                var customers = await _context.Customers
                    .Where(c => c.TIN == tin)
                    .ToListAsync();

                return Success("Customers retrieved successfully", customers);
            }
            catch (Exception ex)
            {
                return Fail($"Search failed: {ex.Message}");
            }
        }

        public async Task<MBSResponse> SearchForCustomerByIRNAsync(string irn)
        {
            try
            {
                var customers = await _context.InvoiceTransactions
                    .Where(i => i.IRN == irn)
                    .Select(i => i.CustomerCode)
                    .Distinct()
                    .Join(_context.Customers,
                          code => code,
                          customer => customer.CustomerCode,
                          (code, customer) => customer)
                    .ToListAsync();

                return Success("Customers retrieved successfully", customers);
            }
            catch (Exception ex)
            {
                return Fail($"Search failed: {ex.Message}");
            }
        }
        public async Task<MBSResponse> SearchCustomersAsync(string? searchTerm)
        {
            try
            {
                // If search term is null/empty/whitespace, return all customers
                if (string.IsNullOrWhiteSpace(searchTerm))
                {
                    var allCustomers = await _context.Customers
                        .OrderBy(c => c.CustomerCode)
                        .ToListAsync();

                    return Success("Customers retrieved successfully", allCustomers);
                }

                searchTerm = searchTerm.Trim();

                // Base customer search using CONTAINS logic
                var customerQuery = _context.Customers
                    .Where(c =>
                        (!string.IsNullOrEmpty(c.CustomerCode) && c.CustomerCode.Contains(searchTerm)) ||
                        (!string.IsNullOrEmpty(c.CustomerName) && c.CustomerName.Contains(searchTerm)) ||
                        (!string.IsNullOrEmpty(c.Email) && c.Email.Contains(searchTerm)) ||
                        (!string.IsNullOrEmpty(c.Telephone) && c.Telephone.Contains(searchTerm)) ||
                        (!string.IsNullOrEmpty(c.TIN) && c.TIN.Contains(searchTerm))
                    );

                // IRN-based customer lookup
                var irnCustomersQuery = _context.InvoiceTransactions
                    .Where(i => i.IRN.Contains(searchTerm))
                    .Select(i => i.CustomerCode)
                    .Distinct()
                    .Join(_context.Customers,
                          code => code,
                          customer => customer.CustomerCode,
                          (code, customer) => customer);

                // Merge and deduplicate results
                var customers = await customerQuery
                    .Union(irnCustomersQuery)
                    .OrderBy(c => c.CustomerCode)
                    .ToListAsync();

                return Success("Customers retrieved successfully", customers);
            }
            catch (Exception ex)
            {
                return Fail($"Search failed: {ex.Message}");
            }
        }

      

        private static MBSResponse Success(string message, object? data = null) =>
            new()
            {
                StatusCode = 200,
                Message = message,
                Data = data
            };

        private static MBSResponse Fail(string message) =>
            new()
            {
                StatusCode = 400,
                Message = message,
                Data = null
            };

       
    }
}
