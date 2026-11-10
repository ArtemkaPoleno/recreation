using System.ComponentModel.DataAnnotations;

namespace RecreationBase.Models.ViewModels;

public class RegisterViewModel
{
    [Required(ErrorMessage = "Укажите имя")]
    [StringLength(100, ErrorMessage = "Имя не длиннее 100 символов")]
    [Display(Name = "ФИО")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Укажите email")]
    [EmailAddress(ErrorMessage = "Некорректный email")]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Phone(ErrorMessage = "Некорректный телефон")]
    [Display(Name = "Телефон")]
    public string? Phone { get; set; }

    [Required(ErrorMessage = "Укажите пароль")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Пароль от 6 символов")]
    [DataType(DataType.Password)]
    [Display(Name = "Пароль")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Повторите пароль")]
    [DataType(DataType.Password)]
    [Compare(nameof(Password), ErrorMessage = "Пароли не совпадают")]
    [Display(Name = "Подтверждение пароля")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
