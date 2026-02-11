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
        //public async Task<MBSResponse> GetStateAndLgaByCityAsync(string City)
        //{
        //    string search = City.Trim();

        //    try
        //    {
        //        var businesslocation = await _context.BusinessLocations
        //        .Where(c =>
        //           EF.Functions.Like(c.City!, $"%{search}%") ||
        //           EF.Functions.Like(c.LgaCode!, $"%{search}%") ||
        //           EF.Functions.Like(c.StateCode!, $"%{search}%"))
        //        .ToListAsync();
        //        if (businesslocation != null)
        //        {
        //            //if (businesslocation.Count > 0)
        //            {
        //                List<BusinessLocationsDto> blocation = new List<BusinessLocationsDto>();

        //                foreach (var item in businesslocation)
        //                {
        //                    blocation.Add(new BusinessLocationsDto
        //                    {
        //                        City = item.City,
        //                        LgaCode = item.LgaCode,
        //                        StateCode = item.StateCode
        //                    });
        //                }

        //                return Success("Customers retrieved successfully", blocation);

        //            }
        //            //return Fail("No matching records found.");
        //        }
        //        return Fail("No matching records found.");

        //    }
        //    catch (Exception ex)
        //    {
        //        return Fail($"Search failed: {ex.Message}");
        //    }
        //}


        public async Task<MBSResponse> GetStateAndLgaByCityAsync(string? city = "")
        {
            try
            {
                IQueryable<BusinessLocations> query = _context.BusinessLocations
                    .AsNoTracking();

                // Apply search ONLY if city is provided
                if (!string.IsNullOrWhiteSpace(city))
                {
                    string search = city.Trim();

                    query = query.Where(c =>
                        EF.Functions.Like(c.City ?? "", $"%{search}%") ||
                        EF.Functions.Like(c.LgaCode ?? "", $"%{search}%") ||
                        EF.Functions.Like(c.StateCode ?? "", $"%{search}%"));
                }

                var results = await query
                    .OrderBy(c => c.City)
                    .Select(c => new BusinessLocationsDto
                    {
                        City = c.City,
                        LgaCode = c.LgaCode,
                        StateCode = c.StateCode
                    })
                    .ToListAsync();

                if (!results.Any())
                    return Fail("No matching locations found");

                return Success("Locations retrieved successfully", new
                {
                    Count = results.Count,
                    Data = results
                });
            }
            catch (Exception ex)
            {
                return Fail($"Search failed: {ex.Message}");
            }
        }

        public async Task<MBSResponse> GetStateAndLgaByCityAsync( string? city,   string? search,  int pageNumber = 1, int pageSize = 20)
        {
            try
            {
                // Validate pagination
                if (pageNumber <= 0) pageNumber = 1;
                if (pageSize <= 0) pageSize = 20;

                IQueryable<BusinessLocations> query =
                    _context.BusinessLocations
                            .AsNoTracking();

                // 🔹 Filter by specific city (if provided)
                if (!string.IsNullOrWhiteSpace(city))
                {
                    string cityFilter = city.Trim();

                    query = query.Where(c =>
                        EF.Functions.Like(c.City ?? "", $"%{cityFilter}%"));
                }

                // 🔹 Global search (City, LGA, State)
                if (!string.IsNullOrWhiteSpace(search))
                {
                    string searchFilter = search.Trim();

                    query = query.Where(c =>
                        EF.Functions.Like(c.City ?? "", $"%{searchFilter}%") ||
                        EF.Functions.Like(c.LgaCode ?? "", $"%{searchFilter}%") ||
                        EF.Functions.Like(c.StateCode ?? "", $"%{searchFilter}%"));
                }

                // Total records BEFORE pagination
                int totalRecords = await query.CountAsync();

                if (totalRecords == 0)
                    return Fail("No matching locations found");

                // Apply ordering + pagination
                var results = await query
                    .OrderBy(c => c.City)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Select(c => new BusinessLocationsDto
                    {
                        City = c.City,
                        LgaCode = c.LgaCode,
                        StateCode = c.StateCode
                    })
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
