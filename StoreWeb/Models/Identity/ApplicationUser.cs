using Microsoft.AspNetCore.Identity;

namespace StoreWeb.Models.Identity;

public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
}
