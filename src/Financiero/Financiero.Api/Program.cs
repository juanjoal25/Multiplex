using Financiero.Api.Filters;
using Financiero.Application;
using Financiero.Infrastructure;
using Financiero.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers(o => o.Filters.Add<DomainExceptionFilter>());
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();
builder.Services.AddFinancieroApplication();
builder.Services.AddFinancieroInfrastructure(builder.Configuration);

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<FinancieroDbContext>();
    db.Database.EnsureCreated();
}
app.UseHttpsRedirection();
app.MapControllers();
app.Run();
