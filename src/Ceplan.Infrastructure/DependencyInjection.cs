using Ceplan.Application.Abstractions;
using Ceplan.Application.Authentication;
using Ceplan.Infrastructure.Persistence;
using Ceplan.Infrastructure.Security;
using Ceplan.Infrastructure.Time;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Ceplan.Infrastructure;

/// <summary>Registro de los servicios de infraestructura en el contenedor de DI.</summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString));

        services.AddScoped<IUserRepository, EfUserRepository>();
        services.AddSingleton<IClock, SystemClock>();
        services.AddSingleton<IPasswordHasher, PasswordHasherAdapter>();

        services.AddScoped<IAuthenticationService, AuthenticationService>();

        return services;
    }
}
