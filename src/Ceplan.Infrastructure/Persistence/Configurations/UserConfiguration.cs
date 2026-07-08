using Ceplan.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ceplan.Infrastructure.Persistence.Configurations;

/// <summary>Mapeo de la entidad <see cref="User"/> a la tabla Users.</summary>
public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");
        builder.HasKey(u => u.Id);

        builder.Property(u => u.DocumentType).HasConversion<int>().IsRequired();
        builder.Property(u => u.DocumentNumber).HasMaxLength(20).IsRequired();
        builder.HasIndex(u => new { u.DocumentType, u.DocumentNumber }).IsUnique();

        builder.Property(u => u.PasswordHash).IsRequired();

        builder.Property(u => u.FirstName).HasMaxLength(100).IsRequired();
        builder.Property(u => u.FirstLastName).HasMaxLength(100).IsRequired();
        builder.Property(u => u.SecondLastName).HasMaxLength(100).IsRequired();
        builder.Property(u => u.Role).HasMaxLength(150).IsRequired();
        builder.Property(u => u.Organization).HasMaxLength(200).IsRequired();
        builder.Property(u => u.Nationality).HasMaxLength(100).IsRequired();
        builder.Property(u => u.Sex).HasMaxLength(20).IsRequired();
        builder.Property(u => u.PrimaryEmail).HasMaxLength(256).IsRequired();
        builder.Property(u => u.SecondaryEmail).HasMaxLength(256);
        builder.Property(u => u.MobilePhone).HasMaxLength(30);
        builder.Property(u => u.SecondaryPhone).HasMaxLength(30);
        builder.Property(u => u.ContractType).HasMaxLength(50).IsRequired();

        // DateOnly -> date (soportado por EF Core 8 + SQL Server)
        builder.Property(u => u.BirthDate).HasColumnType("date");
        builder.Property(u => u.ContractDate).HasColumnType("date");

        builder.Property(u => u.AvatarPath).HasMaxLength(400);

        builder.Property(u => u.IsActive).IsRequired();
        builder.Property(u => u.FailedAttempts).IsRequired();
        builder.Property(u => u.LockoutEndUtc);
    }
}
