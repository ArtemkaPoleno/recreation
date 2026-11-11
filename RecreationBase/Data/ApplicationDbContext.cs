using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RecreationBase.Models;

namespace RecreationBase.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public DbSet<Cabin> Cabins => Set<Cabin>();
    public DbSet<BookingRecord> Bookings => Set<BookingRecord>();

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<BookingRecord>(e =>
        {
            e.HasOne(b => b.User)
                .WithMany()
                .HasForeignKey(b => b.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            e.HasOne(b => b.Cabin)
                .WithMany()
                .HasForeignKey(b => b.CabinId)
                .OnDelete(DeleteBehavior.Restrict);

            // Заявки по датам — частый запрос и в кабинете, и в админке.
            e.HasIndex(b => b.CheckIn);
            e.HasIndex(b => b.Status);
        });
        
    }
}
