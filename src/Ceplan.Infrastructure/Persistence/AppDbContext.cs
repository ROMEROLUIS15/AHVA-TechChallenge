using Ceplan.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Ceplan.Infrastructure.Persistence;

/// <summary>Contexto EF Core del portal. Aplica las configuraciones de entidad del ensamblado.</summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
