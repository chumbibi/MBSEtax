using System.ComponentModel.DataAnnotations;

namespace MBSWeb.Models.Dto
{
     
    public class BusinessLocationsDto
    {
      
        public string? City { get; set; }
        public string? LgaCode { get; set; }
        public string? StateCode { get; set; }
    }
}
