using Programacion.Api.Filters;
using Programacion.Application;
using Programacion.Infrastructure;
using Programacion.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers(o => o.Filters.Add<DomainExceptionFilter>());
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

builder.Services.AddProgramacionApplication();
builder.Services.AddProgramacionInfrastructure(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<ProgramacionDbContext>();
    db.Database.EnsureCreated();
    await DataSeeder.SeedAsync(db);
}

app.UseHttpsRedirection();
app.MapControllers();
app.Run();
