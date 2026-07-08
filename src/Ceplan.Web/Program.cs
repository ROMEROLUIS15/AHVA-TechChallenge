using Ceplan.Application.Abstractions;
using Ceplan.Application.Options;
using Ceplan.Application.Profile;
using Ceplan.Infrastructure;
using Ceplan.Infrastructure.Persistence;
using Ceplan.Web.Storage;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// --- MVC ---
builder.Services.AddControllersWithViews();

// --- Infrastructure (EF Core, repos, hasher, clock, servicio de autenticación) ---
var connectionString = builder.Configuration.GetConnectionString("Default")
    ?? throw new InvalidOperationException(
        "ConnectionStrings:Default no configurada. Defínela en User Secrets o variables de entorno.");
builder.Services.AddInfrastructure(connectionString);

// --- Foto de perfil (extra): almacenamiento web + caso de uso ---
builder.Services.AddScoped<IAvatarStorage, WebAvatarStorage>();
builder.Services.AddScoped<IProfileImageService, ProfileImageService>();

// --- Opciones de bloqueo (no sensibles, desde appsettings) ---
builder.Services.Configure<LockoutOptions>(builder.Configuration.GetSection(LockoutOptions.SectionName));
builder.Services.AddSingleton(sp => sp.GetRequiredService<IOptions<LockoutOptions>>().Value);

builder.Services.Configure<SessionTimeoutOptions>(builder.Configuration.GetSection(SessionTimeoutOptions.SectionName));
builder.Services.AddSingleton(sp => sp.GetRequiredService<IOptions<SessionTimeoutOptions>>().Value);

// --- Autenticación por cookie (sin ASP.NET Core Identity) ---
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/Login";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
        options.SlidingExpiration = true;
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// --- Pipeline HTTP ---
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Manejo de códigos de estado (p. ej. 404) reutilizando la página de error.
app.UseStatusCodePagesWithReExecute("/Home/Error", "?code={0}");

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// La plataforma abre en la pantalla de Activación (capture 1); desde ahí
// "Iniciar sesión" redirige al Login, según el flujo del diseño (capture 9).
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Activation}/{id?}");

// --- Migración + seed al arrancar (aplica migraciones pendientes y crea el usuario semilla) ---
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
    var seedPassword = app.Configuration["Seed:Password"]
        ?? throw new InvalidOperationException("Seed:Password no configurado. Defínelo en User Secrets.");
    await DbSeeder.SeedAsync(db, hasher, seedPassword);
}

app.Run();
