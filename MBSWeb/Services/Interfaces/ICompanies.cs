using MBSWeb.Models.Dto;

namespace MBSWeb.Services.Interfaces
{
    public interface ICompanies
    {
        Task<MBSResponse> CreateCompanyAsync(CompanyDto model);
        Task<MBSResponse> UpdateCompanyAsync(string companyId, CompanyDto model);

        Task<MBSResponse> DeleteCompanyAsync(string companyId);
        Task<MBSResponse> GetCompanyByIdAsync(string companyId);
        Task<MBSResponse> GetAllCompaniesAsync();
    }
}
