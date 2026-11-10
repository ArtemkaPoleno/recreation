using Microsoft.AspNetCore.Identity;

namespace RecreationBase.Models;

/// <summary>
/// Свой пользователь поверх стандартного IdentityUser.
/// Email и пароль уже есть в базовом классе.
/// </summary>
public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
}
