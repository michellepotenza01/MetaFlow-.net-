using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace MetaFlow.Tests.IntegrationTests
{
    public class MLTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;

        public MLTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task GerarRecomendacoes_ReturnsSuccess()
        {
            var client = _factory.CreateClient();
            var usuarioId = Guid.NewGuid();

            var response = await client.GetAsync($"/api/v2/recomendacoes/usuario/{usuarioId}");

            Assert.True(
                response.StatusCode == HttpStatusCode.OK ||
                response.StatusCode == HttpStatusCode.BadRequest,
                $"GET /api/v2/recomendacoes/usuario/ retornou {response.StatusCode}"
            );
        }

        [Fact]
        public async Task StatusServicoML_ReturnsSuccess()
        {
            var client = _factory.CreateClient();

            var response = await client.GetAsync("/api/v2/recomendacoes/status");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task AnalisarPadroes_ReturnsSuccess()
        {
            var client = _factory.CreateClient();
            var usuarioId = Guid.NewGuid();

            var response = await client.GetAsync($"/api/v2/analises/usuario/{usuarioId}/padroes");

            Assert.True(
                response.StatusCode == HttpStatusCode.OK ||
                response.StatusCode == HttpStatusCode.Forbidden,
                $"GET /api/v2/analises/usuario/padroes retornou {response.StatusCode}"
            );
        }
    }
}