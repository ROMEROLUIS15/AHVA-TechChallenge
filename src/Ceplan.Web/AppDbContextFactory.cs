using Ceplan.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Ceplan.Web;

/// <summary>
/// Factory de tiempo de diseño para `dotnet ef` (crear/aplicar migraciones) sin construir
/// el host de la app. Lee la connection string desde User Secrets / variables de entorno;
/// el fallback (sin secretos) solo sirve para 'migrations add', que no abre conexión.
/// </summary>
public sealed class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddUserSecrets<AppDbContextFactory>(optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("Default");
        if (string.IsNullOrWhiteSpace(connectionString) || connectionString.StartsWith("__"))
        {
            // Fallback SIN secretos (solo válido para generar migraciones, no para conectar).
            connectionString = "Server=localhost,1433;Database=CeplanAccessPortal;TrustServerCertificate=True";
        }

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        return new AppDbContext(options);
    }
}
