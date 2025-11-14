using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace MetaFlow.Tests.IntegrationTests
{
    public class RegistroDiarioTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;

        public RegistroDiarioTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task GetRegistros_ReturnsExpectedStatus()
        {
            var client = _factory.CreateClient();

            var response = await client.GetAsync("/api/v1/registro-diario");

            Assert.True(
                response.StatusCode == HttpStatusCode.OK ||
                response.StatusCode == HttpStatusCode.NoContent ||
                response.StatusCode == HttpStatusCode.NotFound,
                $"GET /api/v1/registro-diario retornou {response.StatusCode}"
            );
        }

        [Fact]
        public async Task GetRegistrosByUsuario_ReturnsExpectedStatus()
        {
            var client = _factory.CreateClient();
            var usuarioId = Guid.NewGuid();

            var response = await client.GetAsync($"/api/v1/registro-diario/usuario/{usuarioId}");

            Assert.True(
                response.StatusCode == HttpStatusCode.OK ||
                response.StatusCode == HttpStatusCode.NoContent ||
                response.StatusCode == HttpStatusCode.NotFound,
                $"GET /api/v1/registro-diario/usuario/ retornou {response.StatusCode}"
            );
        }
    }
}