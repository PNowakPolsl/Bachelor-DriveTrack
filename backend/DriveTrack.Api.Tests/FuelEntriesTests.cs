using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;

namespace DriveTrack.Api.Tests;

public class FuelEntriesTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public FuelEntriesTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }


    private record AuthResponseDto(
        Guid UserId,
        string Email,
        string Name,
        string Token
    );

    private record FuelTypeDto(
        Guid Id,
        string Name,
        string DefaultUnit
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

    private record FuelEntryListItemDto(
        Guid Id,
        DateOnly Date,
        decimal Volume,
        string Unit,
        decimal PricePerUnit,
        decimal TotalCost,
        int OdometerKm,
        bool IsFullTank,
        string? Station
    );

    private record CategoryRefDto(
        Guid CategoryId,
        string Name
    );

    private record ExpenseFromFuelDto(
        Guid Id,
        DateOnly Date,
        decimal Amount,
        string? Description,
        int? OdometerKm,
        string CreatedByName,
        CategoryRefDto Category
    );

    // --- HELPERY ---

    private async Task<string> RegisterAndLoginAsync(string prefix = "fuel")
    {
        var email = $"{prefix}_{Guid.NewGuid():N}@example.com";
        const string password = "Test123!";

        var registerBody = new
        {
            name = "Fuel Tester",
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

    private async Task<CreatedVehicleDto> CreateVehicleAsync(string token, string name = "Fuel Test Car")
    {
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var body = new
        {
            name,
            make = "TestMake",
            model = "TestModel",
            plate = "WX FUEL",
            year = 2018,
            vin = "FUELVIN00000000001",
            baseOdometerKm = 100000
        };

        var resp = await _client.PostAsJsonAsync("/vehicles", body);
        resp.StatusCode.Should().Be(HttpStatusCode.Created);

        var created = await resp.Content.ReadFromJsonAsync<CreatedVehicleDto>();
        created.Should().NotBeNull();
        return created!;
    }

    private async Task<FuelTypeDto> CreateFuelTypeAsync(string name = "PB95", string defaultUnit = "L")
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
    public async Task CreateFuelEntry_Then_GetFuelEntries_Should_Return_It()
    {
        var token = await RegisterAndLoginAsync();
        var vehicle = await CreateVehicleAsync(token, "Auto do tankowania");
        var fuelType = await CreateFuelTypeAsync("PB95", "L");
        await AssignFuelTypeToVehicleAsync(vehicle.Id, fuelType.Id, token);

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var today = DateOnly.FromDateTime(DateTime.UtcNow.Date);
        var volume = 40.0m;
        var pricePerUnit = 6.50m;
        var expectedTotal = Math.Round(volume * pricePerUnit, 2, MidpointRounding.AwayFromZero);

        var body = new
        {
            fuelTypeId = fuelType.Id,
            date = today,
            volume,
            unit = "L",
            pricePerUnit,
            odometerKm = 100_500,
            isFullTank = true,
            station = "Orlen Test"
        };

        var postResp = await _client.PostAsJsonAsync($"/vehicles/{vehicle.Id}/fuel-entries", body);
        postResp.StatusCode.Should().Be(HttpStatusCode.Created);

        var createdExpense = await postResp.Content.ReadFromJsonAsync<ExpenseFromFuelDto>();
        createdExpense.Should().NotBeNull();

        createdExpense!.Amount.Should().Be(expectedTotal);
        createdExpense.Date.Should().Be(today);
        createdExpense.OdometerKm.Should().Be(100_500);
        createdExpense.Category.Name.ToLower().Should().Be("paliwo");
        createdExpense.CreatedByName.Should().NotBeNullOrWhiteSpace();

        var getResp = await _client.GetAsync($"/vehicles/{vehicle.Id}/fuel-entries");
        getResp.StatusCode.Should().Be(HttpStatusCode.OK);

        var list = await getResp.Content.ReadFromJsonAsync<List<FuelEntryListItemDto>>();
        list.Should().NotBeNull();

        list!
            .Should()
            .Contain(e =>
                e.TotalCost == expectedTotal &&
                e.Volume == volume &&
                e.Station == "Orlen Test");
    }

    [Fact]
    public async Task CreateFuelEntry_WithoutToken_Should_Return_401()
    {
        var token = await RegisterAndLoginAsync();
        var vehicle = await CreateVehicleAsync(token, "Auto bez tokena");
        var fuelType = await CreateFuelTypeAsync("PB98", "L");
        await AssignFuelTypeToVehicleAsync(vehicle.Id, fuelType.Id, token);

        _client.DefaultRequestHeaders.Authorization = null;

        var today = DateOnly.FromDateTime(DateTime.UtcNow.Date);

        var body = new
        {
            fuelTypeId = fuelType.Id,
            date = today,
            volume = 30m,
            unit = "L",
            pricePerUnit = 6.7m,
            odometerKm = 100_200,
            isFullTank = true,
            station = "Shell"
        };

        var resp = await _client.PostAsJsonAsync($"/vehicles/{vehicle.Id}/fuel-entries", body);

        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateFuelEntry_For_ForeignVehicle_Should_Return_403()
    {
        var token1 = await RegisterAndLoginAsync("fu1");
        var vehicle1 = await CreateVehicleAsync(token1, "Auto user1");
        var fuelType = await CreateFuelTypeAsync("ON", "L");
        await AssignFuelTypeToVehicleAsync(vehicle1.Id, fuelType.Id, token1);

        var token2 = await RegisterAndLoginAsync("fu2");

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token2);

        var today = DateOnly.FromDateTime(DateTime.UtcNow.Date);

        var body = new
        {
            fuelTypeId = fuelType.Id,
            date = today,
            volume = 20m,
            unit = "L",
            pricePerUnit = 7.1m,
            odometerKm = 100_300,
            isFullTank = true,
            station = "Circle K"
        };

        var resp = await _client.PostAsJsonAsync($"/vehicles/{vehicle1.Id}/fuel-entries", body);

        resp.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CreateFuelEntry_With_NonPositiveVolume_Should_Return_400()
    {
        var token = await RegisterAndLoginAsync();
        var vehicle = await CreateVehicleAsync(token, "Auto z błędnym tankowaniem");
        var fuelType = await CreateFuelTypeAsync("PB95", "L");
        await AssignFuelTypeToVehicleAsync(vehicle.Id, fuelType.Id, token);

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var today = DateOnly.FromDateTime(DateTime.UtcNow.Date);

        var body = new
        {
            fuelTypeId = fuelType.Id,
            date = today,
            volume = 0m,
            unit = "L",
            pricePerUnit = 6.5m,
            odometerKm = 100_100,
            isFullTank = true,
            station = "Orlen"
        };

        var resp = await _client.PostAsJsonAsync($"/vehicles/{vehicle.Id}/fuel-entries", body);

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var text = await resp.Content.ReadAsStringAsync();
        text.Should().Contain("Volume must be > 0");
    }

    [Fact]
    public async Task CreateFuelEntry_With_OdometerLowerThanCurrent_Should_Return_400()
    {
        var token = await RegisterAndLoginAsync();
        var vehicle = await CreateVehicleAsync(token, "Auto z przebiegiem");
        var fuelType = await CreateFuelTypeAsync("PB95", "L");
        await AssignFuelTypeToVehicleAsync(vehicle.Id, fuelType.Id, token);

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var today = DateOnly.FromDateTime(DateTime.UtcNow.Date);

        var body1 = new
        {
            fuelTypeId = fuelType.Id,
            date = today,
            volume = 35m,
            unit = "L",
            pricePerUnit = 6.6m,
            odometerKm = 101_000,
            isFullTank = true,
            station = "Stacja 1"
        };

        var resp1 = await _client.PostAsJsonAsync($"/vehicles/{vehicle.Id}/fuel-entries", body1);
        resp1.StatusCode.Should().Be(HttpStatusCode.Created);

        var body2 = new
        {
            fuelTypeId = fuelType.Id,
            date = today,
            volume = 20m,
            unit = "L",
            pricePerUnit = 6.7m,
            odometerKm = 100_500,
            isFullTank = true,
            station = "Stacja 2"
        };

        var resp2 = await _client.PostAsJsonAsync($"/vehicles/{vehicle.Id}/fuel-entries", body2);
        resp2.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var text = await resp2.Content.ReadAsStringAsync();
        text.Should().Contain("OdometerKm must be >= current value");
    }

    [Fact]
    public async Task GetFuelEntries_FilteredByDate_Should_Return_Only_In_Range()
    {
        var token = await RegisterAndLoginAsync();
        var vehicle = await CreateVehicleAsync(token, "Auto filtr dat");
        var fuelType = await CreateFuelTypeAsync("PB95", "L");
        await AssignFuelTypeToVehicleAsync(vehicle.Id, fuelType.Id, token);

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var today = DateOnly.FromDateTime(DateTime.UtcNow.Date);
        var older = today.AddDays(-10);
        var newer = today.AddDays(-2);

        var bodyOld = new
        {
            fuelTypeId = fuelType.Id,
            date = older,
            volume = 30m,
            unit = "L",
            pricePerUnit = 6.4m,
            odometerKm = 100_000,
            isFullTank = true,
            station = "Stara stacja"
        };
        var respOld = await _client.PostAsJsonAsync($"/vehicles/{vehicle.Id}/fuel-entries", bodyOld);
        respOld.StatusCode.Should().Be(HttpStatusCode.Created);

        var bodyNew = new
        {
            fuelTypeId = fuelType.Id,
            date = newer,
            volume = 40m,
            unit = "L",
            pricePerUnit = 6.5m,
            odometerKm = 100_400,
            isFullTank = true,
            station = "Nowa stacja"
        };
        var respNew = await _client.PostAsJsonAsync($"/vehicles/{vehicle.Id}/fuel-entries", bodyNew);
        respNew.StatusCode.Should().Be(HttpStatusCode.Created);

        var from = today.AddDays(-5);
        var to = today;

        var getResp = await _client.GetAsync($"/vehicles/{vehicle.Id}/fuel-entries?from={from:yyyy-MM-dd}&to={to:yyyy-MM-dd}");
        getResp.StatusCode.Should().Be(HttpStatusCode.OK);

        var list = await getResp.Content.ReadFromJsonAsync<List<FuelEntryListItemDto>>();
        list.Should().NotBeNull();

        list!.Should().ContainSingle();
        list[0].Date.Should().Be(newer);
        list[0].Station.Should().Be("Nowa stacja");
    }

    [Fact]
    public async Task CreateFuelEntry_When_GlobalPaliwoCategoryExists_Should_Create_Expense()
    {
        var token = await RegisterAndLoginAsync();
        var vehicle = await CreateVehicleAsync(token, "Auto z kategorią Paliwo");
        var fuelType = await CreateFuelTypeAsync("PB95", "L");
        await AssignFuelTypeToVehicleAsync(vehicle.Id, fuelType.Id, token);

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var today = DateOnly.FromDateTime(DateTime.UtcNow.Date);
        var volume = 45m;
        var pricePerUnit = 6.6m;
        var expectedTotal = Math.Round(volume * pricePerUnit, 2, MidpointRounding.AwayFromZero);

        var body = new
        {
            fuelTypeId = fuelType.Id,
            date = today,
            volume,
            unit = "L",
            pricePerUnit,
            odometerKm = 100_700,
            isFullTank = true,
            station = "Demo Ładowarka / Stacja"
        };

        var resp = await _client.PostAsJsonAsync($"/vehicles/{vehicle.Id}/fuel-entries", body);
        resp.StatusCode.Should().Be(HttpStatusCode.Created);

        var created = await resp.Content.ReadFromJsonAsync<ExpenseFromFuelDto>();
        created.Should().NotBeNull();

        created!.Amount.Should().Be(expectedTotal);
        created.Date.Should().Be(today);
        created.OdometerKm.Should().Be(100_700);
        created.Category.Name.ToLower().Should().Be("paliwo");
        created.CreatedByName.Should().NotBeNullOrWhiteSpace();

        var getExpResp = await _client.GetAsync($"/vehicles/{vehicle.Id}/expenses");
        getExpResp.StatusCode.Should().Be(HttpStatusCode.OK);

        var expenses = await getExpResp.Content.ReadFromJsonAsync<List<ExpenseFromFuelDto>>();
        expenses.Should().NotBeNull();

        expenses!
            .Should()
            .Contain(e =>
                e.Id == created.Id &&
                e.Amount == expectedTotal &&
                e.Category.Name.ToLower() == "paliwo");
    }
}
