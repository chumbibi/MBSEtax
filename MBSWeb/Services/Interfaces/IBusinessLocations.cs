
using MBSWeb.Models.Dto;
using Microsoft.AspNetCore.Mvc;

namespace MBSWeb.Services.Interfaces
{
    public interface IBusinessLocations
    {
        Task<MBSResponse> GetStateAndLgaByCityAsync(string city = "");
        Task<MBSResponse> GetStateAndLgaByCityAsync(string? city, string? search, int pageNumber = 1, int pageSize = 10);
    }
}
