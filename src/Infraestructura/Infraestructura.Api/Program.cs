using Infraestructura.Api.Filters;
using Infraestructura.Application;
using Infraestructura.Infrastructure;
using Infraestructura.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers(o => o.Filters.Add<DomainExceptionFilter>());
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();
builder.Services.AddInfraestructuraApplication();
builder.Services.AddInfraestructuraInfrastructure(builder.Configuration);

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<InfraestructuraDbContext>();
    db.Database.EnsureCreated();
}
app.UseHttpsRedirection();
app.MapControllers();
app.Run();
