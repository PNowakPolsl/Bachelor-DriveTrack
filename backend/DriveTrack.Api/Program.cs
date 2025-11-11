using DriveTrack.Api.Data;
using DriveTrack.Api.Data.Dto;
using DriveTrack.Api.Data.Entities;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddCors(opt =>
{
    opt.AddPolicy("frontend", p => p
        .WithOrigins("http://localhost:5173")
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials());
});


var app = builder.Build();

app.UseCors("frontend");


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/", () => "DriveTrack API is running");

app.MapGet("/db-health", async (AppDbContext db) =>
{
    try
    {
        var ok = await db.Database.CanConnectAsync();
        return ok ? Results.Ok("DB OK") : Results.Problem("DB NOT REACHABLE");
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message);
    }
});

// FuelTypes
app.MapGet("/fuel-types", async (AppDbContext db) =>
    await db.FuelTypes
        .OrderBy(x => x.Name)
        .Select(x => new { x.Id, x.Name, x.DefaultUnit, x.CreatedAt })
        .ToListAsync()
);

app.MapPost("/fuel-types", async (AppDbContext db, FuelType input) =>
{
    if (string.IsNullOrWhiteSpace(input.Name))
        return Results.BadRequest("Name is required.");
    if (string.IsNullOrWhiteSpace(input.DefaultUnit))
        return Results.BadRequest("DefaultUnit is required (L/kWh/galUS/galUK).");

    if (input.Id == Guid.Empty) input.Id = Guid.NewGuid();
    if (input.CreatedAt == default) input.CreatedAt = DateTime.UtcNow;

    db.FuelTypes.Add(input);
    await db.SaveChangesAsync();
    return Results.Created($"/fuel-types/{input.Id}", input);
});

// Vehicles
app.MapGet("/vehicles", async (AppDbContext db) =>
    await db.Vehicles
        .OrderBy(v => v.Make)
        .Select(v => new { v.Id, v.Name, v.Make, v.Model, v.Plate, v.Year })
        .ToListAsync()
);

app.MapPost("/vehicles", async (AppDbContext db, Vehicle input) =>
{
    if (string.IsNullOrWhiteSpace(input.Name))  
        return Results.BadRequest("Name is required.");
    if (string.IsNullOrWhiteSpace(input.Make))  
        return Results.BadRequest("Make is required.");
    if (string.IsNullOrWhiteSpace(input.Model)) 
        return Results.BadRequest("Model is required.");

    input.Id = Guid.NewGuid();
    input.CreatedAt = DateTime.UtcNow;
    db.Vehicles.Add(input);
    await db.SaveChangesAsync();
    return Results.Created($"/vehicles/{input.Id}", input);
});

// Vehicle <-> FuelTypes

app.MapPost("/vehicles/{vehicleId:guid}/fuel-types", async (AppDbContext db, Guid vehicleId, AssignFuelTypeRequest body) =>
{
    var existsVehicle = await db.Vehicles.AnyAsync(v => v.Id == vehicleId);
    if (!existsVehicle) return Results.NotFound("Vehicle not found.");

    var existsFuel = await db.FuelTypes.AnyAsync(f => f.Id == body.FuelTypeId);
    if (!existsFuel) return Results.BadRequest("FuelType not found.");

    var already = await db.VehicleFuelTypes
        .AnyAsync(x => x.VehicleId == vehicleId && x.FuelTypeId == body.FuelTypeId);
    if (already) return Results.Conflict("FuelType already assigned to this vehicle.");

    db.VehicleFuelTypes.Add(new VehicleFuelType
    {
        Id = Guid.NewGuid(),
        VehicleId = vehicleId,
        FuelTypeId = body.FuelTypeId
    });
    await db.SaveChangesAsync();
    return Results.NoContent();
});


app.MapDelete("/vehicles/{vehicleId:guid}/fuel-types/{fuelTypeId:guid}", async (AppDbContext db, Guid vehicleId, Guid fuelTypeId) =>
{
    var link = await db.VehicleFuelTypes
        .FirstOrDefaultAsync(x => x.VehicleId == vehicleId && x.FuelTypeId == fuelTypeId);
    if (link is null) return Results.NotFound();

    db.VehicleFuelTypes.Remove(link);
    await db.SaveChangesAsync();
    return Results.NoContent();
});


app.MapGet("/vehicles/{vehicleId:guid}", async (AppDbContext db, Guid vehicleId) =>
{
    var vehicle = await db.Vehicles
        .Where(v => v.Id == vehicleId)
        .Select(v => new
        {
            v.Id,
            v.Name,
            v.Make,
            v.Model,
            v.Plate,
            v.Year,
            FuelTypes = db.VehicleFuelTypes
                .Where(x => x.VehicleId == v.Id)
                .Join(db.FuelTypes, link => link.FuelTypeId, ft => ft.Id,
                      (link, ft) => new { ft.Id, ft.Name, ft.DefaultUnit })
                .OrderBy(ft => ft.Name)
                .ToList()
        })
        .FirstOrDefaultAsync();

    return vehicle is null ? Results.NotFound() : Results.Ok(vehicle);
});



app.MapPost("/vehicles/{vehicleId:guid}/fuel-entries", async (AppDbContext db, Guid vehicleId, CreateFuelEntryRequest body) =>
{
    var vehicle = await db.Vehicles.FindAsync(vehicleId);
    if (vehicle is null) return Results.NotFound("Vehicle not found.");

    var fuelType = await db.FuelTypes.FindAsync(body.FuelTypeId);
    if (fuelType is null) return Results.BadRequest("FuelType not found.");

    var supported = await db.VehicleFuelTypes
        .AnyAsync(x => x.VehicleId == vehicleId && x.FuelTypeId == body.FuelTypeId);
    if (!supported) return Results.BadRequest("This vehicle is not configured for the selected fuel type.");

    var unit = string.IsNullOrWhiteSpace(body.Unit) ? fuelType.DefaultUnit : body.Unit.Trim();

    if (body.Volume <= 0) return Results.BadRequest("Volume must be > 0.");
    if (body.PricePerUnit < 0) return Results.BadRequest("PricePerUnit must be >= 0.");
    if (body.OdometerKm < 0) return Results.BadRequest("OdometerKm must be >= 0.");

    var total = Math.Round(body.Volume * body.PricePerUnit, 2, MidpointRounding.AwayFromZero);

    var entry = new FuelEntry
    {
        Id = Guid.NewGuid(),
        VehicleId = vehicleId,
        FuelTypeId = body.FuelTypeId,
        Date = body.Date,
        Volume = body.Volume,
        Unit = unit,
        PricePerUnit = body.PricePerUnit,
        TotalCost = total,
        OdometerKm = body.OdometerKm,
        IsFullTank = body.IsFullTank,
        Station = body.Station,
        CreatedAt = DateTime.UtcNow
    };

    db.FuelEntries.Add(entry);
    await db.SaveChangesAsync();

    var fuelCategory = await db.Categories
        .Where(x => x.OwnerUserId == null && x.Name.ToLower() == "paliwo")
        .FirstOrDefaultAsync();

    Expense? createdExpense = null;

    if (fuelCategory != null)
    {
        createdExpense = new Expense
        {
            Id = Guid.NewGuid(),
            VehicleId = vehicleId,
            CategoryId = fuelCategory.Id,
            Date = body.Date,
            Amount = total,
            Description = string.IsNullOrWhiteSpace(body.Station)
                ? $"Tankowanie {fuelType.Name}"
                : $"Tankowanie {fuelType.Name} — {body.Station}",
            OdometerKm = body.OdometerKm,
            CreatedAt = DateTime.UtcNow
        };

        db.Expenses.Add(createdExpense);
        await db.SaveChangesAsync();
    }

    if (createdExpense != null)
    {
        var response = new
        {
            createdExpense.Id,
            createdExpense.Date,
            createdExpense.Amount,
            createdExpense.Description,
            createdExpense.OdometerKm,
            Category = new { CategoryId = createdExpense.CategoryId, Name = fuelCategory.Name }
        };
        return Results.Created($"/vehicles/{vehicleId}/expenses/{createdExpense.Id}", response);
    }

    return Results.Created($"/vehicles/{vehicleId}/fuel-entries/{entry.Id}", new
    {
        entry.Id, entry.Date, entry.Volume, entry.Unit, entry.PricePerUnit, entry.TotalCost, entry.OdometerKm, entry.IsFullTank, entry.Station
    });
});


app.MapGet("/vehicles/{vehicleId:guid}/fuel-entries", async (
    AppDbContext db, Guid vehicleId, DateOnly? from, DateOnly? to) =>
{
    var exists = await db.Vehicles.AnyAsync(v => v.Id == vehicleId);
    if (!exists) return Results.NotFound("Vehicle not found.");

    var q = db.FuelEntries
        .Where(x => x.VehicleId == vehicleId);

    if (from is not null) q = q.Where(x => x.Date >= from);
    if (to is not null) q = q.Where(x => x.Date <= to);

    var list = await q
        .OrderByDescending(x => x.Date)
        .ThenByDescending(x => x.OdometerKm)
        .Select(x => new
        {
            x.Id,
            x.Date,
            x.Volume,
            x.Unit,
            x.PricePerUnit,
            x.TotalCost,
            x.OdometerKm,
            x.IsFullTank,
            x.Station,
            FuelType = new { x.FuelTypeId }
        })
        .ToListAsync();

    return Results.Ok(list);
});

// Categories
app.MapGet("/categories", async (AppDbContext db, Guid? ownerUserId) =>
{
    var q = db.Categories.AsQueryable();
    if (ownerUserId is not null)
        q = q.Where(c => c.OwnerUserId == ownerUserId);
    else
        q = q.Where(c => c.OwnerUserId == null);

    var list = await q.OrderBy(c => c.Name)
        .Select(c => new { c.Id, c.Name, c.OwnerUserId, c.CreatedAt })
        .ToListAsync();

    return Results.Ok(list);
});

app.MapPost("/categories", async (AppDbContext db, CreateCategoryRequest body) =>
{
    if (string.IsNullOrWhiteSpace(body.Name))
        return Results.BadRequest("Name is required.");

    var exists = await db.Categories.AnyAsync(c =>
        c.Name.ToLower() == body.Name.ToLower() &&
        c.OwnerUserId == body.OwnerUserId);

    if (exists) return Results.Conflict("Category with this name already exists for this owner scope.");

    var cat = new Category
    {
        Id = Guid.NewGuid(),
        Name = body.Name.Trim(),
        OwnerUserId = body.OwnerUserId,
        CreatedAt = DateTime.UtcNow
    };

    db.Categories.Add(cat);
    await db.SaveChangesAsync();
    return Results.Created($"/categories/{cat.Id}", cat);
});

// Expenses
app.MapGet("/vehicles/{vehicleId:guid}/expenses", async (
    AppDbContext db, Guid vehicleId, DateOnly? from, DateOnly? to, Guid? categoryId) =>
{
    var exists = await db.Vehicles.AnyAsync(v => v.Id == vehicleId);
    if (!exists) return Results.NotFound("Vehicle not found.");

    var q = db.Expenses.Where(e => e.VehicleId == vehicleId);
    if (from is not null) q = q.Where(e => e.Date >= from);
    if (to   is not null) q = q.Where(e => e.Date <= to);
    if (categoryId is not null) q = q.Where(e => e.CategoryId == categoryId);

    var list = await q
        .OrderByDescending(e => e.Date)
        .Select(e => new {
            e.Id,
            e.Date,
            e.Amount,
            e.Description,
            e.OdometerKm,
            Category = new { CategoryId = e.CategoryId, Name = e.Category.Name } // ⬅️ TU DODALIŚMY NAZWĘ
        })
        .ToListAsync();

    return Results.Ok(list);
});


app.MapPost("/vehicles/{vehicleId:guid}/expenses", async (AppDbContext db, Guid vehicleId, CreateExpenseRequest body) =>
{
    var vehicle = await db.Vehicles.FindAsync(vehicleId);
    if (vehicle is null) return Results.NotFound("Vehicle not found.");

    var category = await db.Categories.FindAsync(body.CategoryId);
    if (category is null) return Results.BadRequest("Category not found.");

    if (body.Amount <= 0) return Results.BadRequest("Amount must be > 0.");

    var exp = new Expense
    {
        Id = Guid.NewGuid(),
        VehicleId = vehicleId,
        CategoryId = body.CategoryId,
        Date = body.Date,
        Amount = Math.Round(body.Amount, 2, MidpointRounding.AwayFromZero),
        Description = string.IsNullOrWhiteSpace(body.Description) ? null : body.Description.Trim(),
        OdometerKm = body.OdometerKm,
        CreatedAt = DateTime.UtcNow
    };

    db.Expenses.Add(exp);
    await db.SaveChangesAsync();

    return Results.Created(
    $"/vehicles/{vehicleId}/expenses/{exp.Id}",
        new {
            exp.Id,
            exp.Date,
            exp.Amount,
            exp.Description,
            exp.OdometerKm,
            Category = new { Id = exp.CategoryId, Name = category.Name }
        }
    );

});

// Reminders
app.MapGet("/vehicles/{vehicleId:guid}/reminders", async (
    AppDbContext db,
    Guid vehicleId,
    DateOnly? fromDue,
    DateOnly? toDue,
    bool? completed,
    bool? onlyOverdue,
    bool? onlyUpcoming) =>
{
    var exists = await db.Vehicles.AnyAsync(v => v.Id == vehicleId);
    if (!exists) return Results.NotFound("Vehicle not found.");

    if ((onlyOverdue ?? false) && (onlyUpcoming ?? false))
        return Results.BadRequest("Choose only one of: onlyOverdue or onlyUpcoming.");

    var q = db.Reminders.Where(r => r.VehicleId == vehicleId);

    if (fromDue is not null) q = q.Where(r => r.DueDate >= fromDue);
    if (toDue   is not null) q = q.Where(r => r.DueDate <= toDue);
    if (completed is not null) q = q.Where(r => r.IsCompleted == completed);

    var today = DateOnly.FromDateTime(DateTime.UtcNow);
    if (onlyOverdue  == true) q = q.Where(r => !r.IsCompleted && r.DueDate <  today);
    if (onlyUpcoming == true) q = q.Where(r => !r.IsCompleted && r.DueDate >= today);

    var list = await q
        .OrderBy(r => r.IsCompleted)
        .ThenBy(r => r.DueDate)
        .Select(r => new {
            r.Id, r.VehicleId, r.Title, r.Description, r.DueDate,
            r.IsCompleted, r.CompletedAt, r.CreatedAt
        })
        .ToListAsync();

    return Results.Ok(list);
});

app.MapPost("/vehicles/{vehicleId:guid}/reminders", async (AppDbContext db, Guid vehicleId, CreateReminderRequest body) =>
{
    var vehicle = await db.Vehicles.FindAsync(vehicleId);
    if (vehicle is null) return Results.NotFound("Vehicle not found.");

    if (string.IsNullOrWhiteSpace(body.Title))
        return Results.BadRequest("Title is required.");

    var reminder = new Reminder
    {
        Id = Guid.NewGuid(),
        VehicleId = vehicleId,
        Title = body.Title.Trim(),
        Description = string.IsNullOrWhiteSpace(body.Description) ? null : body.Description.Trim(),
        DueDate = body.DueDate,
        IsCompleted = false,
        CompletedAt = null,
        CreatedAt = DateTime.UtcNow
    };

    db.Reminders.Add(reminder);
    await db.SaveChangesAsync();

    return Results.Created($"/vehicles/{vehicleId}/reminders/{reminder.Id}", reminder);
});

app.MapPatch("/reminders/{reminderId:guid}", async (AppDbContext db, Guid reminderId, UpdateReminderRequest body) =>
{
    var r = await db.Reminders.FirstOrDefaultAsync(x => x.Id == reminderId);
    if (r is null) return Results.NotFound();

    if (body.Title is not null)
    {
        if (string.IsNullOrWhiteSpace(body.Title)) return Results.BadRequest("Title cannot be empty.");
        r.Title = body.Title.Trim();
    }

    if (body.Description is not null)
        r.Description = string.IsNullOrWhiteSpace(body.Description) ? null : body.Description.Trim();

    if (body.DueDate is not null)
        r.DueDate = body.DueDate.Value;

    if (body.IsCompleted is not null)
    {
        r.IsCompleted = body.IsCompleted.Value;
        r.CompletedAt = r.IsCompleted ? DateTime.UtcNow : null;
    }

    await db.SaveChangesAsync();
    return Results.NoContent();
});

app.MapDelete("/reminders/{reminderId:guid}", async (AppDbContext db, Guid reminderId) =>
{
    var r = await db.Reminders.FirstOrDefaultAsync(x => x.Id == reminderId);
    if (r is null) return Results.NotFound();

    db.Reminders.Remove(r);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

// Users
app.MapPost("/users", async (AppDbContext db, CreateUserRequest input) =>
{
    if (string.IsNullOrWhiteSpace(input.Email))    return Results.BadRequest("Email is required.");
    if (string.IsNullOrWhiteSpace(input.Password)) return Results.BadRequest("Password is required.");
    if (string.IsNullOrWhiteSpace(input.Name))     return Results.BadRequest("Name is required.");

    var email = input.Email.Trim().ToLowerInvariant();
    var exists = await db.Users.AnyAsync(u => u.Email == email);
    if (exists) return Results.Conflict("Email already in use.");

    // zrobić hash hasła
    var user = new AppUser
    {
        Id = Guid.NewGuid(),
        Email = email,
        PasswordHash = input.Password,
        Name = input.Name.Trim(),
        CreatedAt = DateTime.UtcNow
    };

    db.Users.Add(user);
    await db.SaveChangesAsync();

    return Results.Created($"/users/{user.Id}",
        new UserResponse(user.Id, user.Email, user.Name, user.CreatedAt));
});


app.MapGet("/users/{userId:guid}/vehicles", async (AppDbContext db, Guid userId) =>
{
    var exists = await db.Users.AnyAsync(u => u.Id == userId);
    if (!exists) return Results.NotFound("User not found.");

    var list = await db.UserVehicles
        .Where(uv => uv.UserId == userId)
        .Join(db.Vehicles, uv => uv.VehicleId, v => v.Id, (uv, v) => new {
            v.Id, v.Name, v.Make, v.Model, v.Plate, v.Year, Role = uv.Role.ToString()
        })
        .ToListAsync();

    return Results.Ok(list);
});

app.MapGet("/vehicles/{vehicleId:guid}/users", async (AppDbContext db, Guid vehicleId) =>
{
    var exists = await db.Vehicles.AnyAsync(v => v.Id == vehicleId);
    if (!exists) return Results.NotFound("Vehicle not found.");

    var list = await db.UserVehicles
        .Where(uv => uv.VehicleId == vehicleId)
        .Join(db.Users, uv => uv.UserId, u => u.Id, (uv, u) => new {
            u.Id, u.Email, u.Name, Role = uv.Role.ToString()
        })
        .ToListAsync();

    return Results.Ok(list);
});

app.MapPost("/vehicles/{vehicleId:guid}/users", async (AppDbContext db, Guid vehicleId, Guid userId, VehicleRole role) =>
{
    var vehicleExists = await db.Vehicles.AnyAsync(v => v.Id == vehicleId);
    if (!vehicleExists) return Results.NotFound("Vehicle not found.");

    var userExists = await db.Users.AnyAsync(u => u.Id == userId);
    if (!userExists) return Results.BadRequest("User not found.");

    var already = await db.UserVehicles.AnyAsync(x => x.UserId == userId && x.VehicleId == vehicleId);
    if (already) return Results.Conflict("User already assigned to this vehicle.");

    db.UserVehicles.Add(new UserVehicle { UserId = userId, VehicleId = vehicleId, Role = role });
    await db.SaveChangesAsync();

    return Results.NoContent();
});

app.MapDelete("/vehicles/{vehicleId:guid}/users/{userId:guid}", async (AppDbContext db, Guid vehicleId, Guid userId) =>
{
    var link = await db.UserVehicles.FirstOrDefaultAsync(x => x.UserId == userId && x.VehicleId == vehicleId);
    if (link is null) return Results.NotFound();

    db.UserVehicles.Remove(link);
    await db.SaveChangesAsync();
    return Results.NoContent();
});


await app.SeedAsync();

app.Run();
