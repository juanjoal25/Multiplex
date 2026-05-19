using Microsoft.EntityFrameworkCore;

namespace Multiplex.Web.Auth;

public sealed class BffIdentityDbContext(DbContextOptions<BffIdentityDbContext> options)
    : DbContext(options)
{
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<User>(e =>
        {
            e.ToTable("Users");
            e.HasKey(x => x.Id);
            e.Property(x => x.Email).IsRequired().HasMaxLength(180);
            e.HasIndex(x => x.Email).IsUnique();
            e.Property(x => x.PasswordHash).IsRequired();
            e.Property(x => x.Role).IsRequired().HasMaxLength(16);
            e.Property(x => x.Nombre).HasMaxLength(120);
        });
    }
}

public static class BffIdentitySeed
{
    public static void EnsureAdmin(BffIdentityDbContext db, PasswordHasher hasher, IConfiguration cfg)
    {
        if (db.Users.Any(u => u.Role == "ADMIN")) return;

        var email = cfg["Bff:SeedAdmin:Email"] ?? "admin@frame.local";
        var pass = cfg["Bff:SeedAdmin:Password"] ?? "admin1234";

        db.Users.Add(new User
        {
            Email = email,
            PasswordHash = hasher.Hash(pass),
            Role = "ADMIN",
            Nombre = "Administrador FRAME"
        });
        db.SaveChanges();
    }
}
