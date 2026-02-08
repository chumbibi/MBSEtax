using MBSWeb.Data;
using MBSWeb.Models.Dto;
using MBSWeb.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MBSWeb.Services.Repositories
{
    public class BusinessLocationsRepository : IBusinessLocations
    {
        private readonly ApplicationDbContext _context;

        public BusinessLocationsRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<MBSResponse> GetStateAndLgaByCityAsync(string City)
        {
            string search = City.Trim();

            try
            {
                var businesslocation = await _context.BusinessLocations
                .Where(c =>
                   EF.Functions.Like(c.City!, $"%{search}%") ||
                   EF.Functions.Like(c.LgaCode!, $"%{search}%") ||
                   EF.Functions.Like(c.StateCode!, $"%{search}%"))
                .ToListAsync();
                if (businesslocation != null)
                {
                    if (businesslocation.Count > 0)
                    {
                        List<BusinessLocationsDto> blocation = new List<BusinessLocationsDto>();

                        foreach (var item in businesslocation)
                        {
                            blocation.Add(new BusinessLocationsDto
                            {
                                City = item.City,
                                LgaCode = item.LgaCode,
                                StateCode = item.StateCode
                            });
                        }

                        return Success("Customers retrieved successfully", blocation);

                    }
                    //return Fail("No matching records found.");
                }
                return Fail("No matching records found.");

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
