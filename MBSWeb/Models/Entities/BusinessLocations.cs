using System.ComponentModel.DataAnnotations;

namespace MBSWeb.Models.Entities
{
    public class BusinessLocations
    {
        public int Id { get; set; }
        [Key]
        public string? City { get; set; }
        public string? LgaCode { get; set; }
        public string? StateCode { get; set; }
    }
}
