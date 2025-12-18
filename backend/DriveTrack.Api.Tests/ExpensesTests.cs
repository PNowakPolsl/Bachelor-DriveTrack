using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;

namespace DriveTrack.Api.Tests;

public class ExpensesTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ExpensesTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    private record AuthResponseDto(
        Guid UserId,
        string Email,
        string Name,
        string Token
    );

    private record CategoryDtoSimple(
        Guid Id,
        string Name
    );

    private record CreatedExpenseCategoryDto(
        Guid Id,
        string Name
    );

    private record CreatedExpenseDto(
        Guid Id,
        DateOnly Date,
        decimal Amount,
        string? Description,
        int? OdometerKm,
        string CreatedByName,
        CreatedExpenseCategoryDto Category
    );

    private record ExpenseCategoryListDto(
        Guid CategoryId,
        string Name
    );

    private record ExpenseListItemDto(
        Guid Id,
        DateOnly Date,
        decimal Amount,
        string? Description,
        int? OdometerKm,
        string CreatedByName,
        ExpenseCategoryListDto Category
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

    // --- HELPERY ---

    private async Task<string> RegisterAndLoginAsync(string prefix = "exp")
    {
        var email = $"{prefix}_{Guid.NewGuid():N}@example.com";
        const string password = "Test123!";

        var registerBody = new
        {
            name = "Expenses Tester",
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

    private async Task<CategoryDtoSimple> CreateCategoryAsync(string name, Guid? ownerUserId = null)
{
    var body = new
    {
        name,
        ownerUserId
    };

    var resp = await _client.PostAsJsonAsync("/categories", body);

    // Dopuszczamy Created albo Conflict (już istnieje)
    resp.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.Conflict);

    // Teraz pobieramy listę kategorii i wybieramy tę o danej nazwie
    var url = "/categories";
    if (ownerUserId is not null)
    {
        url += $"?ownerUserId={ownerUserId}";
    }

    var listResp = await _client.GetAsync(url);
    listResp.StatusCode.Should().Be(HttpStatusCode.OK);

    var list = await listResp.Content.ReadFromJsonAsync<List<CategoryDtoSimple>>();
    list.Should().NotBeNull();

    var cat = list!.FirstOrDefault(c => c.Name == name);
    cat.Should().NotBeNull($"Category '{name}' should exist after POST or seed.");

    return cat!;
}

    private async Task<CreatedVehicleDto> CreateVehicleAsync(string token, string name = "Expenses Test Car")
    {
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var body = new
        {
            name,
            make = "TestMake",
            model = "TestModel",
            plate = "WX EXP",
            year = 2015,
            vin = "EXPVIN00000000001",
            baseOdometerKm = 100000
        };

        var resp = await _client.PostAsJsonAsync("/vehicles", body);
        resp.StatusCode.Should().Be(HttpStatusCode.Created);

        var created = await resp.Content.ReadFromJsonAsync<CreatedVehicleDto>();
        created.Should().NotBeNull();
        return created!;
    }

    // --- TESTY ---

    [Fact]
    public async Task CreateExpense_Then_GetExpenses_Should_Return_It()
    {
        // arrange: user, token, vehicle, category
        var token = await RegisterAndLoginAsync();
        var vehicle = await CreateVehicleAsync(token, "Car for expenses");

        var category = await CreateCategoryAsync("Serwis");

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var today = DateOnly.FromDateTime(DateTime.UtcNow.Date);

        var body = new
        {
            categoryId = category.Id,
            date = today,
            amount = 350.75m,
            description = "Wymiana klocków hamulcowych",
            odometerKm = 101000
        };

        // act 1: tworzymy wydatek
        var createResp = await _client.PostAsJsonAsync($"/vehicles/{vehicle.Id}/expenses", body);

        createResp.StatusCode.Should().Be(HttpStatusCode.Created);

        var created = await createResp.Content.ReadFromJsonAsync<CreatedExpenseDto>();
        created.Should().NotBeNull();
        created!.Amount.Should().Be(350.75m);
        created.Description.Should().Be("Wymiana klocków hamulcowych");
        created.Category.Name.Should().Be("Serwis");
        created.CreatedByName.Should().NotBeNullOrWhiteSpace();

        // act 2: pobieramy listę wydatków
        var listResp = await _client.GetAsync($"/vehicles/{vehicle.Id}/expenses");
        listResp.StatusCode.Should().Be(HttpStatusCode.OK);

        var list = await listResp.Content.ReadFromJsonAsync<List<ExpenseListItemDto>>();
        list.Should().NotBeNull();

        list!
            .Should()
            .Contain(e =>
                e.Id == created.Id &&
                e.Amount == 350.75m &&
                e.Description == "Wymiana klocków hamulcowych" &&
                e.Category.Name == "Serwis"
            );
    }

    [Fact]
    public async Task CreateExpense_WithoutToken_Should_Return_401()
    {
        var token = await RegisterAndLoginAsync();
        var vehicle = await CreateVehicleAsync(token, "Car no auth");

        var category = await CreateCategoryAsync("BezTokena");

        // brak Authorization header
        _client.DefaultRequestHeaders.Authorization = null;

        var today = DateOnly.FromDateTime(DateTime.UtcNow.Date);

        var body = new
        {
            categoryId = category.Id,
            date = today,
            amount = 100m,
            description = "Coś tam",
            odometerKm = 100500
        };

        var resp = await _client.PostAsJsonAsync($"/vehicles/{vehicle.Id}/expenses", body);

        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateExpense_For_ForeignVehicle_Should_Return_403()
    {
        // user1: ma auto
        var token1 = await RegisterAndLoginAsync("u1");
        var vehicle1 = await CreateVehicleAsync(token1, "Auto user1");

        var category = await CreateCategoryAsync("ObcyWydatek");

        // user2: próbuje dodać wydatek do auta user1
        var token2 = await RegisterAndLoginAsync("u2");
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token2);

        var today = DateOnly.FromDateTime(DateTime.UtcNow.Date);

        var body = new
        {
            categoryId = category.Id,
            date = today,
            amount = 123m,
            description = "Nielegalny wydatek",
            odometerKm = 100100
        };

        var resp = await _client.PostAsJsonAsync($"/vehicles/{vehicle1.Id}/expenses", body);

        resp.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CreateExpense_With_NonPositiveAmount_Should_Return_400()
    {
        var token = await RegisterAndLoginAsync();
        var vehicle = await CreateVehicleAsync(token, "Auto z błędnym wydatkiem");
        var category = await CreateCategoryAsync("BłądKwoty");

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var today = DateOnly.FromDateTime(DateTime.UtcNow.Date);

        var body = new
        {
            categoryId = category.Id,
            date = today,
            amount = 0m, // <= 0 -> BadRequest
            description = "Zły wydatek",
            odometerKm = 100500
        };

        var resp = await _client.PostAsJsonAsync($"/vehicles/{vehicle.Id}/expenses", body);

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var text = await resp.Content.ReadAsStringAsync();
        text.Should().Contain("Amount must be > 0");
    }

    [Fact]
    public async Task CreateExpense_With_OdometerLowerThanCurrent_Should_Return_400()
    {
        var token = await RegisterAndLoginAsync();
        var vehicle = await CreateVehicleAsync(token, "Auto z przebiegiem");
        var category = await CreateCategoryAsync("Przebieg");

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var today = DateOnly.FromDateTime(DateTime.UtcNow.Date);

        // 1) pierwszy wydatek z większym przebiegiem
        var body1 = new
        {
            categoryId = category.Id,
            date = today,
            amount = 200m,
            description = "Pierwszy serwis",
            odometerKm = 101000
        };

        var resp1 = await _client.PostAsJsonAsync($"/vehicles/{vehicle.Id}/expenses", body1);
        resp1.StatusCode.Should().Be(HttpStatusCode.Created);

        // 2) drugi wydatek z mniejszym przebiegiem -> powinno zwrócić 400
        var body2 = new
        {
            categoryId = category.Id,
            date = today,
            amount = 150m,
            description = "Drugi serwis",
            odometerKm = 100500 // mniejszy niż 101000
        };

        var resp2 = await _client.PostAsJsonAsync($"/vehicles/{vehicle.Id}/expenses", body2);
        resp2.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var text = await resp2.Content.ReadAsStringAsync();
        text.Should().Contain("OdometerKm must be >= current value");
    }

    [Fact]
    public async Task GetExpenses_FilteredByCategory_Should_Return_Only_Matching()
    {
        var token = await RegisterAndLoginAsync();
        var vehicle = await CreateVehicleAsync(token, "Auto z filtrami");
        var catFuel = await CreateCategoryAsync("Paliwo");
        var catService = await CreateCategoryAsync("Serwis");

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var today = DateOnly.FromDateTime(DateTime.UtcNow.Date);

        // wydatek paliwo
        var fuelBody = new
        {
            categoryId = catFuel.Id,
            date = today,
            amount = 250m,
            description = "Tankowanie",
            odometerKm = 100500
        };
        var fuelResp = await _client.PostAsJsonAsync($"/vehicles/{vehicle.Id}/expenses", fuelBody);
        fuelResp.StatusCode.Should().Be(HttpStatusCode.Created);

        // wydatek serwis
        var serviceBody = new
        {
            categoryId = catService.Id,
            date = today,
            amount = 500m,
            description = "Serwis",
            odometerKm = 101000
        };
        var serviceResp = await _client.PostAsJsonAsync($"/vehicles/{vehicle.Id}/expenses", serviceBody);
        serviceResp.StatusCode.Should().Be(HttpStatusCode.Created);

        // GET z filtrem categoryId = catFuel.Id
        var resp = await _client.GetAsync($"/vehicles/{vehicle.Id}/expenses?categoryId={catFuel.Id}");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);

        var list = await resp.Content.ReadFromJsonAsync<List<ExpenseListItemDto>>();
        list.Should().NotBeNull();

        list!.Should().OnlyContain(e => e.Category.CategoryId == catFuel.Id);
        list.Should().Contain(e => e.Description == "Tankowanie");
        list.Should().NotContain(e => e.Description == "Serwis");
    }
}
