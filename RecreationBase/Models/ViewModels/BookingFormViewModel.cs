using System.ComponentModel.DataAnnotations;

namespace RecreationBase.Models.ViewModels;

/// <summary>
/// Форма бронирования на странице «Бронирование».
/// </summary>
public class BookingFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Выберите домик")]
    [Display(Name = "Домик")]
    public int CabinId { get; set; }

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

    [Required(ErrorMessage = "Укажите дату заезда")]
    [DataType(DataType.Date)]
    [Display(Name = "Заезд")]
    public DateTime CheckIn { get; set; } = DateTime.Today;

    [Required(ErrorMessage = "Укажите дату выезда")]
    [DataType(DataType.Date)]
    [Display(Name = "Выезд")]
    public DateTime CheckOut { get; set; } = DateTime.Today.AddDays(1);

    [Range(1, 30, ErrorMessage = "От 1 до 30 гостей")]
    [Display(Name = "Гостей")]
    public int Guests { get; set; } = 2;

    [Display(Name = "Баня")]
    public bool NeedSauna { get; set; }

    [Display(Name = "Инвентарь")]
    public bool NeedGear { get; set; }

    [Display(Name = "Питание")]
    public bool NeedFood { get; set; }

    [StringLength(1000, ErrorMessage = "Комментарий не длиннее 1000 символов")]
    [Display(Name = "Комментарий")]
    public string? Comment { get; set; }

    /// <summary>Список домиков для выпадающего списка.</summary>
    public List<Cabin> Cabins { get; set; } = new();
}
