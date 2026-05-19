using Microsoft.EntityFrameworkCore;
using Ventas.Api.Filters;
using Ventas.Application;
using Ventas.Infrastructure;
using Ventas.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers(o => o.Filters.Add<DomainExceptionFilter>());
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();
builder.Services.AddVentasApplication();
builder.Services.AddVentasInfrastructure(builder.Configuration);

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<VentasDbContext>();
    db.Database.EnsureCreated();
    await DataSeeder.SeedAsync(db);
}
app.UseHttpsRedirection();
app.MapControllers();
app.Run();
