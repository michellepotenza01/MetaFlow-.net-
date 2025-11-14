using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace MetaFlow.Tests.IntegrationTests
{
    public class DashboardTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;

        public DashboardTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task GetDashboardUsuario_ReturnsSuccess()
        {
            var client = _factory.CreateClient();
            var usuarioId = Guid.NewGuid();

            var response = await client.GetAsync($"/api/v1/dashboard/usuario/{usuarioId}");

            Assert.True(
                response.StatusCode == HttpStatusCode.OK ||
                response.StatusCode == HttpStatusCode.Forbidden,
                $"GET /api/v1/dashboard/usuario/ retornou {response.StatusCode}"
            );
        }

        [Fact]
        public async Task GetQuickStats_ReturnsSuccess()
        {
            var client = _factory.CreateClient();
            var usuarioId = Guid.NewGuid();

            var response = await client.GetAsync($"/api/v2/dashboard/usuario/{usuarioId}/quick-stats");

            Assert.True(
                response.StatusCode == HttpStatusCode.OK ||
                response.StatusCode == HttpStatusCode.Forbidden,
                $"GET /api/v2/dashboard/usuario/quick-stats retornou {response.StatusCode}"
            );
        }

        [Fact]
        public async Task GetAlertas_ReturnsSuccess()
        {
            var client = _factory.CreateClient();
            var usuarioId = Guid.NewGuid();

            var response = await client.GetAsync($"/api/v2/dashboard/usuario/{usuarioId}/alertas");

            Assert.True(
                response.StatusCode == HttpStatusCode.OK ||
                response.StatusCode == HttpStatusCode.Forbidden,
                $"GET /api/v2/dashboard/usuario/alertas retornou {response.StatusCode}"
            );
        }
    }
}