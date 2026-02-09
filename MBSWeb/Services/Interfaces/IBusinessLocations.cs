
using MBSWeb.Models.Dto;

namespace MBSWeb.Services.Interfaces
{
    public interface IBusinessLocations
    {
        Task<MBSResponse> GetStateAndLgaByCityAsync(string City);
        Task<MBSResponse> GetStateAndLgaByCityAsync(string? city, int pageNumber = 1, int pageSize = 10);
    }
}
