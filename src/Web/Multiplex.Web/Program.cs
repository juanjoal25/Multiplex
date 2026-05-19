using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Multiplex.Web.Auth;
using Multiplex.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// ── MVC + Session ──
builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(o =>
{
    o.Cookie.Name = "multiplex.session";
    o.Cookie.HttpOnly = true;
    o.Cookie.IsEssential = true;
    o.IdleTimeout = TimeSpan.FromHours(2);
});

// ── BFF Identity (SQLite) ──
var bffConn = builder.Configuration["Bff:ConnectionString"]!;
Directory.CreateDirectory(Path.Combine(AppContext.BaseDirectory, "Data"));
builder.Services.AddDbContext<BffIdentityDbContext>(o => o.UseSqlite(bffConn));
builder.Services.AddSingleton<PasswordHasher>();
builder.Services.AddSingleton<JwtTokenService>();

// ── Auth: JWT pulled from HttpOnly cookie ──
var jwtKey = builder.Configuration["Bff:Jwt:SigningKey"]!;
var jwtIssuer = builder.Configuration["Bff:Jwt:Issuer"]!;
var jwtAudience = builder.Configuration["Bff:Jwt:Audience"]!;
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.FromMinutes(1)
        };
        o.Events = new JwtBearerEvents
        {
            OnMessageReceived = ctx =>
            {
                if (ctx.Request.Cookies.TryGetValue("auth_token", out var token) &&
                    !string.IsNullOrWhiteSpace(token))
                {
                    ctx.Token = token;
                }
                return Task.CompletedTask;
            },
            OnChallenge = ctx =>
            {
                // For browser navigation, redirect to /auth/login instead of 401
                if (!ctx.Request.Path.StartsWithSegments("/api"))
                {
                    ctx.HandleResponse();
                    var returnUrl = Uri.EscapeDataString(ctx.Request.Path + ctx.Request.QueryString);
                    ctx.Response.Redirect($"/auth/login?returnUrl={returnUrl}");
                }
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization(o =>
{
    o.AddPolicy("RequireCliente", p => p.RequireAuthenticatedUser().RequireRole("CLIENTE", "ADMIN"));
    o.AddPolicy("RequireAdmin", p => p.RequireAuthenticatedUser().RequireRole("ADMIN"));
});

// ── Typed HttpClients for the 6 microservices ──
builder.Services.AddTransient<EspectadorIdHandler>();

void AddSvc<TClient, TImpl>(string section)
    where TClient : class
    where TImpl : class, TClient
{
    builder.Services
        .AddHttpClient<TClient, TImpl>(c =>
        {
            c.BaseAddress = new Uri(builder.Configuration[$"Services:{section}"]!);
            c.Timeout = TimeSpan.FromSeconds(15);
        })
        .AddHttpMessageHandler<EspectadorIdHandler>();
}

AddSvc<IClientesClient, ClientesClient>("Clientes");
AddSvc<IProgramacionClient, ProgramacionClient>("Programacion");
AddSvc<IInfraestructuraClient, InfraestructuraClient>("Infraestructura");
AddSvc<IVentasClient, VentasClient>("Ventas");
AddSvc<IFinancieroClient, FinancieroClient>("Financiero");
AddSvc<ICadenaClient, CadenaClient>("Cadena");

var app = builder.Build();

// ── DB init + admin seed ──
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<BffIdentityDbContext>();
    db.Database.EnsureCreated();
    var hasher = scope.ServiceProvider.GetRequiredService<PasswordHasher>();
    BffIdentitySeed.EnsureAdmin(db, hasher, app.Configuration);
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
