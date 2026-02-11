using MBSWeb.Data;
using MBSWeb.Models.Dto;
using MBSWeb.Models.Entities;
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
        public async Task<MBSResponse> GetStateAndLgaByCityAsync( string? city, int pageNumber = 1, int pageSize = 20)
        {
            try
            {
                IQueryable<BusinessLocations> query =
                    _context.BusinessLocations.OrderBy(c=>c.City).AsNoTracking();

                // Apply filter ONLY if search term exists
                if (!string.IsNullOrWhiteSpace(city))
                {
                    string search = city.Trim();

                    query = query.Where(c =>
                        EF.Functions.Like(c.City!, $"%{search}%") ||
                        EF.Functions.Like(c.LgaCode!, $"%{search}%") ||
                        EF.Functions.Like(c.StateCode!, $"%{search}%"));
                }

                int totalRecords = await query.CountAsync();

                var results = await query
                    .OrderBy(c => c.City)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return Success("Locations retrieved successfully", new
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalRecords = totalRecords,
                    TotalPages = (int)Math.Ceiling(totalRecords / (double)pageSize),
                    Data = results
                });
            }
            catch (Exception ex)
            {
                return Fail($"Search failed: {ex.Message}");
            }


            //if (string.IsNullOrWhiteSpace(city))
            //    return Success("No search term provided", new List<BusinessLocationsDto>());



            //string search = city.Trim();

            //try
            //{
            //    var query = _context.BusinessLocations
            //        .Where(c =>
            //            EF.Functions.Like(c.City!, $"%{search}%") ||
            //            EF.Functions.Like(c.LgaCode!, $"%{search}%") ||
            //            EF.Functions.Like(c.StateCode!, $"%{search}%"))
            //        .AsNoTracking();

            //    int totalRecords = await query.CountAsync();

            //    var results = await query
            //        .OrderBy(c => c.City)   // Always order before pagination
            //        .Skip((pageNumber - 1) * pageSize)
            //        .Take(pageSize)
            //        .ToListAsync();

            //    return Success("Locations retrieved successfully", new
            //    {
            //        PageNumber = pageNumber,
            //        PageSize = pageSize,
            //        TotalRecords = totalRecords,
            //        TotalPages = (int)Math.Ceiling(totalRecords / (double)pageSize),
            //        Data = results
            //    });
            //}
            //catch (Exception ex)
            //{
            //    return Fail($"Search failed: {ex.Message}");
            //}
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
