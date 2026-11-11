using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecreationBase.Data;
using RecreationBase.Models;

namespace RecreationBase.Controllers;

/// <summary>
/// Админка: заявки и домики. Доступ только для роли Admin.
/// </summary>
[Authorize(Roles = Roles.Admin)]
public class AdminController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public AdminController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    // ---------- Главная админки: сводка ----------

    public async Task<IActionResult> Index()
    {
        var today = DateTime.Today;

        ViewBag.TotalBookings = await _db.Bookings.CountAsync();
        ViewBag.NewBookings = await _db.Bookings.CountAsync(b => b.Status == BookingStatus.New);
        ViewBag.ConfirmedBookings = await _db.Bookings.CountAsync(b => b.Status == BookingStatus.Confirmed);
        ViewBag.ArrivalsToday = await _db.Bookings.CountAsync(b => b.CheckIn.Date == today && b.Status != BookingStatus.Cancelled);
        ViewBag.CabinsActive = await _db.Cabins.CountAsync(c => c.IsActive);
        ViewBag.UsersCount = await _userManager.Users.CountAsync();
        ViewBag.Revenue = await _db.Bookings
            .Where(b => b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Completed)
            .SumAsync(b => b.TotalPrice);

        var upcoming = await _db.Bookings
            .Include(b => b.Cabin)
            .Where(b => b.CheckOut.Date >= today && b.Status != BookingStatus.Cancelled)
            .OrderBy(b => b.CheckIn)
            .Take(6)
            .ToListAsync();

        return View(upcoming);
    }

    // ---------- Заявки ----------

    public async Task<IActionResult> Bookings(string? status, string? q, DateTime? from, DateTime? to)
    {
        // Статус приходит из select'а как текст ("New", "Confirmed"...).
        BookingStatus? parsed = Enum.TryParse<BookingStatus>(status, ignoreCase: true, out var s) ? s : null;

        var query = _db.Bookings
            .Include(b => b.Cabin)
            .Include(b => b.User)
            .AsQueryable();

        if (parsed.HasValue)
        {
            query = query.Where(b => b.Status == parsed.Value);
        }

        if (!string.IsNullOrWhiteSpace(q))
        {
            var needle = q.Trim().ToLower();
            query = query.Where(b =>
                b.GuestName.ToLower().Contains(needle) ||
                b.Phone.ToLower().Contains(needle) ||
                (b.Email != null && b.Email.ToLower().Contains(needle)));
        }

        if (from.HasValue)
        {
            query = query.Where(b => b.CheckOut.Date >= from.Value.Date);
        }

        if (to.HasValue)
        {
            query = query.Where(b => b.CheckIn <= to.Value.Date);
        }

        var bookings = await query.OrderByDescending(b => b.CreatedAtUtc).ToListAsync();

        ViewBag.Status = parsed;
        ViewBag.Query = q;
        ViewBag.From = from?.ToString("yyyy-MM-dd");
        ViewBag.To = to?.ToString("yyyy-MM-dd");

        return View(bookings);
    }

    public async Task<IActionResult> BookingDetails(int id)
    {
        var booking = await _db.Bookings
            .Include(b => b.Cabin)
            .Include(b => b.User)
            .FirstOrDefaultAsync(b => b.Id == id);

        if (booking == null)
        {
            return NotFound();
        }

        return View(booking);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetStatus(int id, BookingStatus status)
    {
        var booking = await _db.Bookings.FindAsync(id);
        if (booking == null)
        {
            return NotFound();
        }

        booking.Status = status;
        await _db.SaveChangesAsync();

        TempData["AdminMessage"] = $"Заявка №{booking.Id}: статус обновлён.";
        return RedirectToAction(nameof(Bookings));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteBooking(int id)
    {
        var booking = await _db.Bookings.FindAsync(id);
        if (booking == null)
        {
            return NotFound();
        }

        _db.Bookings.Remove(booking);
        await _db.SaveChangesAsync();

        TempData["AdminMessage"] = $"Заявка №{id} удалена.";
        return RedirectToAction(nameof(Bookings));
    }

    // ---------- Домики ----------

    public async Task<IActionResult> Cabins()
    {
        var cabins = await _db.Cabins.OrderBy(c => c.Name).ToListAsync();
        return View(cabins);
    }

    public async Task<IActionResult> CabinEdit(int? id)
    {
        if (id == null)
        {
            return View(new Cabin());
        }

        var cabin = await _db.Cabins.FindAsync(id.Value);
        if (cabin == null)
        {
            return NotFound();
        }

        return View(cabin);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CabinEdit(Cabin cabin)
    {
        if (!ModelState.IsValid)
        {
            return View(cabin);
        }

        if (cabin.Id == 0)
        {
            _db.Cabins.Add(cabin);
        }
        else
        {
            var existing = await _db.Cabins.FindAsync(cabin.Id);
            if (existing == null)
            {
                return NotFound();
            }

            existing.Name = cabin.Name;
            existing.Description = cabin.Description;
            existing.Capacity = cabin.Capacity;
            existing.PricePerNight = cabin.PricePerNight;
            existing.ImageUrl = cabin.ImageUrl;
            existing.IsActive = cabin.IsActive;
        }

        await _db.SaveChangesAsync();

        TempData["AdminMessage"] = cabin.Id == 0
            ? $"Домик «{cabin.Name}» добавлен."
            : $"Домик «{cabin.Name}» сохранён.";
        return RedirectToAction(nameof(Cabins));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CabinDelete(int id)
    {
        var cabin = await _db.Cabins.FindAsync(id);
        if (cabin == null)
        {
            return NotFound();
        }

        var hasBookings = await _db.Bookings.AnyAsync(b => b.CabinId == id);
        if (hasBookings)
        {
            // Не удаляем — иначе потеряем историю заявок. Снимаем с публикации.
            cabin.IsActive = false;
            await _db.SaveChangesAsync();
            TempData["AdminMessage"] = $"У «{cabin.Name}» есть заявки — домик скрыт с сайта, но не удалён.";
            return RedirectToAction(nameof(Cabins));
        }

        _db.Cabins.Remove(cabin);
        await _db.SaveChangesAsync();

        TempData["AdminMessage"] = $"Домик «{cabin.Name}» удалён.";
        return RedirectToAction(nameof(Cabins));
    }

    // ---------- Пользователи ----------

    public async Task<IActionResult> Users(string? q)
    {
        var query = _userManager.Users.AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
        {
            var s = q.Trim().ToLower();
            query = query.Where(u =>
                u.FullName.ToLower().Contains(s) ||
                u.Email!.ToLower().Contains(s) ||
                (u.PhoneNumber != null && u.PhoneNumber.ToLower().Contains(s)));
        }

        var users = await query.OrderBy(u => u.FullName).Take(200).ToListAsync();

        var ids = users.Select(u => u.Id).ToList();
        var rolesByUser = await _db.UserRoles
            .Join(_db.Roles, ur => ur.RoleId, r => r.Id, (ur, r) => new { ur.UserId, Role = r.Name })
            .Where(x => ids.Contains(x.UserId))
            .ToListAsync();

        var bookingsByUser = await _db.Bookings
            .Where(b => b.UserId != null && ids.Contains(b.UserId))
            .GroupBy(b => b.UserId!)
            .Select(g => new { UserId = g.Key, Count = g.Count() })
            .ToListAsync();

        var list = users.Select(u => new AdminUserRow
        {
            Id = u.Id,
            FullName = string.IsNullOrWhiteSpace(u.FullName) ? u.UserName ?? "—" : u.FullName,
            Email = u.Email ?? "—",
            Phone = u.PhoneNumber ?? "не указан",
            IsAdmin = rolesByUser.Any(r => r.UserId == u.Id && r.Role == Roles.Admin),
            Bookings = bookingsByUser.FirstOrDefault(b => b.UserId == u.Id)?.Count ?? 0
        }).ToList();

        ViewBag.Query = q;
        return View(list);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleAdmin(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return NotFound();
        }

        var selfId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (user.Id == selfId)
        {
            TempData["AdminMessage"] = "Себе права менять нельзя.";
            return RedirectToAction(nameof(Users));
        }

        if (await _userManager.IsInRoleAsync(user, Roles.Admin))
        {
            await _userManager.RemoveFromRoleAsync(user, Roles.Admin);
            TempData["AdminMessage"] = $"«{user.FullName}» больше не администратор.";
        }
        else
        {
            await _userManager.AddToRoleAsync(user, Roles.Admin);
            TempData["AdminMessage"] = $"«{user.FullName}» назначен администратором.";
        }

        return RedirectToAction(nameof(Users));
    }
}

/// <summary>Строка таблицы пользователей в админке.</summary>
public class AdminUserRow
{
    public string Id { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public DateTime? CreatedAt { get; set; }
    public bool IsAdmin { get; set; }
    public int Bookings { get; set; }
}
