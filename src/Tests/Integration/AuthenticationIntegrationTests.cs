using System.Net.Http.Headers;
using Application.Dtos;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace MoodTracker_back.Tests.Integration;

public class AuthenticationIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AuthenticationIntegrationTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient(); // Inicializa o cliente HTTP usando o WebApplicationFactory
    }

    [Fact]
    public async Task FullAuthenticationFlow_Success()
    {
        // Registro
        var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", new
        {
            Email = "test@example.com",
            Password = "Password123!"
        });
        Assert.True(registerResponse.IsSuccessStatusCode);
        var registerResult = await registerResponse.Content.ReadFromJsonAsync<AuthResult>();
        Assert.NotNull(registerResult?.Token);

        // Login
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            Email = "test@example.com",
            Password = "Password123!"
        });
        Assert.True(loginResponse.IsSuccessStatusCode);
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<AuthResult>();
        Assert.NotNull(loginResult?.Token);

        // Teste endpoint protegido
        _client.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", loginResult.Token);
        var protectedResponse = await _client.GetAsync("/api/protected-endpoint");
        Assert.True(protectedResponse.IsSuccessStatusCode);

        // Logout
        var logoutResponse = await _client.PostAsJsonAsync("/api/auth/logout", new
        {
            RefreshToken = loginResult.RefreshToken
        });
        Assert.True(logoutResponse.IsSuccessStatusCode);
    }
}