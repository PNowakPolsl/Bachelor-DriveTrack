using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;

namespace DriveTrack.Api.Tests;

public class VehiclesTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public VehiclesTests(CustomWebApplicationFactory factory)
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

    private record VehicleListItemDto(
        Guid Id,
        string Name,
        string Make,
        string Model,
        string? Plate,
        int? Year,
        string? Vin
    );

    private record VehicleDetailsDto(
        Guid Id,
        string Name,
        string Make,
        string Model,
        string? Plate,
        int? Year,
        string? Vin
    );

    /// <summary>
    /// Rejestruje użytkownika testowego, loguje go
    /// i zwraca token JWT.
    /// </summary>
    private async Task<string> RegisterAndLoginAsync()
    {
        var email = $"veh_{Guid.NewGuid():N}@example.com";
        const string password = "Test123!";

        var registerBody = new
        {
            name = "Vehicle Tester",
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

    [Fact]
    public async Task CreateVehicle_Then_GetVehicles_Should_Return_It()
    {
        var token = await RegisterAndLoginAsync();

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var createBody = new
        {
            name = "Test Auto",
            make = "Volkswagen",
            model = "Golf",
            plate = "WX TEST",
            year = 2010,
            vin = "TESTVIN1234567890",
            baseOdometerKm = 150000
        };

        var createResponse = await _client.PostAsJsonAsync("/vehicles", createBody);

        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var created =
            await createResponse.Content.ReadFromJsonAsync<CreatedVehicleDto>();

        created.Should().NotBeNull();
        created!.Name.Should().Be("Test Auto");

        var listResponse = await _client.GetAsync("/vehicles");
        listResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var vehicles =
            await listResponse.Content.ReadFromJsonAsync<List<VehicleListItemDto>>();

        vehicles.Should().NotBeNull();

        vehicles!
            .Should()
            .Contain(v => v.Id == created.Id &&
                          v.Name == "Test Auto" &&
                          v.Make == "Volkswagen" &&
                          v.Model == "Golf");
    }

    [Fact]
    public async Task GetVehicles_WithoutToken_Should_Return_401()
    {
        _client.DefaultRequestHeaders.Authorization = null;

        var response = await _client.GetAsync("/vehicles");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateVehicle_WithoutName_Should_Return_400()
    {
        var token = await RegisterAndLoginAsync();
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var body = new
        {
            name = "",
            make = "Audi",
            model = "A4",
            plate = "WX TEST2",
            year = 2015,
            vin = "VINVINVIN1234567",
            baseOdometerKm = 120000
        };

        var response = await _client.PostAsJsonAsync("/vehicles", body);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var text = await response.Content.ReadAsStringAsync();
        text.Should().Contain("Name is required");
    }

    [Fact]
    public async Task Users_Should_See_Only_Their_Vehicles_And_Forbidden_For_Others()
    {
        // User 1
        var token1 = await RegisterAndLoginAsync();
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token1);

        var createBody1 = new
        {
            name = "Auto User1",
            make = "Skoda",
            model = "Octavia",
            plate = "WX U1",
            year = 2018,
            vin = "U1VIN00000000001",
            baseOdometerKm = 100000
        };

        var resp1 = await _client.PostAsJsonAsync("/vehicles", createBody1);
        resp1.StatusCode.Should().Be(HttpStatusCode.Created);

        var created1 = await resp1.Content.ReadFromJsonAsync<CreatedVehicleDto>();
        created1.Should().NotBeNull();

        // User 2
        var token2 = await RegisterAndLoginAsync();
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token2);

        var createBody2 = new
        {
            name = "Auto User2",
            make = "Ford",
            model = "Focus",
            plate = "WX U2",
            year = 2016,
            vin = "U2VIN00000000002",
            baseOdometerKm = 80000
        };

        var resp2 = await _client.PostAsJsonAsync("/vehicles", createBody2);
        resp2.StatusCode.Should().Be(HttpStatusCode.Created);

        var created2 = await resp2.Content.ReadFromJsonAsync<CreatedVehicleDto>();
        created2.Should().NotBeNull();

        var listResp = await _client.GetAsync("/vehicles");
        listResp.StatusCode.Should().Be(HttpStatusCode.OK);

        var vehicles =
            await listResp.Content.ReadFromJsonAsync<List<VehicleListItemDto>>();

        vehicles.Should().NotBeNull();
        vehicles!.Should().ContainSingle(v => v.Id == created2!.Id);
        vehicles.Should().NotContain(v => v.Id == created1!.Id);

        var forbiddenResp = await _client.GetAsync($"/vehicles/{created1!.Id}");
        forbiddenResp.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task UpdateVehicle_Should_Change_Data()
    {
        var token = await RegisterAndLoginAsync();
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var createBody = new
        {
            name = "Do edycji",
            make = "BMW",
            model = "E46",
            plate = "WX EDIT",
            year = 2003,
            vin = "EDITVIN0000000003",
            baseOdometerKm = 220000
        };

        var createResp = await _client.PostAsJsonAsync("/vehicles", createBody);
        createResp.StatusCode.Should().Be(HttpStatusCode.Created);

        var created = await createResp.Content.ReadFromJsonAsync<CreatedVehicleDto>();
        created.Should().NotBeNull();

        var updateBody = new
        {
            name = "Po edycji",
            make = "BMW",
            model = "E46",
            plate = "WX EDIT2",
            year = 2004,
            vin = "EDITVIN0000000003",
            baseOdometerKm = 225000
        };

        var updateResp = await _client.PutAsJsonAsync($"/vehicles/{created!.Id}", updateBody);
        updateResp.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getResp = await _client.GetAsync($"/vehicles/{created.Id}");
        getResp.StatusCode.Should().Be(HttpStatusCode.OK);

        var details = await getResp.Content.ReadFromJsonAsync<VehicleDetailsDto>();
        details.Should().NotBeNull();
        details!.Name.Should().Be("Po edycji");
        details.Plate.Should().Be("WX EDIT2");
        details.Year.Should().Be(2004);
    }

    [Fact]
    public async Task DeleteVehicle_Should_Remove_It()
    {
        var token = await RegisterAndLoginAsync();
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var createBody = new
        {
            name = "Do usunięcia",
            make = "Opel",
            model = "Astra",
            plate = "WX DEL",
            year = 2012,
            vin = "DELVIN0000000001",
            baseOdometerKm = 160000
        };

        var createResp = await _client.PostAsJsonAsync("/vehicles", createBody);
        createResp.StatusCode.Should().Be(HttpStatusCode.Created);

        var created = await createResp.Content.ReadFromJsonAsync<CreatedVehicleDto>();
        created.Should().NotBeNull();

        var deleteResp = await _client.DeleteAsync($"/vehicles/{created!.Id}");
        deleteResp.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var listResp = await _client.GetAsync("/vehicles");
        listResp.StatusCode.Should().Be(HttpStatusCode.OK);

        var vehicles =
            await listResp.Content.ReadFromJsonAsync<List<VehicleListItemDto>>();

        vehicles.Should().NotBeNull();
        vehicles!.Should().NotContain(v => v.Id == created.Id);

        var getResp = await _client.GetAsync($"/vehicles/{created.Id}");
        getResp.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
