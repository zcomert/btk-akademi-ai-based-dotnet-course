using Microsoft.AspNetCore.Identity;
using StoreWeb.Models.Identity;
using StoreWeb.ViewModels.Auth;

namespace StoreWeb.Services;

public interface IAuthService
{
    Task<IdentityResult> RegisterAsync(RegisterViewModel model);
    Task<SignInResult> LoginAsync(LoginViewModel model);
    Task LogoutAsync();
    Task SeedDefaultsAsync();
}

public class AuthService : IAuthService
{
    private const string DefaultAdminFullName = "Zafer C\u00D6MERT";
    private const string DefaultAdminEmail = "comertzafer@gmail.com";
    private const string DefaultAdminPassword = "Zafer123456";

    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _roleManager = roleManager;
    }

    public async Task<IdentityResult> RegisterAsync(RegisterViewModel model)
    {
        var user = new ApplicationUser
        {
            FullName = model.FullName.Trim(),
            UserName = model.Email.Trim(),
            Email = model.Email.Trim()
        };

        var createResult = await _userManager.CreateAsync(user, model.Password);
        if (!createResult.Succeeded)
        {
            return createResult;
        }

        var roleAssignResult = await EnsureUserInRoleAsync(user, AppRoles.Normal);
        if (roleAssignResult.Succeeded)
        {
            return roleAssignResult;
        }

        await _userManager.DeleteAsync(user);
        return roleAssignResult;
    }

    public async Task<SignInResult> LoginAsync(LoginViewModel model)
    {
        var user = await _userManager.FindByEmailAsync(model.Email.Trim());
        if (user is null)
        {
            return SignInResult.Failed;
        }

        return await _signInManager.PasswordSignInAsync(
            user,
            model.Password,
            model.RememberMe,
            lockoutOnFailure: false);
    }

    public Task LogoutAsync()
    {
        return _signInManager.SignOutAsync();
    }

    public async Task SeedDefaultsAsync()
    {
        await SeedRolesAsync();
        await SeedAdminUserAsync();
    }

    private async Task SeedRolesAsync()
    {
        foreach (var roleName in AppRoles.All)
        {
            if (await _roleManager.RoleExistsAsync(roleName))
            {
                continue;
            }

            var result = await _roleManager.CreateAsync(new IdentityRole(roleName));
            if (!result.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Role seed failed ({roleName}): {FormatErrors(result.Errors)}");
            }
        }
    }

    private async Task SeedAdminUserAsync()
    {
        var user = await _userManager.FindByEmailAsync(DefaultAdminEmail);
        if (user is null)
        {
            user = new ApplicationUser
            {
                FullName = DefaultAdminFullName,
                UserName = DefaultAdminEmail,
                Email = DefaultAdminEmail,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, DefaultAdminPassword);
            if (!result.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Default admin user seed failed: {FormatErrors(result.Errors)}");
            }
        }

        if (!string.Equals(user.FullName, DefaultAdminFullName, StringComparison.Ordinal))
        {
            user.FullName = DefaultAdminFullName;
            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Default admin user update failed: {FormatErrors(updateResult.Errors)}");
            }
        }

        var roleResult = await EnsureUserInRoleAsync(user, AppRoles.Admin);
        if (!roleResult.Succeeded)
        {
            throw new InvalidOperationException(
                $"Assigning admin role to default user failed: {FormatErrors(roleResult.Errors)}");
        }
    }

    private async Task<IdentityResult> EnsureUserInRoleAsync(ApplicationUser user, string roleName)
    {
        if (!await _roleManager.RoleExistsAsync(roleName))
        {
            var roleCreateResult = await _roleManager.CreateAsync(new IdentityRole(roleName));
            if (!roleCreateResult.Succeeded)
            {
                return roleCreateResult;
            }
        }

        if (await _userManager.IsInRoleAsync(user, roleName))
        {
            return IdentityResult.Success;
        }

        return await _userManager.AddToRoleAsync(user, roleName);
    }

    private static string FormatErrors(IEnumerable<IdentityError> errors)
    {
        return string.Join("; ", errors.Select(e => $"{e.Code}: {e.Description}"));
    }
}
