using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecreationBase.Data;
using RecreationBase.Models;

namespace RecreationBase.Controllers;

[Authorize]
public class CabinetController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _db;

    public CabinetController(UserManager<ApplicationUser> userManager, ApplicationDbContext db)
    {
        _userManager = userManager;
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return RedirectToAction("Login", "Account");
        }

        // Заявки пользователя: по привязанному UserId и по email на случай гостевых броней.
        var bookings = await _db.Bookings
            .Include(b => b.Cabin)
            .Where(b => b.UserId == user.Id ||
                        (b.UserId == null && b.Email != null && b.Email == user.Email))
            .OrderByDescending(b => b.CheckIn)
            .ToListAsync();

        ViewBag.User = user;
        return View(bookings);
    }
}
