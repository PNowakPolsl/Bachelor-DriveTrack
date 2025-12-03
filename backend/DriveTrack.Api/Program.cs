using DriveTrack.Api.Data;
using DriveTrack.Api.Data.Dto;
using DriveTrack.Api.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using BCrypt.Net;
using Microsoft.AspNetCore.Authorization;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http.Json;





var builder = WebApplication.CreateBuilder(args);

var config = builder.Configuration;

// sekcja Jwt z appsettings
var jwtSection = config.GetSection("Jwt");
var jwtKey = jwtSection["Key"] ?? throw new InvalidOperationException("Jwt:Key is missing");
var jwtIssuer = jwtSection["Issuer"];
var jwtAudience = jwtSection["Audience"];

// AUTH + JWT
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };
});

builder.Services.AddAuthorization();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

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

static (DateOnly From, DateOnly To) GetSixMonthsRange(DateOnly? from, DateOnly? to)
{
    var today = DateOnly.FromDateTime(DateTime.UtcNow);
    var end = to ?? today;

    var startMonth = end.AddMonths(-5);
    var start = new DateOnly(startMonth.Year, startMonth.Month, 1);

    if (from is not null && from > start) start = from.Value;

    return (start, end);
}

static Guid? GetCurrentUserId(HttpContext httpContext)
{
    var sub =
        httpContext.User.FindFirstValue(JwtRegisteredClaimNames.Sub)
        ?? httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

    return Guid.TryParse(sub, out var userId)
        ? userId
        : (Guid?)null;
}

static string GetCurrentUserName(HttpContext httpContext)
{
    return httpContext.User.FindFirst("name")?.Value
        ?? httpContext.User.FindFirst(ClaimTypes.Name)?.Value
        ?? "Nieznany użytkownik";
}


static string GenerateJwtToken(AppUser user, IConfiguration config)
{
    var jwtSection = config.GetSection("Jwt");
    var key = jwtSection["Key"] ?? throw new InvalidOperationException("Jwt:Key missing");
    var issuer = jwtSection["Issuer"];
    var audience = jwtSection["Audience"];
    var expiresMinutes = int.TryParse(jwtSection["ExpiresMinutes"], out var mins) ? mins : 60;

    var claims = new[]
    {
        new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
        new Claim(JwtRegisteredClaimNames.Email, user.Email),
        new Claim("name", user.Name)
    };

    var keyBytes = Encoding.UTF8.GetBytes(key);
    var creds = new SigningCredentials(
        new SymmetricSecurityKey(keyBytes),
        SecurityAlgorithms.HmacSha256
    );

    var token = new JwtSecurityToken(
        issuer: issuer,
        audience: audience,
        claims: claims,
        expires: DateTime.UtcNow.AddMinutes(expiresMinutes),
        signingCredentials: creds
    );

    return new JwtSecurityTokenHandler().WriteToken(token);
}

static async Task<IResult?> EnsureVehicleAccess(HttpContext http, AppDbContext db, Guid vehicleId)
{
    var userId = GetCurrentUserId(http);
    if (userId is null)
        return Results.Unauthorized();

    var hasAccess = await db.UserVehicles
        .AnyAsync(uv => uv.UserId == userId.Value && uv.VehicleId == vehicleId);

    if (!hasAccess)
        return Results.Forbid();

    return null;
}



static async Task<IResult?> EnsureVehicleOwner(HttpContext http, AppDbContext db, Guid vehicleId)
{
    var userId = GetCurrentUserId(http);
    if (userId is null)
        return Results.Unauthorized();

    var isOwner = await db.UserVehicles
        .AnyAsync(uv =>
            uv.UserId == userId.Value &&
            uv.VehicleId == vehicleId &&
            uv.Role == VehicleRole.Owner);

    if (!isOwner)
        return Results.Forbid();

    return null;
}





app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", () => "DriveTrack API is running");

app.MapGet("/me", (HttpContext http) =>
{
    var userId = GetCurrentUserId(http);
    if (userId is null)
        return Results.Unauthorized();

    var email = http.User.FindFirst(JwtRegisteredClaimNames.Email)?.Value;
    var name  = http.User.FindFirst("name")?.Value;

    return Results.Ok(new
    {
        userId,
        email,
        name
    });
});


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


// PRZEBIEG
app.MapGet("/vehicles/{vehicleId:guid}/odometer", async (
    HttpContext http,
    AppDbContext db,
    Guid vehicleId) =>
{
    var accessError = await EnsureVehicleAccess(http, db, vehicleId);
    if (accessError is not null) return accessError;

    var baseOdo = await db.Vehicles
        .Where(v => v.Id == vehicleId && v.BaseOdometerKm != null)
        .Select(v => new OdometerCandidate(
            "base",
            v.BaseOdometerKm,
            null
        ))
        .FirstOrDefaultAsync();

    var lastFuel = await db.FuelEntries
        .Where(x => x.VehicleId == vehicleId && x.OdometerKm >= 0)
        .OrderByDescending(x => x.Date)
        .ThenByDescending(x => x.OdometerKm)
        .Select(x => new OdometerCandidate(
            "fuel",
            x.OdometerKm,
            x.Date
        ))
        .FirstOrDefaultAsync();

    var lastExp = await db.Expenses
        .Where(x => x.VehicleId == vehicleId && x.OdometerKm != null)
        .OrderByDescending(x => x.Date)
        .ThenByDescending(x => x.OdometerKm)
        .Select(x => new OdometerCandidate(
            "expense",
            x.OdometerKm,
            x.Date
        ))
        .FirstOrDefaultAsync();

    var candidates = new[] { baseOdo, lastFuel, lastExp }
        .Where(c => c is not null && c.OdometerKm is not null)
        .Cast<OdometerCandidate>()
        .ToList();

    if (!candidates.Any())
        return Results.Ok(new { odometerKm = (int?)null, source = (string?)null, date = (DateOnly?)null });

    var best = candidates
        .OrderByDescending(c => c.OdometerKm)
        .ThenByDescending(c => c.Date)
        .First();

    return Results.Ok(new { odometerKm = best.OdometerKm, source = best.Source, date = best.Date });
});





// Vehicles
app.MapGet("/vehicles", async (HttpContext http, AppDbContext db) =>
{
    var userId = GetCurrentUserId(http);
    if (userId is null)
        return Results.Unauthorized();

    var list = await db.UserVehicles
        .Where(uv => uv.UserId == userId.Value)
        .Join(
            db.Vehicles,
            uv => uv.VehicleId,
            v  => v.Id,
            (uv, v) => new
            {
                v.Id,
                v.Name,
                v.Make,
                v.Model,
                v.Plate,
                v.Year,
                v.Vin
            }
        )
        .OrderBy(v => v.Make)
        .ToListAsync();

    return Results.Ok(list);
})
.RequireAuthorization(); 




app.MapPost("/vehicles", async (AppDbContext db, HttpContext http, Vehicle input) =>
{
    var currentUserId = GetCurrentUserId(http);
    if (currentUserId is null)
        return Results.Unauthorized();

    if (string.IsNullOrWhiteSpace(input.Name))
        return Results.BadRequest("Name is required.");
    if (string.IsNullOrWhiteSpace(input.Make))
        return Results.BadRequest("Make is required.");
    if (string.IsNullOrWhiteSpace(input.Model))
        return Results.BadRequest("Model is required.");

    if (input.BaseOdometerKm is not null && input.BaseOdometerKm < 0)
        return Results.BadRequest("BaseOdometerKm must be >= 0.");

    input.Id = Guid.NewGuid();
    input.CreatedAt = DateTime.UtcNow;

    db.Vehicles.Add(input);

    db.UserVehicles.Add(new UserVehicle
    {
        UserId = currentUserId.Value,
        VehicleId = input.Id,
        Role = VehicleRole.Owner
    });

    await db.SaveChangesAsync();

    var dto = new
    {
        input.Id,
        input.Name,
        input.Make,
        input.Model,
        input.Plate,
        input.Year,
        input.Vin,
        input.BaseOdometerKm
    };

    return Results.Created($"/vehicles/{input.Id}", dto);
})
.RequireAuthorization();





app.MapDelete("/vehicles/{vehicleId:guid}", async (
    HttpContext http,
    AppDbContext db,
    Guid vehicleId) =>
{
    var ownerError = await EnsureVehicleOwner(http, db, vehicleId);
    if (ownerError is not null) return ownerError;

    var vehicle = await db.Vehicles.FirstOrDefaultAsync(v => v.Id == vehicleId);
    if (vehicle is null)
        return Results.NotFound("Vehicle not found.");

    var fuelEntries = db.FuelEntries.Where(x => x.VehicleId == vehicleId);
    var expenses    = db.Expenses.Where(x => x.VehicleId == vehicleId);
    var reminders   = db.Reminders.Where(x => x.VehicleId == vehicleId);
    var vehicleFuelTypes = db.VehicleFuelTypes.Where(x => x.VehicleId == vehicleId);
    var userVehicles     = db.UserVehicles.Where(x => x.VehicleId == vehicleId);

    db.FuelEntries.RemoveRange(fuelEntries);
    db.Expenses.RemoveRange(expenses);
    db.Reminders.RemoveRange(reminders);
    db.VehicleFuelTypes.RemoveRange(vehicleFuelTypes);
    db.UserVehicles.RemoveRange(userVehicles);
    db.Vehicles.Remove(vehicle);

    await db.SaveChangesAsync();
    return Results.NoContent();
});

app.MapPut("/vehicles/{vehicleId:guid}", async (
    HttpContext http,
    AppDbContext db,
    Guid vehicleId,
    Vehicle input) =>
{
    var ownerError = await EnsureVehicleOwner(http, db, vehicleId);
    if (ownerError is not null) return ownerError;

    var v = await db.Vehicles.FirstOrDefaultAsync(x => x.Id == vehicleId);
    if (v is null) return Results.NotFound("Vehicle not found.");

    if (string.IsNullOrWhiteSpace(input.Name))
        return Results.BadRequest("Name is required.");
    if (string.IsNullOrWhiteSpace(input.Make))
        return Results.BadRequest("Make is required.");
    if (string.IsNullOrWhiteSpace(input.Model))
        return Results.BadRequest("Model is required.");

    v.Name  = input.Name.Trim();
    v.Make  = input.Make.Trim();
    v.Model = input.Model.Trim();
    v.Plate = string.IsNullOrWhiteSpace(input.Plate) ? null : input.Plate.Trim();
    v.Year  = input.Year;

    v.Vin = string.IsNullOrWhiteSpace(input.Vin)
        ? null
        : input.Vin.Trim();

    if (input.BaseOdometerKm is not null && input.BaseOdometerKm < 0)
        return Results.BadRequest("BaseOdometerKm must be >= 0.");

    v.BaseOdometerKm = input.BaseOdometerKm;

    await db.SaveChangesAsync();
    return Results.NoContent();
});




// pomocnicza funkcja do obliczania aktualnego przebiegu
static async Task<int?> GetCurrentOdometer(AppDbContext db, Guid vehicleId)
{
    var baseOdo = await db.Vehicles
        .Where(v => v.Id == vehicleId && v.BaseOdometerKm != null)
        .Select(v => new OdometerCandidate(
            "base",
            v.BaseOdometerKm,
            null
        ))
        .FirstOrDefaultAsync();

    var lastFuel = await db.FuelEntries
        .Where(x => x.VehicleId == vehicleId && x.OdometerKm >= 0)
        .OrderByDescending(x => x.Date)
        .ThenByDescending(x => x.OdometerKm)
        .Select(x => new OdometerCandidate(
            "fuel",
            x.OdometerKm,
            x.Date
        ))
        .FirstOrDefaultAsync();

    var lastExp = await db.Expenses
        .Where(x => x.VehicleId == vehicleId && x.OdometerKm != null)
        .OrderByDescending(x => x.Date)
        .ThenByDescending(x => x.OdometerKm)
        .Select(x => new OdometerCandidate(
            "expense",
            x.OdometerKm,
            x.Date
        ))
        .FirstOrDefaultAsync();

    var candidates = new[] { baseOdo, lastFuel, lastExp }
        .Where(c => c is not null && c!.OdometerKm is not null)
        .Cast<OdometerCandidate>()
        .ToList();

    if (!candidates.Any())
        return null;

    var best = candidates
        .OrderByDescending(c => c.OdometerKm)
        .ThenByDescending(c => c.Date)
        .First();

    return best.OdometerKm;
}



// Vehicle <-> FuelTypes

app.MapPost("/vehicles/{vehicleId:guid}/fuel-types", async (
    HttpContext http,
    AppDbContext db,
    Guid vehicleId,
    AssignFuelTypeRequest body) =>
{
    var accessError = await EnsureVehicleAccess(http, db, vehicleId);
    if (accessError is not null) return accessError;

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



app.MapDelete("/vehicles/{vehicleId:guid}/fuel-types/{fuelTypeId:guid}", async (
    HttpContext http,
    AppDbContext db,
    Guid vehicleId,
    Guid fuelTypeId) =>
{
    var accessError = await EnsureVehicleAccess(http, db, vehicleId);
    if (accessError is not null) return accessError;

    var link = await db.VehicleFuelTypes
        .FirstOrDefaultAsync(x => x.VehicleId == vehicleId && x.FuelTypeId == fuelTypeId);
    if (link is null) return Results.NotFound();

    db.VehicleFuelTypes.Remove(link);
    await db.SaveChangesAsync();
    return Results.NoContent();
});




app.MapGet("/vehicles/{vehicleId:guid}", async (HttpContext http, AppDbContext db, Guid vehicleId) =>
{
    var accessError = await EnsureVehicleAccess(http, db, vehicleId);
    if (accessError is not null) return accessError;

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
            v.Vin,
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



app.MapPost("/vehicles/{vehicleId:guid}/fuel-entries", async (
    HttpContext http,
    AppDbContext db,
    Guid vehicleId,
    CreateFuelEntryRequest body) =>
{
    var accessError = await EnsureVehicleAccess(http, db, vehicleId);
    if (accessError is not null) return accessError;

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

    var currentOdo = await GetCurrentOdometer(db, vehicleId);
    if (currentOdo is not null && body.OdometerKm < currentOdo.Value)
    {
        return Results.BadRequest(
            $"OdometerKm must be >= current value ({currentOdo.Value} km). " +
            "Jeśli chcesz obniżyć przebieg, zrób to w edycji pojazdu."
        );
    }

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

    if (fuelCategory is null)
    {
        return Results.Created($"/vehicles/{vehicleId}/fuel-entries/{entry.Id}", new
        {
            entry.Id,
            entry.Date,
            entry.Volume,
            entry.Unit,
            entry.PricePerUnit,
            entry.TotalCost,
            entry.OdometerKm,
            entry.IsFullTank,
            entry.Station
        });
    }

    var createdExpense = new Expense
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
        CreatedAt = DateTime.UtcNow,
        CreatedByName = GetCurrentUserName(http)
    };

    db.Expenses.Add(createdExpense);
    await db.SaveChangesAsync();

    return Results.Created($"/vehicles/{vehicleId}/expenses/{createdExpense.Id}", new
    {
        createdExpense.Id,
        createdExpense.Date,
        createdExpense.Amount,
        createdExpense.Description,
        createdExpense.OdometerKm,
        createdExpense.CreatedByName,
        Category = new { CategoryId = createdExpense.CategoryId, Name = fuelCategory.Name }
    });
});



app.MapGet("/vehicles/{vehicleId:guid}/fuel-entries", async (
    HttpContext http,
    AppDbContext db,
    Guid vehicleId,
    DateOnly? from,
    DateOnly? to) =>
{
    var accessError = await EnsureVehicleAccess(http, db, vehicleId);
    if (accessError is not null) return accessError;

    var q = db.FuelEntries
        .Where(x => x.VehicleId == vehicleId);

    if (from is not null) q = q.Where(x => x.Date >= from);
    if (to   is not null) q = q.Where(x => x.Date <= to);

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

app.MapGet("/reports/monthly-expenses", async (
    HttpContext http,
    AppDbContext db,
    DateOnly? from,
    DateOnly? to
) =>
{
    var userId = GetCurrentUserId(http);
    if (userId is null) return Results.Unauthorized();

    var vehicleIds = await db.UserVehicles
        .Where(uv => uv.UserId == userId.Value)
        .Select(uv => uv.VehicleId)
        .ToListAsync();

    var (start, end) = GetSixMonthsRange(from, to);

    var raw = await db.Expenses
        .Where(e => vehicleIds.Contains(e.VehicleId))
        .Where(e => e.Date >= start && e.Date <= end)
        .Select(e => new { e.Date, e.Amount })
        .ToListAsync();

    var list = raw
        .GroupBy(e => new { e.Date.Year, e.Date.Month })
        .Select(g => new MonthlyExpenseReportItem(
            g.Key.Year,
            g.Key.Month,
            g.Sum(x => x.Amount)
        ))
        .OrderBy(x => x.Year)
        .ThenBy(x => x.Month)
        .ToList();

    return Results.Ok(list);
});



app.MapGet("/reports/expenses-by-category", async (
    HttpContext http,
    AppDbContext db,
    DateOnly? from,
    DateOnly? to
) =>
{
    var userId = GetCurrentUserId(http);
    if (userId is null) return Results.Unauthorized();

    var vehicleIds = await db.UserVehicles
        .Where(uv => uv.UserId == userId.Value)
        .Select(uv => uv.VehicleId)
        .ToListAsync();

    var (start, end) = GetSixMonthsRange(from, to);

    var raw = await db.Expenses
        .Where(e => vehicleIds.Contains(e.VehicleId))
        .Where(e => e.Date >= start && e.Date <= end)
        .Select(e => new
        {
            Amount = e.Amount,
            CategoryName = e.Category != null
                ? e.Category.Name
                : "Brak kategorii"
        })
        .ToListAsync();

    var list = raw
        .GroupBy(x => x.CategoryName)
        .Select(g => new CategoryExpenseReportItem(
            g.Key,
            g.Sum(x => x.Amount)
        ))
        .OrderByDescending(x => x.Total)
        .ToList();

    return Results.Ok(list);
});



app.MapGet("/reports/fuel-consumption", async (
    HttpContext http,
    AppDbContext db,
    DateOnly? from,
    DateOnly? to
) =>
{
    var userId = GetCurrentUserId(http);
    if (userId is null) return Results.Unauthorized();

    var vehicleIds = await db.UserVehicles
        .Where(uv => uv.UserId == userId.Value)
        .Select(uv => uv.VehicleId)
        .ToListAsync();

    var (start, end) = GetSixMonthsRange(from, to);

    var entries = await db.FuelEntries
        .Where(x => vehicleIds.Contains(x.VehicleId))
        .Where(x =>
            x.IsFullTank == true &&
            x.OdometerKm >= 0 &&
            x.Unit != null &&
            x.Unit.ToLower() == "l"
        )
        .OrderBy(x => x.VehicleId)
        .ThenBy(x => x.Date)
        .ThenBy(x => x.OdometerKm)
        .ToListAsync();

    var buckets = new Dictionary<(int Year, int Month), (double Volume, double Distance)>();

    foreach (var vehicleGroup in entries.GroupBy(e => e.VehicleId))
    {
        FuelEntry? prev = null;

        foreach (var current in vehicleGroup)
        {
            if (prev is null)
            {
                prev = current;
                continue;
            }

            var distance = current.OdometerKm - prev.OdometerKm;

            if (distance <= 0)
            {
                prev = current;
                continue;
            }

            var endDate = current.Date;

            if (endDate < start || endDate > end)
            {
                prev = current;
                continue;
            }

            var key = (endDate.Year, endDate.Month);
            if (!buckets.TryGetValue(key, out var agg))
                agg = (0, 0);

            agg.Volume += (double)current.Volume;
            agg.Distance += (double)distance;

            buckets[key] = agg;

            prev = current;
        }
    }

    var result = buckets
        .Select(kv =>
        {
            var (year, month) = kv.Key;
            var (volume, distance) = kv.Value;
            var avg = distance > 0 ? (volume * 100.0) / distance : 0.0;

            return new FuelConsumptionReportItem(
                year,
                month,
                avg
            );
        })
        .OrderBy(x => x.Year)
        .ThenBy(x => x.Month)
        .ToList();

    return Results.Ok(result);
});



// Expenses
app.MapGet("/vehicles/{vehicleId:guid}/expenses", async (
    HttpContext http,
    AppDbContext db,
    Guid vehicleId,
    DateOnly? from,
    DateOnly? to,
    Guid? categoryId) =>
{
    var accessError = await EnsureVehicleAccess(http, db, vehicleId);
    if (accessError is not null) return accessError;

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
            e.CreatedByName,
            Category = new { CategoryId = e.CategoryId, Name = e.Category.Name }
        })
        .ToListAsync();

    return Results.Ok(list);
});



app.MapPost("/vehicles/{vehicleId:guid}/expenses", async (
    HttpContext http,
    AppDbContext db,
    Guid vehicleId,
    CreateExpenseRequest body) =>
{
    var accessError = await EnsureVehicleAccess(http, db, vehicleId);
    if (accessError is not null) return accessError;

    var vehicle = await db.Vehicles.FindAsync(vehicleId);
    if (vehicle is null) return Results.NotFound("Vehicle not found.");

    var category = await db.Categories.FindAsync(body.CategoryId);
    if (category is null) return Results.BadRequest("Category not found.");

    if (body.Amount <= 0) return Results.BadRequest("Amount must be > 0.");

    if (body.OdometerKm is not null)
    {
        var currentOdo = await GetCurrentOdometer(db, vehicleId);
        if (currentOdo is not null && body.OdometerKm < currentOdo.Value)
        {
            return Results.BadRequest(
                $"OdometerKm must be >= current value ({currentOdo.Value} km). " +
                "Jeśli chcesz obniżyć przebieg, zrób to w edycji pojazdu."
            );
        }
    }


    var exp = new Expense
    {
        Id = Guid.NewGuid(),
        VehicleId = vehicleId,
        CategoryId = body.CategoryId,
        Date = body.Date,
        Amount = Math.Round(body.Amount, 2, MidpointRounding.AwayFromZero),
        Description = string.IsNullOrWhiteSpace(body.Description) ? null : body.Description.Trim(),
        OdometerKm = body.OdometerKm,
        CreatedAt = DateTime.UtcNow,
        CreatedByName = GetCurrentUserName(http)
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
            exp.CreatedByName,
            Category = new { Id = exp.CategoryId, Name = category.Name }
        }
    );

});

// Reminders
app.MapGet("/vehicles/{vehicleId:guid}/reminders", async (
    HttpContext http,
    AppDbContext db,
    Guid vehicleId,
    DateOnly? fromDue,
    DateOnly? toDue,
    bool? completed,
    bool? onlyOverdue,
    bool? onlyUpcoming) =>
{
    var accessError = await EnsureVehicleAccess(http, db, vehicleId);
    if (accessError is not null) return accessError;

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
            r.IsCompleted, r.CompletedAt, r.CreatedAt,
            r.CreatedByName
        })
        .ToListAsync();

    return Results.Ok(list);
});



app.MapPost("/vehicles/{vehicleId:guid}/reminders", async (
    HttpContext http,
    AppDbContext db,
    Guid vehicleId,
    CreateReminderRequest body) =>
{
    var accessError = await EnsureVehicleAccess(http, db, vehicleId);
    if (accessError is not null) return accessError;

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
        CreatedAt = DateTime.UtcNow,
        CreatedByName = GetCurrentUserName(http)
    };

    db.Reminders.Add(reminder);
    await db.SaveChangesAsync();

    return Results.Created($"/vehicles/{vehicleId}/reminders/{reminder.Id}", reminder);
});



app.MapPatch("/reminders/{reminderId:guid}", async (
    HttpContext http,
    AppDbContext db,
    Guid reminderId,
    UpdateReminderRequest body) =>
{
    var r = await db.Reminders.FirstOrDefaultAsync(x => x.Id == reminderId);
    if (r is null) return Results.NotFound();

    var accessError = await EnsureVehicleAccess(http, db, r.VehicleId);
    if (accessError is not null) return accessError;

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


app.MapDelete("/reminders/{reminderId:guid}", async (
    HttpContext http,
    AppDbContext db,
    Guid reminderId) =>
{
    var r = await db.Reminders.FirstOrDefaultAsync(x => x.Id == reminderId);
    if (r is null) return Results.NotFound();

    var accessError = await EnsureVehicleAccess(http, db, r.VehicleId);
    if (accessError is not null) return accessError;

    db.Reminders.Remove(r);
    await db.SaveChangesAsync();
    return Results.NoContent();
});


// Users
app.MapPost("/auth/register", async (AppDbContext db, IConfiguration config, RegisterRequest input) =>
{
    if (string.IsNullOrWhiteSpace(input.Email) ||
        string.IsNullOrWhiteSpace(input.Password) ||
        string.IsNullOrWhiteSpace(input.Name))
    {
        return Results.BadRequest("Name, email and password are required.");
    }

    var email = input.Email.Trim().ToLowerInvariant();

    var exists = await db.Users.AnyAsync(u => u.Email == email);
    if (exists)
        return Results.Conflict("Email already in use.");

    var user = new AppUser
    {
        Id = Guid.NewGuid(),
        Email = email,
        Name = input.Name.Trim(),
        PasswordHash = BCrypt.Net.BCrypt.HashPassword(input.Password),
        CreatedAt = DateTime.UtcNow
    };

    db.Users.Add(user);
    await db.SaveChangesAsync();

    var token = GenerateJwtToken(user, config);

    return Results.Ok(new AuthResponse(user.Id, user.Email, user.Name, token));
});

app.MapPost("/auth/login", async (AppDbContext db, IConfiguration config, LoginRequest input) =>
{
    if (string.IsNullOrWhiteSpace(input.Email) || string.IsNullOrWhiteSpace(input.Password))
        return Results.BadRequest("Email and password are required.");

    var email = input.Email.Trim().ToLowerInvariant();
    var user = await db.Users.FirstOrDefaultAsync(u => u.Email == email);

    if (user is null)
        return Results.BadRequest("Invalid email or password.");

    if (string.IsNullOrWhiteSpace(user.PasswordHash))
        return Results.BadRequest("To konto jest powiązane z logowaniem przez Google. Zaloguj się używając przycisku Google.");

    bool ok;
    try
    {
        ok = BCrypt.Net.BCrypt.Verify(input.Password, user.PasswordHash);
    }
    catch
    {
        // na wypadek starych, zepsutych wpisów
        return Results.BadRequest("Invalid email or password.");
    }

    if (!ok)
        return Results.BadRequest("Invalid email or password.");

    var token = GenerateJwtToken(user, config);

    return Results.Ok(new AuthResponse(user.Id, user.Email, user.Name, token));
});

app.MapPost("/auth/change-password",
    async (HttpContext http, AppDbContext db, ChangePasswordRequest input) =>
{
    var userIdString = http.User.FindFirstValue(JwtRegisteredClaimNames.Sub)
                      ?? http.User.FindFirstValue(ClaimTypes.NameIdentifier);

    if (string.IsNullOrWhiteSpace(userIdString) || !Guid.TryParse(userIdString, out var userId))
    {
        return Results.Unauthorized();
    }

    var user = await db.Users.FirstOrDefaultAsync(u => u.Id == userId);
    if (user is null)
    {
        return Results.Unauthorized();
    }

    if (string.IsNullOrWhiteSpace(user.PasswordHash))
    {
        return Results.BadRequest("To konto nie ma ustawionego hasła.");
    }

    var isCurrentOk = BCrypt.Net.BCrypt.Verify(input.CurrentPassword, user.PasswordHash);
    if (!isCurrentOk)
    {
        return Results.BadRequest("Obecne hasło jest nieprawidłowe.");
    }

    if (string.IsNullOrWhiteSpace(input.NewPassword) || input.NewPassword.Length < 6)
    {
        return Results.BadRequest("Nowe hasło musi mieć co najmniej 6 znaków.");
    }

    if (input.NewPassword == input.CurrentPassword)
    {
        return Results.BadRequest("Nowe hasło nie może być takie samo jak obecne.");
    }

    user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(input.NewPassword);
    await db.SaveChangesAsync();

    return Results.NoContent();
})
.RequireAuthorization();



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

app.MapGet("/me/vehicles", async (HttpContext http, AppDbContext db) =>
{
    var userId = GetCurrentUserId(http);
    if (userId is null)
        return Results.Unauthorized();

    var list = await db.UserVehicles
        .Where(uv => uv.UserId == userId.Value)
        .Join(db.Vehicles, uv => uv.VehicleId, v => v.Id, (uv, v) => new {
            v.Id,
            v.Name,
            v.Make,
            v.Model,
            v.Plate,
            v.Year,
            Role = uv.Role.ToString()
        })
        .ToListAsync();

    return Results.Ok(list);
});


app.MapGet("/vehicles/{vehicleId:guid}/users", async (
    HttpContext http,
    AppDbContext db,
    Guid vehicleId) =>
{
    var accessError = await EnsureVehicleAccess(http, db, vehicleId);
    if (accessError is not null) return accessError;

    var list = await db.UserVehicles
        .Where(uv => uv.VehicleId == vehicleId)
        .Join(db.Users, uv => uv.UserId, u => u.Id, (uv, u) => new
        {
            Id = u.Id,
            u.Email,
            u.Name,
            Role = uv.Role.ToString()
        })
        .ToListAsync();

    return Results.Ok(list);
})
.RequireAuthorization();




app.MapPost("/vehicles/{vehicleId:guid}/users", async (
    HttpContext http,
    AppDbContext db,
    Guid vehicleId,
    AddVehicleUserRequest body) =>
{
    var ownerError = await EnsureVehicleOwner(http, db, vehicleId);
    if (ownerError is not null) return ownerError;

    var vehicleExists = await db.Vehicles.AnyAsync(v => v.Id == vehicleId);
    if (!vehicleExists) return Results.NotFound("Vehicle not found.");

    if (string.IsNullOrWhiteSpace(body.Email))
        return Results.BadRequest("Email is required.");

    var email = body.Email.Trim().ToLowerInvariant();

    var user = await db.Users.FirstOrDefaultAsync(u => u.Email == email);
    if (user is null)
        return Results.BadRequest("User with this email not found.");

    var already = await db.UserVehicles
        .AnyAsync(x => x.UserId == user.Id && x.VehicleId == vehicleId);
    if (already)
        return Results.Conflict("User already assigned to this vehicle.");

    db.UserVehicles.Add(new UserVehicle
    {
        UserId = user.Id,
        VehicleId = vehicleId,
        Role = body.Role
    });

    await db.SaveChangesAsync();
    return Results.NoContent();
})
.RequireAuthorization();


app.MapDelete("/vehicles/{vehicleId:guid}/users/{userId:guid}", async (
    HttpContext http,
    AppDbContext db,
    Guid vehicleId,
    Guid userId) =>
{
    var ownerError = await EnsureVehicleOwner(http, db, vehicleId);
    if (ownerError is not null) return ownerError;

    var link = await db.UserVehicles
        .FirstOrDefaultAsync(x => x.UserId == userId && x.VehicleId == vehicleId);

    if (link is null) return Results.NotFound();

    db.UserVehicles.Remove(link);
    await db.SaveChangesAsync();
    return Results.NoContent();
})
.RequireAuthorization();

app.MapGet("/dashboard/upcoming-reminders", async (
    HttpContext http,
    AppDbContext db
) =>
{
    var userId = GetCurrentUserId(http);
    if (userId is null)
        return Results.Unauthorized();

    var today = DateOnly.FromDateTime(DateTime.UtcNow);

    // pojazdy, do których użytkownik ma dostęp
    var vehicleIds = await db.UserVehicles
        .Where(uv => uv.UserId == userId.Value)
        .Select(uv => uv.VehicleId)
        .Distinct()
        .ToListAsync();

    if (!vehicleIds.Any())
        return Results.Ok(Array.Empty<object>());

    // bierzemy tylko przyszłe, nieukończone przypomnienia
    var raw = await db.Reminders
        .Where(r =>
            vehicleIds.Contains(r.VehicleId) &&
            !r.IsCompleted &&
            r.DueDate >= today
        )
        .OrderBy(r => r.DueDate)
        .Take(10)   // weźmy trochę więcej, potem i tak obetniemy do 3
        .Select(r => new
        {
            r.Id,
            r.Title,
            r.DueDate,
            r.VehicleId
        })
        .ToListAsync();

    if (!raw.Any())
        return Results.Ok(Array.Empty<object>());

    // słownik pojazdów, żeby mieć nazwy
    var vehicleNames = await db.Vehicles
        .Where(v => vehicleIds.Contains(v.Id))
        .Select(v => new { v.Id, v.Name })
        .ToDictionaryAsync(v => v.Id, v => v.Name);

    var todayDateTime = DateTime.UtcNow.Date;

    var result = raw
        .Select(r =>
        {
            var dueDateTime = r.DueDate.ToDateTime(TimeOnly.MinValue);
            var daysLeft = (int)(dueDateTime.Date - todayDateTime).TotalDays;

            vehicleNames.TryGetValue(r.VehicleId, out var vehicleName);

            return new
            {
                r.Id,
                r.Title,
                r.DueDate,
                VehicleName = vehicleName ?? "(bez nazwy)",
                DaysLeft = daysLeft
            };
        })
        .OrderBy(x => x.DaysLeft)
        .ThenBy(x => x.DueDate)
        .Take(3) // tu faktycznie ograniczamy do 3
        .ToList();

    return Results.Ok(result);
})
.RequireAuthorization();

app.MapGet("/vehicles/{vehicleId:guid}/stations", async (
    HttpContext http,
    AppDbContext db,
    Guid vehicleId
) =>
{
    var accessError = await EnsureVehicleAccess(http, db, vehicleId);
    if (accessError is not null) return accessError;

    var list = await db.FuelEntries
        .Where(x => x.VehicleId == vehicleId &&
                    x.Station != null &&
                    x.Station != "")
        .GroupBy(x => x.Station!)
        .Select(g => new
        {
            Name = g.Key!,
            LastDate = g.Max(x => x.Date) // żeby ostatnio użyte były wyżej
        })
        .OrderByDescending(x => x.LastDate)
        .Select(x => x.Name)
        .Take(15)  // np. max 15 stacji
        .ToListAsync();

    return Results.Ok(list);
});





await app.SeedAsync();

app.Run();
