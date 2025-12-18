using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;

namespace DriveTrack.Api.Tests;

public class AuthTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AuthTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    private record AuthResponseDto(
        Guid UserId,
        string Email,
        string Name,
        string Token
    );

    [Fact]
    public async Task Register_Then_Login_Should_Work()
    {
        var email = $"test_{Guid.NewGuid():N}@example.com";
        const string password = "Test123!";

        var registerBody = new
        {
            name = "Test User",
            email,
            password
        };

        var loginBody = new
        {
            email,
            password
        };

        var registerResponse = await _client.PostAsJsonAsync("/auth/register", registerBody);

        registerResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var loginResponse = await _client.PostAsJsonAsync("/auth/login", loginBody);

        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var auth = await loginResponse.Content.ReadFromJsonAsync<AuthResponseDto>();

        auth.Should().NotBeNull();
        auth!.Email.Should().Be(email);
        auth.Token.Should().NotBeNullOrWhiteSpace();
        auth.UserId.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public async Task Register_WithExistingEmail_Should_Return_Conflict()
    {
        var email = $"dup_{Guid.NewGuid():N}@example.com";
        const string password = "Test123!";

        var body = new
        {
            name = "User 1",
            email,
            password
        };

        // pierwsza rejestracja – OK
        var r1 = await _client.PostAsJsonAsync("/auth/register", body);
        r1.StatusCode.Should().Be(HttpStatusCode.OK);

        // druga rejestracja tym samym mailem – powinno być 409
        var r2 = await _client.PostAsJsonAsync("/auth/register", body);
        r2.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Login_WithWrongPassword_Should_Return_BadRequest()
    {
        var email = $"login_{Guid.NewGuid():N}@example.com";
        const string password = "Test123!";

        // najpierw rejestracja
        var registerBody = new { name = "Test User", email, password };
        var r = await _client.PostAsJsonAsync("/auth/register", registerBody);
        r.StatusCode.Should().Be(HttpStatusCode.OK);

        // teraz logowanie ze złym hasłem
        var loginBody = new { email, password = "ZleHaslo999!" };
        var loginResponse = await _client.PostAsJsonAsync("/auth/login", loginBody);

        loginResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login_WithoutEmailOrPassword_Should_Return_BadRequest()
    {
        var body = new
        {
            email = "",
            password = ""
        };

        var response = await _client.PostAsJsonAsync("/auth/login", body);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }







}
