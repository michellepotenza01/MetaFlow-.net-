using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using MetaFlow.API.Models.Auth;
using Xunit;

namespace MetaFlow.Tests.IntegrationTests
{
    public class AuthTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;

        public AuthTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task Login_WithInvalidCredentials_ReturnsUnauthorized()
        {
            var client = _factory.CreateClient();
            var loginRequest = new LoginRequest
            {
                Email = "invalid@example.com",
                Senha = "WrongPassword123!"
            };

            var response = await client.PostAsJsonAsync("/api/v1/auth/login", loginRequest);

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task ValidateToken_ReturnsSuccess()
        {
            var client = _factory.CreateClient();

            var response = await client.GetAsync("/api/v1/auth/validate");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task GetCurrentUser_ReturnsUserInfo()
        {
            var client = _factory.CreateClient();

            var response = await client.GetAsync("/api/v2/auth/me");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }
}