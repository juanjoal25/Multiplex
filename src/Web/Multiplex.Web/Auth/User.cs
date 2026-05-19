namespace Multiplex.Web.Auth;

public sealed class User
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string Email { get; set; }
    public required string PasswordHash { get; set; }
    public required string Role { get; set; } // CLIENTE | ADMIN
    public Guid? IdEspectador { get; set; }
    public string? Nombre { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
