using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StoreWeb.Models.Identity;
using StoreWeb.Repositories;
using StoreWeb.ViewModels.Admin;

namespace StoreWeb.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = AppRoles.Admin)]
public class UserManagementController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly RepositoryContext _context;

    public UserManagementController(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        RepositoryContext context)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? searchTerm = null, string? roleFilter = null)
    {
        var users = await _userManager.Users
            .AsNoTracking()
            .OrderBy(user => user.Email)
            .ToListAsync();

        var rolesByUser = await GetRolesByUserAsync();
        var normalizedSearch = searchTerm?.Trim();
        var normalizedRole = roleFilter?.Trim();

        var filteredUsers = users
            .Where(user =>
            {
                var matchesSearch = string.IsNullOrWhiteSpace(normalizedSearch)
                    || user.Email?.Contains(normalizedSearch, StringComparison.OrdinalIgnoreCase) == true
                    || user.FullName.Contains(normalizedSearch, StringComparison.OrdinalIgnoreCase);

                var userRoles = rolesByUser.GetValueOrDefault(user.Id, []);
                var matchesRole = string.IsNullOrWhiteSpace(normalizedRole)
                    || userRoles.Contains(normalizedRole, StringComparer.OrdinalIgnoreCase);

                return matchesSearch && matchesRole;
            })
            .Select(user => new UserManagementListItemViewModel
            {
                Id = user.Id,
                FullName = string.IsNullOrWhiteSpace(user.FullName) ? "-" : user.FullName,
                Email = user.Email ?? "-",
                EmailConfirmed = user.EmailConfirmed,
                IsLockedOut = IsUserLockedOut(user),
                Roles = rolesByUser.GetValueOrDefault(user.Id, [])
            })
            .ToList();

        var model = new UserManagementIndexViewModel
        {
            SearchTerm = normalizedSearch,
            RoleFilter = normalizedRole,
            Users = filteredUsers,
            RoleOptions = await BuildRoleOptionsAsync(
                string.IsNullOrWhiteSpace(normalizedRole) ? [] : [normalizedRole])
        };

        ViewData["Title"] = "User Management";
        ViewData["Subtitle"] = "Manage identity users, roles, and account states.";
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Details(string id)
    {
        var user = await _userManager.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == id);

        if (user is null)
        {
            return NotFound();
        }

        var model = new UserManagementDetailsViewModel
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email ?? "-",
            EmailConfirmed = user.EmailConfirmed,
            IsLockedOut = IsUserLockedOut(user),
            LockoutEnd = user.LockoutEnd,
            Roles = await GetUserRolesAsync(user.Id)
        };

        ViewData["Title"] = $"User Details - {model.Email}";
        ViewData["Subtitle"] = "Inspect account details and role assignments.";
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var model = new UserManagementCreateViewModel
        {
            SelectedRoles = [AppRoles.Normal],
            RoleOptions = await BuildRoleOptionsAsync([AppRoles.Normal])
        };

        ViewData["Title"] = "Create User";
        ViewData["Subtitle"] = "Create a new account and assign initial roles.";
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(UserManagementCreateViewModel model)
    {
        model.SelectedRoles = NormalizeRoles(model.SelectedRoles);
        if (!model.SelectedRoles.Any())
        {
            model.SelectedRoles.Add(AppRoles.Normal);
        }

        if (!await ValidateRolesAsync(model.SelectedRoles))
        {
            ModelState.AddModelError(nameof(model.SelectedRoles), "One or more selected roles are invalid.");
        }

        if (!ModelState.IsValid)
        {
            model.RoleOptions = await BuildRoleOptionsAsync(model.SelectedRoles);
            ViewData["Title"] = "Create User";
            ViewData["Subtitle"] = "Create a new account and assign initial roles.";
            return View(model);
        }

        var user = new ApplicationUser
        {
            FullName = model.FullName.Trim(),
            Email = model.Email.Trim(),
            UserName = model.Email.Trim(),
            EmailConfirmed = true
        };

        var createResult = await _userManager.CreateAsync(user, model.Password);
        if (!createResult.Succeeded)
        {
            AddErrorsToModelState(createResult);
            model.RoleOptions = await BuildRoleOptionsAsync(model.SelectedRoles);
            ViewData["Title"] = "Create User";
            ViewData["Subtitle"] = "Create a new account and assign initial roles.";
            return View(model);
        }

        var roleResult = await _userManager.AddToRolesAsync(user, model.SelectedRoles);
        if (!roleResult.Succeeded)
        {
            await _userManager.DeleteAsync(user);
            AddErrorsToModelState(roleResult);
            model.RoleOptions = await BuildRoleOptionsAsync(model.SelectedRoles);
            ViewData["Title"] = "Create User";
            ViewData["Subtitle"] = "Create a new account and assign initial roles.";
            return View(model);
        }

        TempData["AdminUserMessage"] = "User created successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user is null)
        {
            return NotFound();
        }

        var selectedRoles = await _userManager.GetRolesAsync(user);
        var model = new UserManagementEditViewModel
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email ?? string.Empty,
            IsLockedOut = IsUserLockedOut(user),
            SelectedRoles = selectedRoles.ToList(),
            RoleOptions = await BuildRoleOptionsAsync(selectedRoles)
        };

        ViewData["Title"] = $"Edit User - {model.Email}";
        ViewData["Subtitle"] = "Update account profile, access roles, and lock status.";
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(UserManagementEditViewModel model)
    {
        var user = await _userManager.FindByIdAsync(model.Id);
        if (user is null)
        {
            return NotFound();
        }

        model.SelectedRoles = NormalizeRoles(model.SelectedRoles);
        if (!model.SelectedRoles.Any())
        {
            ModelState.AddModelError(nameof(model.SelectedRoles), "At least one role must be selected.");
        }

        if (!await ValidateRolesAsync(model.SelectedRoles))
        {
            ModelState.AddModelError(nameof(model.SelectedRoles), "One or more selected roles are invalid.");
        }

        var currentUserId = _userManager.GetUserId(User);
        if (string.Equals(currentUserId, user.Id, StringComparison.Ordinal)
            && !model.SelectedRoles.Contains(AppRoles.Admin, StringComparer.OrdinalIgnoreCase))
        {
            ModelState.AddModelError(nameof(model.SelectedRoles), "You cannot remove your own Admin role.");
        }

        if (!ModelState.IsValid)
        {
            model.RoleOptions = await BuildRoleOptionsAsync(model.SelectedRoles);
            ViewData["Title"] = $"Edit User - {model.Email}";
            ViewData["Subtitle"] = "Update account profile, access roles, and lock status.";
            return View(model);
        }

        user.FullName = model.FullName.Trim();
        user.Email = model.Email.Trim();
        user.UserName = model.Email.Trim();
        user.NormalizedEmail = _userManager.NormalizeEmail(user.Email);
        user.NormalizedUserName = _userManager.NormalizeName(user.UserName);
        user.LockoutEnabled = true;
        user.LockoutEnd = model.IsLockedOut
            ? DateTimeOffset.UtcNow.AddYears(50)
            : null;

        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            AddErrorsToModelState(updateResult);
            model.RoleOptions = await BuildRoleOptionsAsync(model.SelectedRoles);
            ViewData["Title"] = $"Edit User - {model.Email}";
            ViewData["Subtitle"] = "Update account profile, access roles, and lock status.";
            return View(model);
        }

        if (!string.IsNullOrWhiteSpace(model.NewPassword))
        {
            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
            var resetResult = await _userManager.ResetPasswordAsync(user, resetToken, model.NewPassword);
            if (!resetResult.Succeeded)
            {
                AddErrorsToModelState(resetResult);
                model.RoleOptions = await BuildRoleOptionsAsync(model.SelectedRoles);
                ViewData["Title"] = $"Edit User - {model.Email}";
                ViewData["Subtitle"] = "Update account profile, access roles, and lock status.";
                return View(model);
            }
        }

        var currentRoles = await _userManager.GetRolesAsync(user);
        var rolesToRemove = currentRoles.Except(model.SelectedRoles, StringComparer.OrdinalIgnoreCase).ToList();
        var rolesToAdd = model.SelectedRoles.Except(currentRoles, StringComparer.OrdinalIgnoreCase).ToList();

        if (rolesToRemove.Count > 0)
        {
            var removeResult = await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
            if (!removeResult.Succeeded)
            {
                AddErrorsToModelState(removeResult);
                model.RoleOptions = await BuildRoleOptionsAsync(model.SelectedRoles);
                ViewData["Title"] = $"Edit User - {model.Email}";
                ViewData["Subtitle"] = "Update account profile, access roles, and lock status.";
                return View(model);
            }
        }

        if (rolesToAdd.Count > 0)
        {
            var addResult = await _userManager.AddToRolesAsync(user, rolesToAdd);
            if (!addResult.Succeeded)
            {
                AddErrorsToModelState(addResult);
                model.RoleOptions = await BuildRoleOptionsAsync(model.SelectedRoles);
                ViewData["Title"] = $"Edit User - {model.Email}";
                ViewData["Subtitle"] = "Update account profile, access roles, and lock status.";
                return View(model);
            }
        }

        TempData["AdminUserMessage"] = "User updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Delete(string id)
    {
        var user = await _userManager.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == id);

        if (user is null)
        {
            return NotFound();
        }

        var model = new UserManagementDeleteViewModel
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email ?? "-",
            IsCurrentUser = string.Equals(_userManager.GetUserId(User), user.Id, StringComparison.Ordinal),
            Roles = await GetUserRolesAsync(user.Id)
        };

        ViewData["Title"] = $"Delete User - {model.Email}";
        ViewData["Subtitle"] = "Delete user account and remove role memberships.";
        return View(model);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user is null)
        {
            return NotFound();
        }

        if (string.Equals(_userManager.GetUserId(User), user.Id, StringComparison.Ordinal))
        {
            TempData["AdminUserMessage"] = "You cannot delete your own account.";
            return RedirectToAction(nameof(Index));
        }

        var result = await _userManager.DeleteAsync(user);
        if (!result.Succeeded)
        {
            AddErrorsToModelState(result);
            var model = new UserManagementDeleteViewModel
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email ?? "-",
                IsCurrentUser = false,
                Roles = await GetUserRolesAsync(user.Id)
            };

            ViewData["Title"] = $"Delete User - {model.Email}";
            ViewData["Subtitle"] = "Delete user account and remove role memberships.";
            return View("Delete", model);
        }

        TempData["AdminUserMessage"] = "User deleted successfully.";
        return RedirectToAction(nameof(Index));
    }

    private async Task<Dictionary<string, List<string>>> GetRolesByUserAsync()
    {
        var rolePairs = await (
            from userRole in _context.UserRoles
            join role in _context.Roles on userRole.RoleId equals role.Id
            select new { userRole.UserId, RoleName = role.Name })
            .ToListAsync();

        return rolePairs
            .GroupBy(item => item.UserId)
            .ToDictionary(
                group => group.Key,
                group => group
                    .Select(item => item.RoleName ?? string.Empty)
                    .Where(role => !string.IsNullOrWhiteSpace(role))
                    .OrderBy(role => role)
                    .ToList());
    }

    private async Task<List<string>> GetUserRolesAsync(string userId)
    {
        var roleNames = await (
            from userRole in _context.UserRoles
            join role in _context.Roles on userRole.RoleId equals role.Id
            where userRole.UserId == userId
            select role.Name)
            .ToListAsync();

        return roleNames
            .Where(role => !string.IsNullOrWhiteSpace(role))
            .Select(role => role!)
            .OrderBy(role => role)
            .ToList();
    }

    private async Task<List<SelectListItem>> BuildRoleOptionsAsync(IEnumerable<string>? selectedRoles = null)
    {
        var selected = (selectedRoles ?? [])
            .Where(role => !string.IsNullOrWhiteSpace(role))
            .Select(role => role.Trim())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var roles = await _roleManager.Roles
            .AsNoTracking()
            .OrderBy(role => role.Name)
            .Select(role => role.Name)
            .Where(role => role != null)
            .Select(role => role!)
            .ToListAsync();

        if (!roles.Any())
        {
            roles = [.. AppRoles.All];
        }

        return roles
            .Select(role => new SelectListItem
            {
                Text = role,
                Value = role,
                Selected = selected.Contains(role)
            })
            .ToList();
    }

    private async Task<bool> ValidateRolesAsync(IEnumerable<string> roles)
    {
        foreach (var role in roles)
        {
            if (!await _roleManager.RoleExistsAsync(role))
            {
                return false;
            }
        }

        return true;
    }

    private static List<string> NormalizeRoles(IEnumerable<string>? roles)
    {
        if (roles is null)
        {
            return [];
        }

        return roles
            .Where(role => !string.IsNullOrWhiteSpace(role))
            .Select(role => role.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static bool IsUserLockedOut(ApplicationUser user)
    {
        return user.LockoutEnabled
            && user.LockoutEnd.HasValue
            && user.LockoutEnd.Value > DateTimeOffset.UtcNow;
    }

    private void AddErrorsToModelState(IdentityResult result)
    {
        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }
    }
}
