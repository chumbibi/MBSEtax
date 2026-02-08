using MBSWeb.Models.Dto;
using MBSWeb.Services.Interfaces;

namespace MBSWeb.Managers
{
    public class BusinessLocationsManager
    {
        private readonly IBusinessLocations _manager;
        public BusinessLocationsManager(IBusinessLocations manager)
        {
            _manager = manager;
        }

        public async Task<MBSResponse> GetStateAndLgaByCityAsync(string City)
        {
            var response = await _manager.GetStateAndLgaByCityAsync(City);
            return response;

        }
    }
}
