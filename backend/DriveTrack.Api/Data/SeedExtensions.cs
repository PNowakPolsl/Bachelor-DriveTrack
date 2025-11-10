using DriveTrack.Api.Data;
using DriveTrack.Api.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace DriveTrack.Api.Data;

public static class SeedExtensions
{
    public static async Task SeedAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await db.Database.MigrateAsync();

        if (!await db.FuelTypes.AnyAsync())
        {
            db.FuelTypes.AddRange(
                new FuelType { Id = Guid.NewGuid(), Name = "Diesel", DefaultUnit = "L" },
                new FuelType { Id = Guid.NewGuid(), Name = "Benzyna", DefaultUnit = "L" },
                new FuelType { Id = Guid.NewGuid(), Name = "LPG", DefaultUnit = "L" },
                new FuelType { Id = Guid.NewGuid(), Name = "Elektryczność", DefaultUnit = "kWh" }
            );
            await db.SaveChangesAsync();
        }
        
        if (!await db.Categories.AnyAsync())
        {
            db.Categories.AddRange(
                new Category { Id = Guid.NewGuid(), Name = "Paliwo" },
                new Category { Id = Guid.NewGuid(), Name = "Mechanik" },
                new Category { Id = Guid.NewGuid(), Name = "Przegląd" },
                new Category { Id = Guid.NewGuid(), Name = "Ubezpieczenie" },
                new Category { Id = Guid.NewGuid(), Name = "Inne" }
            );
            await db.SaveChangesAsync();
        }

    }
}