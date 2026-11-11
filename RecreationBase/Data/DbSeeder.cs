using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RecreationBase.Data;
using RecreationBase.Models;

namespace RecreationBase.Data;

/// <summary>
/// Наполнение базы при старте: домики по умолчанию, роль Admin и учётная запись администратора.
/// Удобно для учебного проекта — не нужно ничего создавать руками.
/// </summary>
public static class DbSeeder
{
    // Пароль администратора по умолчанию (меняется в appsettings.json, см. AdminSeed:Password).
    public const string DefaultAdminEmail = "admin@kochevnya.ru";
    public const string DefaultAdminPassword = "Admin#2026";

    public static async Task SeedAsync(IServiceProvider services, IConfiguration config)
    {
        var db = services.GetRequiredService<ApplicationDbContext>();
        await db.Database.MigrateAsync();

        // 1. Домики по умолчанию, если таблица пуста.
        if (!await db.Cabins.AnyAsync())
        {
            db.Cabins.AddRange(
                new Cabin
                {
                    Name = "Тайга",
                    Description = "Двухместный домик у воды с террасой. Печка, свой выход к берегу.",
                    Capacity = 2,
                    PricePerNight = 5500,
                    ImageUrl = "/images/cabins.jpg"
                },
                new Cabin
                {
                    Name = "Сосны",
                    Description = "Просторный домик для компании до четырёх человек среди сосен.",
                    Capacity = 4,
                    PricePerNight = 7500,
                    ImageUrl = "/images/cabins-forest.jpg"
                },
                new Cabin
                {
                    Name = "Пирс",
                    Description = "Домик на шесть гостей с большим окном на озеро и пирс рядом.",
                    Capacity = 6,
                    PricePerNight = 11000,
                    ImageUrl = "/images/dock.jpg"
                });

            await db.SaveChangesAsync();
        }

        // 2. Роль администратора.
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        if (!await roleManager.RoleExistsAsync(Roles.Admin))
        {
            await roleManager.CreateAsync(new IdentityRole(Roles.Admin));
        }

        // 3. Учётная запись администратора.
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var adminEmail = config["AdminSeed:Email"] ?? DefaultAdminEmail;
        var adminPassword = config["AdminSeed:Password"] ?? DefaultAdminPassword;

        var admin = await userManager.FindByEmailAsync(adminEmail);
        if (admin == null)
        {
            admin = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true,
                FullName = "Администратор «Кочевни»"
            };

            var result = await userManager.CreateAsync(admin, adminPassword);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(admin, Roles.Admin);
            }
        }
        else if (!await userManager.IsInRoleAsync(admin, Roles.Admin))
        {
            await userManager.AddToRoleAsync(admin, Roles.Admin);
        }
    }
}
