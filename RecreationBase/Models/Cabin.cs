using System.ComponentModel.DataAnnotations;

namespace RecreationBase.Models;

/// <summary>
/// Домик (объект размещения), который можно забронировать.
/// </summary>
public class Cabin
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Укажите название")]
    [StringLength(80)]
    [Display(Name = "Название")]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    [Display(Name = "Описание")]
    public string? Description { get; set; }

    [Range(1, 30, ErrorMessage = "Вместимость от 1 до 30 гостей")]
    [Display(Name = "Вместимость")]
    public int Capacity { get; set; } = 2;

    [Range(0, 1_000_000, ErrorMessage = "Цена не может быть отрицательной")]
    [Display(Name = "Цена за ночь, ₽")]
    public decimal PricePerNight { get; set; }

    [Url(ErrorMessage = "Ссылка должна начинаться с http")]
    [Display(Name = "Фото (ссылка)")]
    public string? ImageUrl { get; set; }

    [Display(Name = "Показывать на сайте")]
    public bool IsActive { get; set; } = true;
}
