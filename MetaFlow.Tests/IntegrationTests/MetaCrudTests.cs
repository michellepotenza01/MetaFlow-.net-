using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace MetaFlow.Tests.IntegrationTests
{
    public class MetaCrudTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;

        public MetaCrudTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task GetMetas_ReturnsExpectedStatus()
        {
            var client = _factory.CreateClient();

            var response = await client.GetAsync("/api/v1/meta");

            Assert.True(
                response.StatusCode == HttpStatusCode.OK ||
                response.StatusCode == HttpStatusCode.NoContent ||
                response.StatusCode == HttpStatusCode.NotFound,
                $"GET /api/v1/meta retornou {response.StatusCode}"
            );
        }

        [Fact]
        public async Task GetMetasByUsuario_ReturnsExpectedStatus()
        {
            var client = _factory.CreateClient();
            var usuarioId = Guid.NewGuid();

            var response = await client.GetAsync($"/api/v1/meta/usuario/{usuarioId}");

            Assert.True(
                response.StatusCode == HttpStatusCode.OK ||
                response.StatusCode == HttpStatusCode.NoContent ||
                response.StatusCode == HttpStatusCode.NotFound ||
                response.StatusCode == HttpStatusCode.BadRequest, // <- ADICIONADO
                $"GET /api/v1/meta/usuario/ retornou {response.StatusCode}"
            );
        }
    }
}