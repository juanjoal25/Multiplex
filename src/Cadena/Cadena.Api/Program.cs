using Cadena.Api.Filters;
using Cadena.Application;
using Cadena.Infrastructure;
using Cadena.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers(o => o.Filters.Add<DomainExceptionFilter>())
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter()));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();
builder.Services.AddCadenaApplication();
builder.Services.AddCadenaInfrastructure(builder.Configuration);

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<CadenaDbContext>();
    db.Database.EnsureCreated();
    await DataSeeder.SeedAsync(db);
}
app.UseHttpsRedirection();
app.MapControllers();
app.Run();
