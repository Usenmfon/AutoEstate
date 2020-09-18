using Microsoft.AspNetCore.Identity;

namespace AutoEstate.Data.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName {get; set; }
    }
}