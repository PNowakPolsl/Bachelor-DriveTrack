using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;

namespace DriveTrack.Api.Tests;

public class ReportsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ReportsTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }


    private record AuthResponseDto(
        Guid UserId,
        string Email,
        string Name,
        string Token
    );

    private record CreatedVehicleDto(
        Guid Id,
        string Name,
        string Make,
        string Model,
        string? Plate,
        int? Year,
        string? Vin,
        int? BaseOdometerKm
    );

    private record CategoryDtoSimple(
        Guid Id,
        string Name
    );

    private record FuelTypeDto(
        Guid Id,
        string Name,
        string DefaultUnit
    );

    private record MonthlyExpenseReportItem(
        int Year,
        int Month,
        decimal TotalAmount
    );

    private record CategoryExpenseReportItem(
        string CategoryName,
        decimal Total
    );

    private record FuelConsumptionReportItem(
        int Year,
        int Month,
        double AverageConsumption
    );

    private record ElectricConsumptionReportItem(
        int Year,
        int Month,
        double AverageConsumption
    );

    private record VehicleExpensesReportItem(
        Guid VehicleId,
        string VehicleName,
        decimal TotalAmount
    );

    private record VehicleCostPer100KmReportItem(
        Guid VehicleId,
        string VehicleName,
        double CostPer100Km
    );

    // --- HELPERY ---

    private async Task<string> RegisterAndLoginAsync(string prefix = "rep")
    {
        var email = $"{prefix}_{Guid.NewGuid():N}@example.com";
        const string password = "Test123!";

        var registerBody = new
        {
            name = "Reports Tester",
            email,
            password
        };

        var r1 = await _client.PostAsJsonAsync("/auth/register", registerBody);
        r1.StatusCode.Should().Be(HttpStatusCode.OK);

        var loginBody = new
        {
            email,
            password
        };

        var r2 = await _client.PostAsJsonAsync("/auth/login", loginBody);
        r2.StatusCode.Should().Be(HttpStatusCode.OK);

        var auth = await r2.Content.ReadFromJsonAsync<AuthResponseDto>();
        auth.Should().NotBeNull();

        return auth!.Token;
    }

    private async Task<CreatedVehicleDto> CreateVehicleAsync(string token, string name)
    {
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var body = new
        {
            name,
            make = "TestMake",
            model = "TestModel",
            plate = "WX REP",
            year = 2020,
            vin = "REPVIN00000000001",
            baseOdometerKm = 0
        };

        var resp = await _client.PostAsJsonAsync("/vehicles", body);
        resp.StatusCode.Should().Be(HttpStatusCode.Created);

        var created = await resp.Content.ReadFromJsonAsync<CreatedVehicleDto>();
        created.Should().NotBeNull();
        return created!;
    }

    private async Task<CategoryDtoSimple> CreateCategoryAsync(string name)
{
    var body = new
    {
        name,
        ownerUserId = (Guid?)null
    };

    var resp = await _client.PostAsJsonAsync("/categories", body);

    if (resp.StatusCode == HttpStatusCode.Conflict)
    {
        var listResp = await _client.GetAsync("/categories");
        listResp.StatusCode.Should().Be(HttpStatusCode.OK);

        var list = await listResp.Content.ReadFromJsonAsync<List<CategoryDtoSimple>>();
        list.Should().NotBeNull();

        var existing = list!
            .First(c => c.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

        return existing;
    }

    resp.StatusCode.Should().Be(HttpStatusCode.Created);

    var cat = await resp.Content.ReadFromJsonAsync<CategoryDtoSimple>();
    cat.Should().NotBeNull();
    return cat!;
}


    private async Task<FuelTypeDto> CreateFuelTypeAsync(string name, string defaultUnit)
    {
        var body = new
        {
            name,
            defaultUnit
        };

        var resp = await _client.PostAsJsonAsync("/fuel-types", body);
        resp.StatusCode.Should().Be(HttpStatusCode.Created);

        var ft = await resp.Content.ReadFromJsonAsync<FuelTypeDto>();
        ft.Should().NotBeNull();
        return ft!;
    }

    private async Task AssignFuelTypeToVehicleAsync(Guid vehicleId, Guid fuelTypeId, string token)
    {
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var body = new
        {
            fuelTypeId
        };

        var resp = await _client.PostAsJsonAsync($"/vehicles/{vehicleId}/fuel-types", body);
        resp.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    // --- TESTY ---

    [Fact]
    public async Task MonthlyExpenses_Should_Aggregate_Per_Month()
    {
        var token = await RegisterAndLoginAsync("rep_month");
        var vehicle = await CreateVehicleAsync(token, "Raportowane auto");
        var category = await CreateCategoryAsync("Serwis");

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var today = DateOnly.FromDateTime(DateTime.UtcNow.Date);
        var prevMonth = today.AddMonths(-1);

        var d1 = new DateOnly(prevMonth.Year, prevMonth.Month, 10);
        var d2 = new DateOnly(today.Year, today.Month, 5);

        // wydatek w poprzednim miesiącu
        var body1 = new
        {
            categoryId = category.Id,
            date = d1,
            amount = 100m,
            description = "Serwis poprzedni miesiąc",
            odometerKm = 10
        };
        var r1 = await _client.PostAsJsonAsync($"/vehicles/{vehicle.Id}/expenses", body1);
        r1.StatusCode.Should().Be(HttpStatusCode.Created);

        // 2 wydatki w bieżącym miesiącu
        var body2 = new
        {
            categoryId = category.Id,
            date = d2,
            amount = 200m,
            description = "Serwis bieżący 1",
            odometerKm = 20
        };
        var body3 = new
        {
            categoryId = category.Id,
            date = d2.AddDays(1),
            amount = 50m,
            description = "Serwis bieżący 2",
            odometerKm = 30
        };

        var r2 = await _client.PostAsJsonAsync($"/vehicles/{vehicle.Id}/expenses", body2);
        var r3 = await _client.PostAsJsonAsync($"/vehicles/{vehicle.Id}/expenses", body3);
        r2.StatusCode.Should().Be(HttpStatusCode.Created);
        r3.StatusCode.Should().Be(HttpStatusCode.Created);

        var from = new DateOnly(prevMonth.Year, prevMonth.Month, 1);
        var to = today.AddDays(1);

        var url = $"/reports/monthly-expenses?from={from:yyyy-MM-dd}&to={to:yyyy-MM-dd}";
        var resp = await _client.GetAsync(url);
        resp.StatusCode.Should().Be(HttpStatusCode.OK);

        var list = await resp.Content.ReadFromJsonAsync<List<MonthlyExpenseReportItem>>();
        list.Should().NotBeNull();

        var items = list!;
        items.Should().Contain(i => i.Year == prevMonth.Year && i.Month == prevMonth.Month && i.TotalAmount == 100m);
        items.Should().Contain(i => i.Year == today.Year && i.Month == today.Month && i.TotalAmount == 250m);
    }

    [Fact]
    public async Task ExpensesByCategory_Should_Group_Correctly()
    {
        var token = await RegisterAndLoginAsync("rep_cat");
        var vehicle = await CreateVehicleAsync(token, "Raport kategorie");
        var catFuel = await CreateCategoryAsync("Paliwo");
        var catService = await CreateCategoryAsync("Serwis");

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var today = DateOnly.FromDateTime(DateTime.UtcNow.Date);
        var d = today.AddDays(-1);

        var body1 = new
        {
            categoryId = catFuel.Id,
            date = d,
            amount = 120m,
            description = "Tankowanie",
            odometerKm = 100
        };
        var body2 = new
        {
            categoryId = catFuel.Id,
            date = d,
            amount = 80m,
            description = "Tankowanie 2",
            odometerKm = 200
        };
        var body3 = new
        {
            categoryId = catService.Id,
            date = d,
            amount = 300m,
            description = "Serwis",
            odometerKm = 300
        };

        (await _client.PostAsJsonAsync($"/vehicles/{vehicle.Id}/expenses", body1)).StatusCode.Should().Be(HttpStatusCode.Created);
        (await _client.PostAsJsonAsync($"/vehicles/{vehicle.Id}/expenses", body2)).StatusCode.Should().Be(HttpStatusCode.Created);
        (await _client.PostAsJsonAsync($"/vehicles/{vehicle.Id}/expenses", body3)).StatusCode.Should().Be(HttpStatusCode.Created);

        var from = d.AddDays(-1);
        var to = d.AddDays(1);

        var url = $"/reports/expenses-by-category?from={from:yyyy-MM-dd}&to={to:yyyy-MM-dd}";
        var resp = await _client.GetAsync(url);
        resp.StatusCode.Should().Be(HttpStatusCode.OK);

        var list = await resp.Content.ReadFromJsonAsync<List<CategoryExpenseReportItem>>();
        list.Should().NotBeNull();

        var items = list!;
        items.Should().Contain(i => i.CategoryName.ToLower() == "paliwo" && i.Total == 200m);
        items.Should().Contain(i => i.CategoryName.ToLower() == "serwis" && i.Total == 300m);
    }

    [Fact]
    public async Task FuelConsumption_Should_Calculate_From_FullTanks()
    {
        var token = await RegisterAndLoginAsync("rep_fuel");
        var vehicle = await CreateVehicleAsync(token, "Raport spalanie");
        var fuelType = await CreateFuelTypeAsync("PB95", "L");
        await AssignFuelTypeToVehicleAsync(vehicle.Id, fuelType.Id, token);

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var today = DateOnly.FromDateTime(DateTime.UtcNow.Date);
        var baseDate = today.AddDays(-1);

        var first = new
        {
            fuelTypeId = fuelType.Id,
            date = baseDate.AddDays(-2),
            volume = 10m,
            unit = "L",
            pricePerUnit = 6.0m,
            odometerKm = 1000,
            isFullTank = true,
            station = "Stacja 1"
        };
        var second = new
        {
            fuelTypeId = fuelType.Id,
            date = baseDate,
            volume = 40m, // zużycie między 1000 i 1500
            unit = "L",
            pricePerUnit = 6.0m,
            odometerKm = 1500,
            isFullTank = true,
            station = "Stacja 2"
        };

        (await _client.PostAsJsonAsync($"/vehicles/{vehicle.Id}/fuel-entries", first)).StatusCode.Should().Be(HttpStatusCode.Created);
        (await _client.PostAsJsonAsync($"/vehicles/{vehicle.Id}/fuel-entries", second)).StatusCode.Should().Be(HttpStatusCode.Created);

        var resp = await _client.GetAsync("/reports/fuel-consumption");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);

        var list = await resp.Content.ReadFromJsonAsync<List<FuelConsumptionReportItem>>();
        list.Should().NotBeNull();

        var item = list!.Single(i => i.Year == baseDate.Year && i.Month == baseDate.Month);
        var expected = 40.0 * 100.0 / 500.0;

        item.AverageConsumption.Should().BeApproximately(expected, 0.001);
    }

    [Fact]
    public async Task EvConsumption_Should_Calculate_From_KWh()
    {
        var token = await RegisterAndLoginAsync("rep_ev");
        var vehicle = await CreateVehicleAsync(token, "Raport EV");
        var fuelType = await CreateFuelTypeAsync("Elektryczność", "kWh");
        await AssignFuelTypeToVehicleAsync(vehicle.Id, fuelType.Id, token);

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var today = DateOnly.FromDateTime(DateTime.UtcNow.Date);
        var baseDate = today.AddDays(-1);

        var first = new
        {
            fuelTypeId = fuelType.Id,
            date = baseDate.AddDays(-2),
            volume = 5m,
            unit = "kWh",
            pricePerUnit = 1.0m,
            odometerKm = 10_000,
            isFullTank = true,
            station = "Ładowarka 1"
        };
        var second = new
        {
            fuelTypeId = fuelType.Id,
            date = baseDate,
            volume = 30m,
            unit = "kWh",
            pricePerUnit = 1.0m,
            odometerKm = 10_200,
            isFullTank = true,
            station = "Ładowarka 2"
        };

        (await _client.PostAsJsonAsync($"/vehicles/{vehicle.Id}/fuel-entries", first)).StatusCode.Should().Be(HttpStatusCode.Created);
        (await _client.PostAsJsonAsync($"/vehicles/{vehicle.Id}/fuel-entries", second)).StatusCode.Should().Be(HttpStatusCode.Created);

        var resp = await _client.GetAsync("/reports/ev-consumption");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);

        var list = await resp.Content.ReadFromJsonAsync<List<ElectricConsumptionReportItem>>();
        list.Should().NotBeNull();

        var item = list!.Single(i => i.Year == baseDate.Year && i.Month == baseDate.Month);
        var expected = 30.0 * 100.0 / 200.0;

        item.AverageConsumption.Should().BeApproximately(expected, 0.001);
    }

    [Fact]
    public async Task VehicleExpenses_Should_Sum_Per_Vehicle()
    {
        var token = await RegisterAndLoginAsync("rep_veh_exp");
        var v1 = await CreateVehicleAsync(token, "Auto 1");
        var v2 = await CreateVehicleAsync(token, "Auto 2");
        var category = await CreateCategoryAsync("Inne");

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var today = DateOnly.FromDateTime(DateTime.UtcNow.Date);
        var d = today.AddDays(-1);

        var body1 = new { categoryId = category.Id, date = d, amount = 100m, description = "v1-1", odometerKm = 10 };
        var body2 = new { categoryId = category.Id, date = d, amount = 50m, description = "v1-2", odometerKm = 20 };

        var body3 = new { categoryId = category.Id, date = d, amount = 200m, description = "v2-1", odometerKm = 30 };

        (await _client.PostAsJsonAsync($"/vehicles/{v1.Id}/expenses", body1)).StatusCode.Should().Be(HttpStatusCode.Created);
        (await _client.PostAsJsonAsync($"/vehicles/{v1.Id}/expenses", body2)).StatusCode.Should().Be(HttpStatusCode.Created);
        (await _client.PostAsJsonAsync($"/vehicles/{v2.Id}/expenses", body3)).StatusCode.Should().Be(HttpStatusCode.Created);

        var from = d.AddDays(-1);
        var to = d.AddDays(1);

        var url = $"/reports/vehicle-expenses?from={from:yyyy-MM-dd}&to={to:yyyy-MM-dd}";
        var resp = await _client.GetAsync(url);
        resp.StatusCode.Should().Be(HttpStatusCode.OK);

        var list = await resp.Content.ReadFromJsonAsync<List<VehicleExpensesReportItem>>();
        list.Should().NotBeNull();

        var items = list!;
        items.Should().Contain(i => i.VehicleId == v1.Id && i.TotalAmount == 150m);
        items.Should().Contain(i => i.VehicleId == v2.Id && i.TotalAmount == 200m);
    }

    [Fact]
    public async Task VehicleCostPer100Km_Should_Use_Expenses_And_Distance()
    {
        var token = await RegisterAndLoginAsync("rep_cost");
        var vehicle = await CreateVehicleAsync(token, "Auto koszt/100km");
        var category = await CreateCategoryAsync("Koszty");
        var fuelType = await CreateFuelTypeAsync("PB95", "L");
        await AssignFuelTypeToVehicleAsync(vehicle.Id, fuelType.Id, token);

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var today = DateOnly.FromDateTime(DateTime.UtcNow.Date);
        var baseDate = today.AddDays(-1);

        var fuel1 = new
        {
            fuelTypeId = fuelType.Id,
            date = baseDate.AddDays(-1),
            volume = 5m,
            unit = "L",
            pricePerUnit = 6m,
            odometerKm = 0,
            isFullTank = true,
            station = "Stacja 1"
        };
        var fuel2 = new
        {
            fuelTypeId = fuelType.Id,
            date = baseDate,
            volume = 5m,
            unit = "L",
            pricePerUnit = 6m,
            odometerKm = 100,
            isFullTank = true,
            station = "Stacja 2"
        };

        (await _client.PostAsJsonAsync($"/vehicles/{vehicle.Id}/fuel-entries", fuel1)).StatusCode.Should().Be(HttpStatusCode.Created);
        (await _client.PostAsJsonAsync($"/vehicles/{vehicle.Id}/fuel-entries", fuel2)).StatusCode.Should().Be(HttpStatusCode.Created);

        var expBody = new
        {
            categoryId = category.Id,
            date = baseDate,
            amount = 50m,
            description = "Koszt ogólny",
            odometerKm = 100
        };
        (await _client.PostAsJsonAsync($"/vehicles/{vehicle.Id}/expenses", expBody)).StatusCode.Should().Be(HttpStatusCode.Created);

        var from = baseDate.AddDays(-2);
        var to = baseDate.AddDays(1);

        var url = $"/reports/vehicle-cost-per-100km?from={from:yyyy-MM-dd}&to={to:yyyy-MM-dd}";
        var resp = await _client.GetAsync(url);
        resp.StatusCode.Should().Be(HttpStatusCode.OK);

        var list = await resp.Content.ReadFromJsonAsync<List<VehicleCostPer100KmReportItem>>();
        list.Should().NotBeNull();

        var item = list!.Single(i => i.VehicleId == vehicle.Id);

        var expected = 110.0;
        item.CostPer100Km.Should().BeApproximately(expected, 0.001);
    }

    [Fact]
    public async Task Reports_WithoutToken_Should_Return_401()
    {
        var resp = await _client.GetAsync("/reports/monthly-expenses");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
