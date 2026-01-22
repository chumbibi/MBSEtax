using MBSWeb.Data;
using MBSWeb.Models.Dto;
using MBSWeb.Models.Entities;
using MBSWeb.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MBSWeb.Services.Repositories
{
    public class CompaniesRepository : ICompanies
    {
        private readonly ApplicationDbContext _context;

        public CompaniesRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<MBSResponse> CreateCompanyAsync(CompanyDto model)
        {
            try
            {
                var exists = await _context.Companies
                    .AnyAsync(c => c.CompanyId == model.CompanyId);

                if (exists)
                    return Fail("Company with the same CompanyId already exists");

                var company = new Companies
                {
                    CompanyId = model.CompanyId,
                    ERPUserId = model.ERPUserId,
                    ERPPassword = model.ERPPassword,
                    CompanyFIRSReferenceNumber = model.CompanyFIRSReferenceNumber,
                    CompanyFIRSServiceNumber = model.CompanyFIRSServiceNumber,
                    CompanyFIRSBusinessId = model.CompanyFIRSBusinessId,
                    CompanyCode = model.CompanyCode,
                    CompanyName = model.CompanyName,
                    CompanyAddress = model.CompanyAddress,
                    BusinessDescription = model.BusinessDescription,
                    Email = model.Email,
                    City = model.City,
                    Country = model.Country,
                    CountryCode = model.CountryCode,
                    PostalZone = model.PostalZone,
                    Street = model.Street,
                    Telephone = model.Telephone,
                    TIN = model.TIN,
                    AuthUrl = model.AuthUrl,
                    ActiveStatus = model.ActiveStatus
                };

                await _context.Companies.AddAsync(company);
                await _context.SaveChangesAsync();

                return Success("Company created successfully", company);
            }
            catch (Exception ex)
            {
                return Fail($"Failed to create company: {ex.Message}");
            }
        }

        public async Task<MBSResponse> UpdateCompanyAsync(string companyId, CompanyDto model)
        {
            try
            {
                var company = await _context.Companies
                    .FirstOrDefaultAsync(c => c.CompanyId == companyId);

                if (company == null)
                    return Fail("Company not found");

                company.ERPUserId = model.ERPUserId;
                company.ERPPassword = model.ERPPassword;
                company.CompanyFIRSReferenceNumber = model.CompanyFIRSReferenceNumber;
                company.CompanyFIRSServiceNumber = model.CompanyFIRSServiceNumber;
                company.CompanyFIRSBusinessId = model.CompanyFIRSBusinessId;
                company.CompanyCode = model.CompanyCode;
                company.CompanyName = model.CompanyName;
                company.CompanyAddress = model.CompanyAddress;
                company.BusinessDescription = model.BusinessDescription;
                company.Email = model.Email;
                company.City = model.City;
                company.Country = model.Country;
                company.CountryCode = model.CountryCode;
                company.PostalZone = model.PostalZone;
                company.Street = model.Street;
                company.Telephone = model.Telephone;
                company.TIN = model.TIN;
                company.AuthUrl = model.AuthUrl;
                company.ActiveStatus = model.ActiveStatus;

                await _context.SaveChangesAsync();

                return Success("Company updated successfully", company);
            }
            catch (Exception ex)
            {
                return Fail($"Failed to update company: {ex.Message}");
            }
        }

        public async Task<MBSResponse> DeleteCompanyAsync(string companyId)
        {
            try
            {
                var company = await _context.Companies
                    .FirstOrDefaultAsync(c => c.CompanyId == companyId);

                if (company == null)
                    return Fail("Company not found");

                _context.Companies.Remove(company);
                await _context.SaveChangesAsync();

                return Success("Company deleted successfully");
            }
            catch (Exception ex)
            {
                return Fail($"Failed to delete company: {ex.Message}");
            }
        }

        public async Task<MBSResponse> GetCompanyByIdAsync(string companyId)
        {
            try
            {
                var company = await _context.Companies
                    .FirstOrDefaultAsync(c => c.CompanyId == companyId);

                if (company == null)
                    return Fail("Company not found");

                return Success("Company retrieved successfully", company);
            }
            catch (Exception ex)
            {
                return Fail($"Failed to retrieve company: {ex.Message}");
            }
        }

        public async Task<MBSResponse> GetAllCompaniesAsync()
        {
            try
            {
                var companies = await _context.Companies
                    .OrderBy(c => c.CompanyName)
                    .ToListAsync();

                return Success("Companies retrieved successfully", companies);
            }
            catch (Exception ex)
            {
                return Fail($"Failed to retrieve companies: {ex.Message}");
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
