using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace MetaFlow.Tests.IntegrationTests
{
    public class HealthCheckTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;

        public HealthCheckTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task HealthCheck_ReturnsExpectedStatus()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync("/api/v1/health");

            // Assert
            Assert.True(
                response.StatusCode == HttpStatusCode.OK ||
                response.StatusCode == HttpStatusCode.ServiceUnavailable,
                $"Health check retornou {response.StatusCode}"
            );
        }

        [Fact]
        public async Task HealthCheckV2_ReturnsExpectedStatus()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync("/api/v2/health");

            // Assert
            Assert.True(
                response.StatusCode == HttpStatusCode.OK ||
                response.StatusCode == HttpStatusCode.ServiceUnavailable,
                $"Health check V2 retornou {response.StatusCode}"
            );
        }
    }
}