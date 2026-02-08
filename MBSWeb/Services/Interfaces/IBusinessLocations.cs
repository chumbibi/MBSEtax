
using MBSWeb.Models.Dto;

namespace MBSWeb.Services.Interfaces
{
    public interface IBusinessLocations
    {
        Task<MBSResponse> GetStateAndLgaByCityAsync(string City);
    }
}
