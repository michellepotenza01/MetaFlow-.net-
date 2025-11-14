using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace MetaFlow.Tests.IntegrationTests
{
    public class BasicApiTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;

        public BasicApiTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task HomePage_ReturnsSuccess()
        {
            var client = _factory.CreateClient();
            var response = await client.GetAsync("/");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task Swagger_ReturnsSuccess()
        {
            var client = _factory.CreateClient();
            var response = await client.GetAsync("/swagger");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task ApiEndpoints_AreAccessible()
        {
            var client = _factory.CreateClient();
            
            // Testa endpoints básicos com status esperados
            var endpoints = new[]
            {
                "/api/v1/meta",
                "/api/v1/registro-diario", 
                "/api/v1/usuarios",
                "/api/v1/auth/validate"
            };

            foreach (var endpoint in endpoints)
            {
                var response = await client.GetAsync(endpoint);
                Assert.True(
                    response.IsSuccessStatusCode || 
                    response.StatusCode == HttpStatusCode.NotFound ||
                    response.StatusCode == HttpStatusCode.NoContent,
                    $"Endpoint {endpoint} retornou {response.StatusCode}"
                );
            }
        }
    }
}