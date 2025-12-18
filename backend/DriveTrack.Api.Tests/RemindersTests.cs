using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;

namespace DriveTrack.Api.Tests;

public class RemindersTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public RemindersTests(CustomWebApplicationFactory factory)
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

    private record ReminderDto(
        Guid Id,
        Guid VehicleId,
        string Title,
        string? Description,
        DateOnly DueDate,
        bool IsCompleted,
        DateTime? CompletedAt,
        DateTime CreatedAt,
        string CreatedByName
    );

    // --- HELPERY ---

    private async Task<string> RegisterAndLoginAsync(string prefix = "rem")
    {
        var email = $"{prefix}_{Guid.NewGuid():N}@example.com";
        const string password = "Test123!";

        var registerBody = new
        {
            name = "Reminder Tester",
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

    private async Task<CreatedVehicleDto> CreateVehicleAsync(string token, string name = "Reminder Test Car")
    {
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var body = new
        {
            name,
            make = "TestMake",
            model = "TestModel",
            plate = "WX REM",
            year = 2019,
            vin = "REMVIN00000000001",
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
    public async Task CreateReminder_Then_GetReminders_Should_Return_It()
    {
        var token = await RegisterAndLoginAsync();
        var vehicle = await CreateVehicleAsync(token, "Auto z przypomnieniem");

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var today = DateOnly.FromDateTime(DateTime.UtcNow.Date);
        var due = today.AddDays(7);

        var body = new
        {
            title = "Przegląd techniczny",
            description = "Roczny przegląd pojazdu",
            dueDate = due
        };

        // POST
        var postResp = await _client.PostAsJsonAsync($"/vehicles/{vehicle.Id}/reminders", body);
        postResp.StatusCode.Should().Be(HttpStatusCode.Created);

        var created = await postResp.Content.ReadFromJsonAsync<ReminderDto>();
        created.Should().NotBeNull();

        created!.Title.Should().Be("Przegląd techniczny");
        created.Description.Should().Be("Roczny przegląd pojazdu");
        created.DueDate.Should().Be(due);
        created.IsCompleted.Should().BeFalse();
        created.CreatedByName.Should().NotBeNullOrWhiteSpace();

        // GET lista
        var getResp = await _client.GetAsync($"/vehicles/{vehicle.Id}/reminders");
        getResp.StatusCode.Should().Be(HttpStatusCode.OK);

        var list = await getResp.Content.ReadFromJsonAsync<List<ReminderDto>>();
        list.Should().NotBeNull();

        list!
            .Should()
            .Contain(r =>
                r.Id == created.Id &&
                r.Title == "Przegląd techniczny" &&
                r.DueDate == due);
    }

    [Fact]
    public async Task CreateReminder_WithoutToken_Should_Return_401()
    {
        var token = await RegisterAndLoginAsync();
        var vehicle = await CreateVehicleAsync(token, "Auto bez tokena");

        _client.DefaultRequestHeaders.Authorization = null;

        var today = DateOnly.FromDateTime(DateTime.UtcNow.Date);

        var body = new
        {
            title = "Coś tam",
            description = "Opis",
            dueDate = today.AddDays(3)
        };

        var resp = await _client.PostAsJsonAsync($"/vehicles/{vehicle.Id}/reminders", body);
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateReminder_For_ForeignVehicle_Should_Return_403()
    {
        // user1 z autem
        var token1 = await RegisterAndLoginAsync("r1");
        var vehicle1 = await CreateVehicleAsync(token1, "Auto user1");

        // user2 próbuje dodać przypomnienie do auta user1
        var token2 = await RegisterAndLoginAsync("r2");
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token2);

        var body = new
        {
            title = "Nielegalne przypomnienie",
            description = "Coś",
            dueDate = DateOnly.FromDateTime(DateTime.UtcNow.Date).AddDays(2)
        };

        var resp = await _client.PostAsJsonAsync($"/vehicles/{vehicle1.Id}/reminders", body);
        resp.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CreateReminder_WithoutTitle_Should_Return_400()
    {
        var token = await RegisterAndLoginAsync();
        var vehicle = await CreateVehicleAsync(token, "Auto z błędnym przypomnieniem");

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var body = new
        {
            title = "",
            description = "Brak tytułu",
            dueDate = DateOnly.FromDateTime(DateTime.UtcNow.Date).AddDays(5)
        };

        var resp = await _client.PostAsJsonAsync($"/vehicles/{vehicle.Id}/reminders", body);
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var text = await resp.Content.ReadAsStringAsync();
        text.Should().Contain("Title is required");
    }

    [Fact]
    public async Task GetReminders_WithoutToken_Should_Return_401()
    {
        var token = await RegisterAndLoginAsync();
        var vehicle = await CreateVehicleAsync(token, "Auto do GET bez tokena");

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var today = DateOnly.FromDateTime(DateTime.UtcNow.Date);

        var body = new
        {
            title = "Jakieś przypomnienie",
            description = "Opis",
            dueDate = today.AddDays(1)
        };

        var postResp = await _client.PostAsJsonAsync($"/vehicles/{vehicle.Id}/reminders", body);
        postResp.StatusCode.Should().Be(HttpStatusCode.Created);

        // GET bez tokena
        _client.DefaultRequestHeaders.Authorization = null;

        var getResp = await _client.GetAsync($"/vehicles/{vehicle.Id}/reminders");
        getResp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetReminders_OnlyOverdue_Should_Return_Only_Overdue_And_NotCompleted()
    {
        var token = await RegisterAndLoginAsync();
        var vehicle = await CreateVehicleAsync(token, "Auto z zaległymi");

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var today = DateOnly.FromDateTime(DateTime.UtcNow.Date);

        // zaległe, nieukończone
        var overdueBody = new
        {
            title = "Stare niezrobione",
            description = "Stare",
            dueDate = today.AddDays(-3)
        };
        var overdueResp = await _client.PostAsJsonAsync($"/vehicles/{vehicle.Id}/reminders", overdueBody);
        overdueResp.StatusCode.Should().Be(HttpStatusCode.Created);

        // nadchodzące
        var upcomingBody = new
        {
            title = "Nadchodzące",
            description = "W przyszłości",
            dueDate = today.AddDays(5)
        };
        var upcomingResp = await _client.PostAsJsonAsync($"/vehicles/{vehicle.Id}/reminders", upcomingBody);
        upcomingResp.StatusCode.Should().Be(HttpStatusCode.Created);

        // zaległe ale już zrobione
        var doneBody = new
        {
            title = "Stare zrobione",
            description = "Zrobione",
            dueDate = today.AddDays(-10)
        };
        var doneResp = await _client.PostAsJsonAsync($"/vehicles/{vehicle.Id}/reminders", doneBody);
        doneResp.StatusCode.Should().Be(HttpStatusCode.Created);

        var doneReminder = await doneResp.Content.ReadFromJsonAsync<ReminderDto>();
        doneReminder.Should().NotBeNull();

        // oznacz jako wykonane
        var patchBody = new { isCompleted = true };
        var patchResp = await _client.PatchAsync($"/reminders/{doneReminder!.Id}", JsonContent.Create(patchBody));
        patchResp.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // GET onlyOverdue
        var getResp = await _client.GetAsync($"/vehicles/{vehicle.Id}/reminders?onlyOverdue=true");
        getResp.StatusCode.Should().Be(HttpStatusCode.OK);

        var list = await getResp.Content.ReadFromJsonAsync<List<ReminderDto>>();
        list.Should().NotBeNull();

        var result = list!;
        result.Should().NotBeEmpty();
        result.Should().OnlyContain(r => !r.IsCompleted && r.DueDate < today);

        result.Should().Contain(r => r.Title == "Stare niezrobione");
        result.Should().NotContain(r => r.Title == "Nadchodzące");
        result.Should().NotContain(r => r.Title == "Stare zrobione");
    }

    [Fact]
    public async Task GetReminders_OnlyUpcoming_Should_Return_Only_Upcoming_NotCompleted()
    {
        var token = await RegisterAndLoginAsync();
        var vehicle = await CreateVehicleAsync(token, "Auto z przyszłymi");

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var today = DateOnly.FromDateTime(DateTime.UtcNow.Date);

        // przeszłe
        var oldBody = new
        {
            title = "Stare",
            description = "Przeszłe",
            dueDate = today.AddDays(-2)
        };
        var oldResp = await _client.PostAsJsonAsync($"/vehicles/{vehicle.Id}/reminders", oldBody);
        oldResp.StatusCode.Should().Be(HttpStatusCode.Created);

        // przyszłe 1
        var up1Body = new
        {
            title = "Up1",
            description = "Przyszłe 1",
            dueDate = today.AddDays(3)
        };
        var up1Resp = await _client.PostAsJsonAsync($"/vehicles/{vehicle.Id}/reminders", up1Body);
        up1Resp.StatusCode.Should().Be(HttpStatusCode.Created);

        // przyszłe 2
        var up2Body = new
        {
            title = "Up2",
            description = "Przyszłe 2",
            dueDate = today.AddDays(10)
        };
        var up2Resp = await _client.PostAsJsonAsync($"/vehicles/{vehicle.Id}/reminders", up2Body);
        up2Resp.StatusCode.Should().Be(HttpStatusCode.Created);

        var getResp = await _client.GetAsync($"/vehicles/{vehicle.Id}/reminders?onlyUpcoming=true");
        getResp.StatusCode.Should().Be(HttpStatusCode.OK);

        var list = await getResp.Content.ReadFromJsonAsync<List<ReminderDto>>();
        list.Should().NotBeNull();

        var result = list!;
        result.Should().NotBeEmpty();
        result.Should().OnlyContain(r => !r.IsCompleted && r.DueDate >= today);

        result.Should().Contain(r => r.Title == "Up1");
        result.Should().Contain(r => r.Title == "Up2");
        result.Should().NotContain(r => r.Title == "Stare");
    }

    [Fact]
    public async Task GetReminders_Overdue_And_Upcoming_Flags_Both_Should_Return_400()
    {
        var token = await RegisterAndLoginAsync();
        var vehicle = await CreateVehicleAsync(token);

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var url = $"/vehicles/{vehicle.Id}/reminders?onlyOverdue=true&onlyUpcoming=true";

        var resp = await _client.GetAsync(url);
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var text = await resp.Content.ReadAsStringAsync();
        text.Should().Contain("Choose only one of: onlyOverdue or onlyUpcoming");
    }

    [Fact]
    public async Task PatchReminder_Should_Update_Fields_And_Completion()
    {
        var token = await RegisterAndLoginAsync();
        var vehicle = await CreateVehicleAsync(token, "Auto do patch");

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var today = DateOnly.FromDateTime(DateTime.UtcNow.Date);

        var createBody = new
        {
            title = "Stary tytuł",
            description = "Stary opis",
            dueDate = today.AddDays(3)
        };

        var postResp = await _client.PostAsJsonAsync($"/vehicles/{vehicle.Id}/reminders", createBody);
        postResp.StatusCode.Should().Be(HttpStatusCode.Created);

        var reminder = await postResp.Content.ReadFromJsonAsync<ReminderDto>();
        reminder.Should().NotBeNull();

        var patchBody = new
        {
            title = "Nowy tytuł",
            description = "Nowy opis",
            dueDate = today.AddDays(10),
            isCompleted = true
        };

        var patchResp = await _client.PatchAsync($"/reminders/{reminder!.Id}", JsonContent.Create(patchBody));
        patchResp.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // GET i sprawdzamy zmiany
        var getResp = await _client.GetAsync($"/vehicles/{vehicle.Id}/reminders");
        getResp.StatusCode.Should().Be(HttpStatusCode.OK);

        var list = await getResp.Content.ReadFromJsonAsync<List<ReminderDto>>();
        list.Should().NotBeNull();

        var updated = list!.Single(r => r.Id == reminder.Id);
        updated.Title.Should().Be("Nowy tytuł");
        updated.Description.Should().Be("Nowy opis");
        updated.DueDate.Should().Be(today.AddDays(10));
        updated.IsCompleted.Should().BeTrue();
        updated.CompletedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task PatchReminder_WithoutAccess_Should_Return_403()
    {
        // user1 z przypomnieniem
        var token1 = await RegisterAndLoginAsync("p1");
        var vehicle1 = await CreateVehicleAsync(token1, "Auto p1");

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token1);

        var createBody = new
        {
            title = "Do edycji",
            description = "Opis",
            dueDate = DateOnly.FromDateTime(DateTime.UtcNow.Date).AddDays(4)
        };

        var postResp = await _client.PostAsJsonAsync($"/vehicles/{vehicle1.Id}/reminders", createBody);
        postResp.StatusCode.Should().Be(HttpStatusCode.Created);

        var reminder = await postResp.Content.ReadFromJsonAsync<ReminderDto>();
        reminder.Should().NotBeNull();

        // user2 próbuje patch
        var token2 = await RegisterAndLoginAsync("p2");
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token2);

        var patchBody = new { title = "Nie powinno się udać" };

        var patchResp = await _client.PatchAsync($"/reminders/{reminder!.Id}", JsonContent.Create(patchBody));
        patchResp.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task DeleteReminder_Should_Remove_It()
    {
        var token = await RegisterAndLoginAsync();
        var vehicle = await CreateVehicleAsync(token, "Auto do usunięcia przypomnienia");

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var body = new
        {
            title = "Do usunięcia",
            description = "Opis",
            dueDate = DateOnly.FromDateTime(DateTime.UtcNow.Date).AddDays(2)
        };

        var postResp = await _client.PostAsJsonAsync($"/vehicles/{vehicle.Id}/reminders", body);
        postResp.StatusCode.Should().Be(HttpStatusCode.Created);

        var reminder = await postResp.Content.ReadFromJsonAsync<ReminderDto>();
        reminder.Should().NotBeNull();

        // DELETE
        var delResp = await _client.DeleteAsync($"/reminders/{reminder!.Id}");
        delResp.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // lista nie powinna zawierać
        var getResp = await _client.GetAsync($"/vehicles/{vehicle.Id}/reminders");
        getResp.StatusCode.Should().Be(HttpStatusCode.OK);

        var list = await getResp.Content.ReadFromJsonAsync<List<ReminderDto>>();
        list.Should().NotBeNull();

        list!.Should().NotContain(r => r.Id == reminder.Id);

        // drugi DELETE -> 404
        var del2Resp = await _client.DeleteAsync($"/reminders/{reminder.Id}");
        del2Resp.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteReminder_WithoutAccess_Should_Return_403()
    {
        // user1
        var token1 = await RegisterAndLoginAsync("d1");
        var vehicle1 = await CreateVehicleAsync(token1, "Auto d1");

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token1);

        var createBody = new
        {
            title = "Do usunięcia przez obcego",
            description = "Opis",
            dueDate = DateOnly.FromDateTime(DateTime.UtcNow.Date).AddDays(3)
        };

        var postResp = await _client.PostAsJsonAsync($"/vehicles/{vehicle1.Id}/reminders", createBody);
        postResp.StatusCode.Should().Be(HttpStatusCode.Created);

        var reminder = await postResp.Content.ReadFromJsonAsync<ReminderDto>();
        reminder.Should().NotBeNull();

        // user2 próbuje DELETE
        var token2 = await RegisterAndLoginAsync("d2");
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token2);

        var delResp = await _client.DeleteAsync($"/reminders/{reminder!.Id}");
        delResp.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
