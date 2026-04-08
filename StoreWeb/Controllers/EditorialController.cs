using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StoreWeb.Models.Identity;

namespace StoreWeb.Controllers;

[Authorize(Roles = $"{AppRoles.Editor},{AppRoles.Admin}")]
public class EditorialController : Controller
{
    public IActionResult Index()
    {
        ViewData["Title"] = "Editoryal Alan";
        return View();
    }
}
