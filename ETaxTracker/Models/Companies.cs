using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETaxTracker.Models
{
    public class Companies
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string? CompanyId { get; set; }

        public string? ERPUserId { get; set; }

        public string? ERPPassword { get; set; }

        public string? CompanyFIRSReferenceNumber { get; set; }

        public string? CompanyFIRSServiceNumber { get; set; }

        public string? CompanyFIRSBsinessId { get; set; }

        public string? CompanyCode { get; set; }

        [Required]
        public string? CompanyName { get; set; }

        public string? CompanyAddress { get; set; }

        public string? BusinessDescription { get; set; }

        public string? Email { get; set; }

        public string? City { get; set; }

        public string? LgaCode { get; set; }

        public string? StateCode { get; set; }

        public string? Country { get; set; }

        public string? CountryCode { get; set; }

        public string? PostalZone { get; set; }

        public string? Street { get; set; }

        public string? Telephone { get; set; }

        public string? TIN { get; set; }

        public string? AuthUrl { get; set; }

        public int ActiveStatus { get; set; }
    }

}
