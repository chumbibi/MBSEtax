using Microsoft.AspNetCore.Identity;

namespace MBSWeb.Models.Entities
{
    public class MBSUsers: IdentityUser
    {
         public string? CompanyId { get; set; } // Which company the user belongs to
        //userName = FirstName
        public string? LastName { get; set; } // Surname
    }
}
