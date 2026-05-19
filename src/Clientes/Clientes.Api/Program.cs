using Clientes.Api.Filters;
using Clientes.Application;
using Clientes.Infrastructure;
using Clientes.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers(o => o.Filters.Add<DomainExceptionFilter>());
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

builder.Services.AddClientesApplication();
builder.Services.AddClientesInfrastructure(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<ClientesDbContext>();
    db.Database.EnsureCreated();
    await DataSeeder.SeedAsync(db);
}

app.UseHttpsRedirection();
app.MapControllers();
app.Run();
