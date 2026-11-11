using System.ComponentModel.DataAnnotations;

namespace RecreationBase.Models;

/// <summary>
/// Статусы заявки на бронирование.
/// </summary>
public enum BookingStatus
{
    New = 0,          // Новая
    Confirmed = 1,    // Подтверждена
    Cancelled = 2,    // Отменена
    Completed = 3     // Завершена
}

/// <summary>
/// Заявка на бронирование.
/// </summary>
public class BookingRecord
{
    public int Id { get; set; }

    /// <summary>Id пользователя Identity (может быть null — заявка как гость).</summary>
    [StringLength(450)]
    public string? UserId { get; set; }

    /// <summary>Навигация: пользователь, оформивший заявку (если авторизован).</summary>
    public ApplicationUser? User { get; set; }

    public int CabinId { get; set; }

    public Cabin? Cabin { get; set; }

    [Required(ErrorMessage = "Укажите имя")]
    [StringLength(100)]
    [Display(Name = "Имя и фамилия")]
    public string GuestName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Укажите телефон")]
    [Phone(ErrorMessage = "Некорректный телефон")]
    [Display(Name = "Телефон")]
    public string Phone { get; set; } = string.Empty;

    [EmailAddress(ErrorMessage = "Некорректный email")]
    [Display(Name = "Email")]
    public string? Email { get; set; }

    [DataType(DataType.Date)]
    [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy}")]
    [Display(Name = "Заезд")]
    public DateTime CheckIn { get; set; }

    [DataType(DataType.Date)]
    [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy}")]
    [Display(Name = "Выезд")]
    public DateTime CheckOut { get; set; }

    [Range(1, 30, ErrorMessage = "От 1 до 30 гостей")]
    [Display(Name = "Гостей")]
    public int Guests { get; set; } = 1;

    [Display(Name = "Баня")]
    public bool NeedSauna { get; set; }

    [Display(Name = "Инвентарь")]
    public bool NeedGear { get; set; }

    [Display(Name = "Питание")]
    public bool NeedFood { get; set; }

    [StringLength(1000)]
    [Display(Name = "Комментарий")]
    public string? Comment { get; set; }

    [Display(Name = "Сумма, ₽")]
    public decimal TotalPrice { get; set; }

    [Display(Name = "Статус")]
    public BookingStatus Status { get; set; } = BookingStatus.New;

    [Display(Name = "Создана")]
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    /// <summary>Количество ночей (не хранится в базе, считается из дат).</summary>
    [Display(Name = "Ночей")]
    public int Nights => Math.Max(1, (CheckOut.Date - CheckIn.Date).Days);
}
