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


        public async Task<MBSResponse> GetStateAndLgaByCityAsync(string? City)
        {
            try
            {
                // Trim and check if search term is provided
                string? search = City?.Trim();

                var query = _context.BusinessLocations.AsQueryable();

                // Only apply filter if search is not null/empty
                if (!string.IsNullOrWhiteSpace(search))
                {
                    query = query.Where(c =>
                        EF.Functions.Like(c.City!, $"%{search}%") ||
                        EF.Functions.Like(c.LgaCode!, $"%{search}%") ||
                        EF.Functions.Like(c.StateCode!, $"%{search}%")
                    );
                }

                var businessLocations = await query.ToListAsync();

                if (businessLocations.Count == 0)
                {
                    return Fail("No matching records found.");
                }

                // Map to DTO
                var blocation = businessLocations.Select(item => new BusinessLocationsDto
                {
                    City = item.City,
                    LgaCode = item.LgaCode,
                    StateCode = item.StateCode
                }).ToList();

                return Success("Customers retrieved successfully", blocation);
            }
            catch (Exception ex)
            {
                return Fail($"Search failed: {ex.Message}");
            }
        }

        //public async Task<MBSResponse> GetStateAndLgaByCityAsync( string? city,  int pageNumber = 1, int pageSize = 20)
        //{
        //    if (string.IsNullOrWhiteSpace(city))
        //        return Success("No search term provided", new List<BusinessLocationsDto>());

        //    string search = city.Trim();

        //    try
        //    {
        //        var query = _context.BusinessLocations
        //            .Where(c =>
        //                EF.Functions.Like(c.City!, $"%{search}%") ||
        //                EF.Functions.Like(c.LgaCode!, $"%{search}%") ||
        //                EF.Functions.Like(c.StateCode!, $"%{search}%"))
        //            .AsNoTracking();

        //        int totalRecords = await query.CountAsync();

        //        var results = await query
        //            .OrderBy(c => c.City)   // Always order before pagination
        //            .Skip((pageNumber - 1) * pageSize)
        //            .Take(pageSize)
        //            .ToListAsync();

        //        return Success("Locations retrieved successfully", new
        //        {
        //            PageNumber = pageNumber,
        //            PageSize = pageSize,
        //            TotalRecords = totalRecords,
        //            TotalPages = (int)Math.Ceiling(totalRecords / (double)pageSize),
        //            Data = results
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        return Fail($"Search failed: {ex.Message}");
        //    }
        //}

        public async Task<MBSResponse> GetStateAndLgaByCityAsync( string? city, int pageNumber = 1,int pageSize = 20)
        {
            try
            {
                if (pageNumber <= 0) pageNumber = 1;
                if (pageSize <= 0) pageSize = 20;

                // Base query
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

                // Total records before pagination
                int totalRecords = await query.CountAsync();

                // Paginated result
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
