using MBSWeb.Models.Entities;

namespace MBSWeb.Models.Dto
{
    public class RegisterUserDto
    {
        public string? Role { get; set; }
        public string? FirstName { get; set; } // UserName
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Password { get; set; } // ConfirmPassword at the GUI level
        public string? CompanyId { get; set; } // Which company the user belongs to
 
    }
}
