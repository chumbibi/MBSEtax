using MBSWeb.Data;
using MBSWeb.Models.Dto;
using MBSWeb.Models.Entities;
using MBSWeb.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MBSWeb.Services.Repositories
{
    public class InvoiceTransactionsRepository : IInvoiceTransactions
    {
        private readonly ApplicationDbContext _context;

        public InvoiceTransactionsRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<MBSResponse> GetAllInvoicesByCompany(int companyid)
        {
            try
            {
                var invoices = await _context.InvoiceTransactions
                    .Where(i => i.CompanyId == companyid)
                    .OrderByDescending(i => i.InvoiceDate)
                    .ToListAsync();

                return Success("Invoices retrieved successfully", invoices);
            }
            catch (Exception ex)
            {
                return Fail($"Failed to retrieve invoices: {ex.Message}");
            }
        }

        public async Task<MBSResponse> GetInvoiceByInvoiceNumber(int companyid, string invoiceNumber)
        {
            try
            {
                var invoice = await _context.InvoiceTransactions
                    .FirstOrDefaultAsync(i =>
                        i.CompanyId == companyid &&
                        i.InvoiceNumber == invoiceNumber);

                if (invoice == null)
                    return Fail("Invoice not found");

                return Success("Invoice retrieved successfully", invoice);
            }
            catch (Exception ex)
            {
                return Fail($"Failed to retrieve invoice: {ex.Message}");
            }
        }

        public async Task<MBSResponse> GetInvoiceItemsByInvoiceNumber(int companyid, string invoiceNumber)
        {
            try
            {
                var items = await _context.ItemLines
                    .Where(i =>
                        i.CompanyId == companyid &&
                        i.DocEntry == invoiceNumber)
                    .OrderBy(i => i.LineNum)
                    .ToListAsync();

                return Success("Invoice items retrieved successfully", items);
            }
            catch (Exception ex)
            {
                return Fail($"Failed to retrieve invoice items: {ex.Message}");
            }
        }

        public async Task<MBSResponse> GetInvoicesByCustomerCode(int companyid, string customerCode)
        {
            try
            {
                var invoices = await _context.InvoiceTransactions
                    .Where(i =>
                        i.CompanyId == companyid &&
                        i.CustomerCode == customerCode)
                    .OrderByDescending(i => i.InvoiceDate)
                    .ToListAsync();

                return Success("Invoices retrieved successfully", invoices);
            }
            catch (Exception ex)
            {
                return Fail($"Failed to retrieve invoices: {ex.Message}");
            }
        }

        public async Task<MBSResponse> GetInvoicesByDateRange(DateRangeDto model)
        {
            try
            {
                if (model.StartDate == null || model.EndDate == null)
                    return Fail("Invalid date range");

                var query = _context.InvoiceTransactions.AsQueryable();

                query = query.Where(i =>
                    i.CompanyId == model.CompanyId &&
                    i.InvoiceDate >= model.StartDate &&
                    i.InvoiceDate <= model.EndDate);

                if (!string.IsNullOrWhiteSpace(model.CustomerCode))
                {
                    query = query.Where(i => i.CustomerCode == model.CustomerCode);
                }

                var invoices = await query
                    .OrderByDescending(i => i.InvoiceDate)
                    .ToListAsync();

                return Success("Invoices retrieved successfully", invoices);
            }
            catch (Exception ex)
            {
                return Fail($"Failed to retrieve invoices: {ex.Message}");
            }
        }

        public async Task<MBSResponse> UpdateInvoiceByIRNAsync(string irn, PaymentStatusDto model)
        {
            try
            {
                var invoice = await _context.InvoiceTransactions
                    .FirstOrDefaultAsync(i => i.IRN == irn);

                if (invoice == null)
                    return Fail("Invoice not found");
               int paymentStatus =  0;
                switch (model.PaymentStatus)
                {
                    case PaymentStatus.Pending:
                        paymentStatus = 0;
                        break;
                    case PaymentStatus.Paid:
                        paymentStatus = 1;
                        break;
                    case PaymentStatus.Rejected:
                        paymentStatus = 2;
                        break;
                    default:
                        return Fail("Invalid payment status");
                }

                invoice.InvoicePaymentStatus = paymentStatus;
                invoice.PaymentStatusTransmited = 1;

                await _context.SaveChangesAsync();

                return Success("Invoice payment status updated successfully", invoice);
            }
            catch (Exception ex)
            {
                return Fail($"Failed to update invoice: {ex.Message}");
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
