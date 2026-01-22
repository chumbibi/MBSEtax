using MBSWeb.Models.Dto;
using MBSWeb.Services.Interfaces;

namespace MBSWeb.Managers
{
    public class CompanyManager
    {
        private readonly ICompanies _company;
        public CompanyManager(ICompanies company)
        {
            _company = company;
        }

        public async Task<MBSResponse> CreateCompanyAsync(CompanyDto model)
        {

            var result = await _company.CreateCompanyAsync(model);
            return result;
        }

        public async Task<MBSResponse> DeleteCompanyAsync(string companyId)
        {
            var result = await _company.DeleteCompanyAsync(companyId);
            return result;
        }

        public async Task<MBSResponse> GetAllCompaniesAsync()
        {
            var result = await _company.GetAllCompaniesAsync();
            return result;
        }

        public async Task<MBSResponse> GetCompanyByIdAsync(string companyId)
        {
            var result = await _company.GetCompanyByIdAsync(companyId);
            return result;
        }

        public async Task<MBSResponse> UpdateCompanyAsync(string companyId, CompanyDto model)
        {
            var result = await _company.UpdateCompanyAsync(companyId, model);
            return result;  
        }
    }
}
