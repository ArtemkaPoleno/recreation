using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecreationBase.Data;
using RecreationBase.Models;
using RecreationBase.Models.ViewModels;

namespace RecreationBase.Controllers;

/// <summary>
/// Страница бронирования: витрина домиков + форма заявки.
/// </summary>
public class BookingController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public BookingController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<IActionResult> Index(DateTime? checkIn, DateTime? checkOut)
    {
        var cabins = await _db.Cabins
            .Where(c => c.IsActive)
            .OrderBy(c => c.PricePerNight)
            .ToListAsync();

        var user = await _userManager.GetUserAsync(User);

        var model = new BookingFormViewModel
        {
            Cabins = cabins,
            CheckIn = checkIn ?? DateTime.Today,
            CheckOut = checkOut ?? DateTime.Today.AddDays(1),
            GuestName = user?.FullName ?? string.Empty,
            Email = user?.Email,
            Phone = user?.PhoneNumber ?? string.Empty
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(BookingFormViewModel model)
    {
        // Подновляем список домиков для select'а, если проверка не прошла.
        model.Cabins = await _db.Cabins
            .Where(c => c.IsActive)
            .OrderBy(c => c.PricePerNight)
            .ToListAsync();

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var cabin = await _db.Cabins.FirstOrDefaultAsync(c => c.Id == model.CabinId && c.IsActive);
        if (cabin == null)
        {
            ModelState.AddModelError(nameof(model.CabinId), "Выберите доступный домик");
            return View(model);
        }

        if (model.CheckOut <= model.CheckIn)
        {
            ModelState.AddModelError(nameof(model.CheckOut), "Дата выезда должна быть позже даты заезда");
            return View(model);
        }

        if (model.Guests > cabin.Capacity)
        {
            ModelState.AddModelError(nameof(model.Guests),
                $"В домике «{cabin.Name}» мест не больше {cabin.Capacity}");
            return View(model);
        }

        // Пересечение с уже забронированными датами этого домика (отменённые не считаем).
        var busy = await _db.Bookings.AnyAsync(b =>
            b.CabinId == cabin.Id
            && b.Status != BookingStatus.Cancelled
            && b.CheckIn < model.CheckOut.Date
            && b.CheckOut.Date > model.CheckIn.Date);

        if (busy)
        {
            ModelState.AddModelError(nameof(model.CheckIn),
                $"«{cabin.Name}» уже занят на выбранные даты. Попробуйте другие.");
            return View(model);
        }

        var nights = Math.Max(1, (model.CheckOut.Date - model.CheckIn.Date).Days);
        var user = await _userManager.GetUserAsync(User);

        var booking = new BookingRecord
        {
            UserId = user?.Id,
            CabinId = cabin.Id,
            GuestName = model.GuestName.Trim(),
            Phone = model.Phone.Trim(),
            Email = string.IsNullOrWhiteSpace(model.Email) ? null : model.Email.Trim(),
            CheckIn = model.CheckIn.Date,
            CheckOut = model.CheckOut.Date,
            Guests = model.Guests,
            NeedSauna = model.NeedSauna,
            NeedGear = model.NeedGear,
            NeedFood = model.NeedFood,
            Comment = string.IsNullOrWhiteSpace(model.Comment) ? null : model.Comment.Trim(),
            TotalPrice = nights * cabin.PricePerNight,
            Status = BookingStatus.New,
            CreatedAtUtc = DateTime.UtcNow
        };

        _db.Bookings.Add(booking);
        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(Success), new { id = booking.Id });
    }

    [HttpGet]
    public async Task<IActionResult> Success(int id)
    {
        var booking = await _db.Bookings
            .Include(b => b.Cabin)
            .FirstOrDefaultAsync(b => b.Id == id);

        if (booking == null)
        {
            return NotFound();
        }

        return View(booking);
    }

    /// <summary>Свободные даты выбранного домика в формате JSON для календаря формы.</summary>
    [HttpGet]
    public async Task<IActionResult> AvailableDates(int cabinId)
    {
        var taken = await _db.Bookings
            .Where(b => b.CabinId == cabinId && b.Status != BookingStatus.Cancelled)
            .Select(b => new { b.CheckIn, b.CheckOut })
            .ToListAsync();

        return Json(taken.Select(t => new
        {
            from = t.CheckIn.ToString("yyyy-MM-dd"),
            to = t.CheckOut.ToString("yyyy-MM-dd")
        }));
    }
}
