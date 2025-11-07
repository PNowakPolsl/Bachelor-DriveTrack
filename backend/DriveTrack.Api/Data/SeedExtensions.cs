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
                new FuelType { Id = Guid.NewGuid(), Name = "Petrol", DefaultUnit = "L" },
                new FuelType { Id = Guid.NewGuid(), Name = "LPG", DefaultUnit = "L" },
                new FuelType { Id = Guid.NewGuid(), Name = "Electricity", DefaultUnit = "kWh" }
            );
            await db.SaveChangesAsync();
        }
        
        if (!await db.Categories.AnyAsync())
        {
            db.Categories.AddRange(
                new Category { Id = Guid.NewGuid(), Name = "Insurance" },
                new Category { Id = Guid.NewGuid(), Name = "Service" },
                new Category { Id = Guid.NewGuid(), Name = "Wash" },
                new Category { Id = Guid.NewGuid(), Name = "Parking" },
                new Category { Id = Guid.NewGuid(), Name = "Tolls" }
            );
            await db.SaveChangesAsync();
        }

    }
}