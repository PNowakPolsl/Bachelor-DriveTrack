using DriveTrack.Api.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace DriveTrack.Api.Data;

public static class SeedExtensions
{
    public static async Task SeedAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var db  = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var env = scope.ServiceProvider.GetRequiredService<IHostEnvironment>();

        if (db.Database.IsRelational())
        {
            await db.Database.MigrateAsync();
        }
        else
        {
            await db.Database.EnsureCreatedAsync();
        }


        await EnsureFuelTypesAsync(db);
        await EnsureCategoriesAsync(db);

        if (!env.IsEnvironment("Testing"))
        {
            await SeedDemoDataAsync(db);
        }
    }


    // ----------------- FUEL TYPES & CATEGORIES -----------------

    private static async Task EnsureFuelTypesAsync(AppDbContext db)
    {
        if (await db.FuelTypes.AnyAsync()) return;

        db.FuelTypes.AddRange(
            new FuelType { Id = Guid.NewGuid(), Name = "Diesel",        DefaultUnit = "L"   },
            new FuelType { Id = Guid.NewGuid(), Name = "Benzyna",       DefaultUnit = "L"   },
            new FuelType { Id = Guid.NewGuid(), Name = "LPG",           DefaultUnit = "L"   },
            new FuelType { Id = Guid.NewGuid(), Name = "Elektryczność", DefaultUnit = "kWh" }
        );
        await db.SaveChangesAsync();
    }

    private static async Task EnsureCategoriesAsync(AppDbContext db)
    {
        if (await db.Categories.AnyAsync()) return;

        db.Categories.AddRange(
            new Category { Id = Guid.NewGuid(), Name = "Paliwo" },
            new Category { Id = Guid.NewGuid(), Name = "Mechanik" },
            new Category { Id = Guid.NewGuid(), Name = "Przegląd" },
            new Category { Id = Guid.NewGuid(), Name = "Ubezpieczenie" },
            new Category { Id = Guid.NewGuid(), Name = "Inne" }
        );
        await db.SaveChangesAsync();
    }

    // ----------------- DEMO DATA -----------------

    private static async Task SeedDemoDataAsync(AppDbContext db)
    {
        const string demoEmail1 = "demo1@drivetrack.local";
        if (await db.Users.AnyAsync(u => u.Email == demoEmail1))
            return;

        var rand = new Random(42);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var user1 = new AppUser
        {
            Id = Guid.NewGuid(),
            Email = demoEmail1,
            Name = "Demo Kierowca",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Demo123!"),
            CreatedAt = DateTime.UtcNow
        };

        var user2 = new AppUser
        {
            Id = Guid.NewGuid(),
            Email = "demo2@drivetrack.local",
            Name = "Demo Współużytkownik",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Demo123!"),
            CreatedAt = DateTime.UtcNow
        };

        db.Users.AddRange(user1, user2);
        await db.SaveChangesAsync();

        var fuelTypes = await db.FuelTypes.ToListAsync();
        FuelType? ftDiesel  = fuelTypes.FirstOrDefault(f => f.Name == "Diesel");
        FuelType? ftPb      = fuelTypes.FirstOrDefault(f => f.Name == "Benzyna");
        FuelType? ftLpg     = fuelTypes.FirstOrDefault(f => f.Name == "LPG");
        FuelType? ftEv      = fuelTypes.FirstOrDefault(f => f.Name == "Elektryczność");

        var categories = await db.Categories.ToListAsync();
        var catFuel   = categories.First(c => c.Name == "Paliwo");
        var catMech   = categories.First(c => c.Name == "Mechanik");
        var catInspect= categories.First(c => c.Name == "Przegląd");
        var catIns    = categories.First(c => c.Name == "Ubezpieczenie");
        var catOther  = categories.First(c => c.Name == "Inne");

        var vehicles = new List<Vehicle>();

        var audi = new Vehicle
        {
            Id = Guid.NewGuid(),
            Name = "Audi A4 B8",
            Make = "Audi",
            Model = "A4",
            Plate = "WX 12345",
            Year = 2014,
            Vin = "WAUZZZ8K9EA000001",
            BaseOdometerKm = 180_000,
            CreatedBy = user1.Id,
            CreatedAt = DateTime.UtcNow
        };
        vehicles.Add(audi);

        var octavia = new Vehicle
        {
            Id = Guid.NewGuid(),
            Name = "Octavia Kombi",
            Make = "Skoda",
            Model = "Octavia",
            Plate = "WX 56789",
            Year = 2017,
            Vin = "TMBJG7NE9H0012345",
            BaseOdometerKm = 145_000,
            CreatedBy = user1.Id,
            CreatedAt = DateTime.UtcNow
        };
        vehicles.Add(octavia);

        var tesla = new Vehicle
        {
            Id = Guid.NewGuid(),
            Name = "Model 3 LR",
            Make = "Tesla",
            Model = "Model 3",
            Plate = "EL 1234T",
            Year = 2021,
            Vin = "5YJ3E7EB1MF000001",
            BaseOdometerKm = 45_000,
            CreatedBy = user1.Id,
            CreatedAt = DateTime.UtcNow
        };
        vehicles.Add(tesla);

        var fiat = new Vehicle
        {
            Id = Guid.NewGuid(),
            Name = "Fiat 500 Miejski",
            Make = "Fiat",
            Model = "500",
            Plate = "WX 9F500",
            Year = 2012,
            Vin = "ZFA3120000J000001",
            BaseOdometerKm = 120_000,
            CreatedBy = user2.Id,
            CreatedAt = DateTime.UtcNow
        };
        vehicles.Add(fiat);

        db.Vehicles.AddRange(vehicles);
        await db.SaveChangesAsync();

        // ---- UserVehicle ----
        var links = new List<UserVehicle>
        {
            new() { UserId = user1.Id, VehicleId = audi.Id,   Role = VehicleRole.Owner },
            new() { UserId = user1.Id, VehicleId = octavia.Id,Role = VehicleRole.Owner },
            new() { UserId = user1.Id, VehicleId = tesla.Id,  Role = VehicleRole.Owner },

            new() { UserId = user2.Id, VehicleId = fiat.Id,   Role = VehicleRole.Owner },
            new() { UserId = user2.Id, VehicleId = audi.Id,   Role = VehicleRole.Driver }
        };

        db.UserVehicles.AddRange(links);
        await db.SaveChangesAsync();

        // ---- VehicleFuelType ----
        var vFuelLinks = new List<VehicleFuelType>();

        if (ftPb    != null) vFuelLinks.Add(new VehicleFuelType { Id = Guid.NewGuid(), VehicleId = audi.Id,   FuelTypeId = ftPb.Id });
        if (ftDiesel!= null) vFuelLinks.Add(new VehicleFuelType { Id = Guid.NewGuid(), VehicleId = octavia.Id, FuelTypeId = ftDiesel.Id });
        if (ftEv    != null) vFuelLinks.Add(new VehicleFuelType { Id = Guid.NewGuid(), VehicleId = tesla.Id,  FuelTypeId = ftEv.Id });
        if (ftPb    != null) vFuelLinks.Add(new VehicleFuelType { Id = Guid.NewGuid(), VehicleId = fiat.Id,   FuelTypeId = ftPb.Id });
        if (ftLpg   != null) vFuelLinks.Add(new VehicleFuelType { Id = Guid.NewGuid(), VehicleId = fiat.Id,   FuelTypeId = ftLpg.Id });

        db.VehicleFuelTypes.AddRange(vFuelLinks);
        await db.SaveChangesAsync();

        // ---- Tankowania + wydatki ----
        if (ftPb != null)
            GenerateFuelHistoryWithExpenses(db, rand, audi,   ftPb,    catFuel, user1.Name, today);
        if (ftDiesel != null)
            GenerateFuelHistoryWithExpenses(db, rand, octavia,ftDiesel,catFuel, user1.Name, today);
        if (ftEv != null)
            GenerateEvHistoryWithExpenses(db, rand, tesla,   ftEv,    catFuel, user1.Name, today);
        if (ftPb != null)
            GenerateFuelHistoryWithExpenses(db, rand, fiat,  ftPb,    catFuel, user2.Name, today);

        // ---- Inne wydatki ----
        GenerateServiceExpenses(db, rand, audi,    catMech, catInspect, catIns, catOther, user1.Name, today);
        GenerateServiceExpenses(db, rand, octavia, catMech, catInspect, catIns, catOther, user1.Name, today);
        GenerateServiceExpenses(db, rand, tesla,   catMech, catInspect, catIns, catOther, user1.Name, today);
        GenerateServiceExpenses(db, rand, fiat,    catMech, catInspect, catIns, catOther, user2.Name, today);

        // ---- Przypomnienia ----
        GenerateReminders(db, audi,    user1.Name, today);
        GenerateReminders(db, octavia, user1.Name, today);
        GenerateReminders(db, tesla,   user1.Name, today);
        GenerateReminders(db, fiat,    user2.Name, today);

        await db.SaveChangesAsync();
    }

    // ----------------- HELPERS: paliwo -----------------

    private static void GenerateFuelHistoryWithExpenses(
        AppDbContext db,
        Random rand,
        Vehicle vehicle,
        FuelType fuelType,
        Category fuelCategory,
        string createdByName,
        DateOnly today)
    {

        var startDate = today.AddMonths(-6);
        var currentDate = startDate;
        var odometer = (vehicle.BaseOdometerKm ?? 100_000);

        decimal minL, maxL, minPrice, maxPrice;

        if (fuelType.Name == "Diesel")
        {
            minL = 4.8m; maxL = 7.2m;
            minPrice = 6.5m; maxPrice = 7.5m;
        }
        else if (fuelType.Name == "LPG")
        {
            minL = 7.5m; maxL = 10.5m;
            minPrice = 3.0m; maxPrice = 3.8m;
        }
        else
        {
            minL = 6.5m; maxL = 9.5m;
            minPrice = 6.0m; maxPrice = 7.2m;
        }

        for (int i = 0; i < 12; i++)
        {
            var daysStep = rand.Next(10, 22);
            currentDate = currentDate.AddDays(daysStep);
            if (currentDate > today) break;

            var distance = rand.Next(250, 650);
            odometer += distance;

            var cons = RandomDecimal(rand, minL, maxL);
            var liters = cons * distance / 100m;
            var pricePerUnit = RandomDecimal(rand, minPrice, maxPrice);
            var totalCost = Math.Round(liters * pricePerUnit, 2);

            var entry = new FuelEntry
            {
                Id = Guid.NewGuid(),
                VehicleId = vehicle.Id,
                FuelTypeId = fuelType.Id,
                Date = currentDate,
                Volume = Math.Round(liters, 3),
                Unit = fuelType.DefaultUnit,
                PricePerUnit = pricePerUnit,
                TotalCost = totalCost,
                OdometerKm = odometer,
                IsFullTank = true,
                Station = "Demo Stacja " + (rand.Next(1, 5)),
                CreatedAt = DateTime.UtcNow
            };
            db.FuelEntries.Add(entry);

            var expense = new Expense
            {
                Id = Guid.NewGuid(),
                VehicleId = vehicle.Id,
                CategoryId = fuelCategory.Id,
                Date = currentDate,
                Amount = totalCost,
                Description = $"Tankowanie {fuelType.Name}",
                OdometerKm = odometer,
                CreatedByName = createdByName,
                CreatedAt = DateTime.UtcNow
            };
            db.Expenses.Add(expense);
        }
    }

    private static void GenerateEvHistoryWithExpenses(
        AppDbContext db,
        Random rand,
        Vehicle vehicle,
        FuelType fuelType,
        Category fuelCategory,
        string createdByName,
        DateOnly today)
    {
        var startDate = today.AddMonths(-6);
        var currentDate = startDate;
        var odometer = (vehicle.BaseOdometerKm ?? 10_000);

        for (int i = 0; i < 12; i++)
        {
            var daysStep = rand.Next(9, 18);
            currentDate = currentDate.AddDays(daysStep);
            if (currentDate > today) break;

            var distance = rand.Next(250, 650);
            odometer += distance;

            var kwhPer100 = RandomDecimal(rand, 13m, 20m);
            var kwh = kwhPer100 * distance / 100m;
            var pricePerUnit = RandomDecimal(rand, 0.8m, 1.5m);
            var totalCost = Math.Round(kwh * pricePerUnit, 2);

            var entry = new FuelEntry
            {
                Id = Guid.NewGuid(),
                VehicleId = vehicle.Id,
                FuelTypeId = fuelType.Id,
                Date = currentDate,
                Volume = Math.Round(kwh, 3),
                Unit = fuelType.DefaultUnit,
                PricePerUnit = pricePerUnit,
                TotalCost = totalCost,
                OdometerKm = odometer,
                IsFullTank = true,
                Station = "Demo Ładowarka " + (rand.Next(1, 4)),
                CreatedAt = DateTime.UtcNow
            };
            db.FuelEntries.Add(entry);

            var expense = new Expense
            {
                Id = Guid.NewGuid(),
                VehicleId = vehicle.Id,
                CategoryId = fuelCategory.Id,
                Date = currentDate,
                Amount = totalCost,
                Description = "Ładowanie EV",
                OdometerKm = odometer,
                CreatedByName = createdByName,
                CreatedAt = DateTime.UtcNow
            };
            db.Expenses.Add(expense);
        }
    }

    // ----------------- HELPERS: inne wydatki -----------------

    private static void GenerateServiceExpenses(
        AppDbContext db,
        Random rand,
        Vehicle vehicle,
        Category mech,
        Category inspect,
        Category ins,
        Category other,
        string createdByName,
        DateOnly today)
    {
        var baseDate = today.AddMonths(-6);

        var inspectionDate = baseDate.AddDays(rand.Next(20, 70));
        db.Expenses.Add(new Expense
        {
            Id = Guid.NewGuid(),
            VehicleId = vehicle.Id,
            CategoryId = inspect.Id,
            Date = inspectionDate,
            Amount = RandomDecimal(rand, 200m, 400m),
            Description = "Okresowy przegląd",
            OdometerKm = (vehicle.BaseOdometerKm ?? 0) + rand.Next(2_000, 8_000),
            CreatedByName = createdByName,
            CreatedAt = DateTime.UtcNow
        });

        var insDate = baseDate.AddDays(rand.Next(10, 40));
        db.Expenses.Add(new Expense
        {
            Id = Guid.NewGuid(),
            VehicleId = vehicle.Id,
            CategoryId = ins.Id,
            Date = insDate,
            Amount = RandomDecimal(rand, 600m, 1800m),
            Description = "Ubezpieczenie OC/AC",
            OdometerKm = (vehicle.BaseOdometerKm ?? 0) + rand.Next(1_000, 5_000),
            CreatedByName = createdByName,
            CreatedAt = DateTime.UtcNow
        });

        var mechCount = rand.Next(1, 3);
        for (int i = 0; i < mechCount; i++)
        {
            var d = baseDate.AddDays(rand.Next(30, 140));
            db.Expenses.Add(new Expense
            {
                Id = Guid.NewGuid(),
                VehicleId = vehicle.Id,
                CategoryId = mech.Id,
                Date = d,
                Amount = RandomDecimal(rand, 300m, 1500m),
                Description = "Naprawa / serwis",
                OdometerKm = (vehicle.BaseOdometerKm ?? 0) + rand.Next(3_000, 15_000),
                CreatedByName = createdByName,
                CreatedAt = DateTime.UtcNow
            });
        }

        if (rand.NextDouble() < 0.5)
        {
            var d = baseDate.AddDays(rand.Next(15, 160));
            db.Expenses.Add(new Expense
            {
                Id = Guid.NewGuid(),
                VehicleId = vehicle.Id,
                CategoryId = other.Id,
                Date = d,
                Amount = RandomDecimal(rand, 50m, 400m),
                Description = "Myjnia / parking / drobne opłaty",
                OdometerKm = (vehicle.BaseOdometerKm ?? 0) + rand.Next(1_000, 10_000),
                CreatedByName = createdByName,
                CreatedAt = DateTime.UtcNow
            });
        }
    }

    // ----------------- HELPERS: przypomnienia -----------------

    private static void GenerateReminders(
        AppDbContext db,
        Vehicle vehicle,
        string createdByName,
        DateOnly today)
    {
        db.Reminders.Add(new Reminder
        {
            Id = Guid.NewGuid(),
            VehicleId = vehicle.Id,
            Title = "Przegląd techniczny",
            Description = "Roczny przegląd pojazdu",
            DueDate = today.AddDays(-10),
            IsCompleted = true,
            CompletedAt = DateTime.UtcNow.AddDays(-9),
            CreatedAt = DateTime.UtcNow.AddMonths(-1),
            CreatedByName = createdByName
        });

        db.Reminders.Add(new Reminder
        {
            Id = Guid.NewGuid(),
            VehicleId = vehicle.Id,
            Title = "Ubezpieczenie OC",
            Description = "Zbliża się termin odnowienia OC",
            DueDate = today.AddDays(7),
            IsCompleted = false,
            CompletedAt = null,
            CreatedAt = DateTime.UtcNow.AddDays(-5),
            CreatedByName = createdByName
        });

        db.Reminders.Add(new Reminder
        {
            Id = Guid.NewGuid(),
            VehicleId = vehicle.Id,
            Title = "Wymiana oleju",
            Description = "Zaplanowana wymiana oleju silnikowego",
            DueDate = today.AddDays(60),
            IsCompleted = false,
            CompletedAt = null,
            CreatedAt = DateTime.UtcNow,
            CreatedByName = createdByName
        });
    }

    // ----------------- HELPERS: util -----------------

    private static decimal RandomDecimal(Random rand, decimal min, decimal max)
    {
        var v = (decimal)rand.NextDouble();
        return Math.Round(min + (max - min) * v, 2);
    }
}
