using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RecreationBase.Models;

namespace RecreationBase.Controllers;

[Authorize]
public class CabinetController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;

    public CabinetController(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return RedirectToAction("Login", "Account");
        }

        return View(user);
    }
}
