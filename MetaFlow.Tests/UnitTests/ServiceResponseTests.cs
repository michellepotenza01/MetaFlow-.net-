using MetaFlow.API.Services;
using MetaFlow.API.Models.Common;
using Xunit;

namespace MetaFlow.Tests.UnitTests
{
    public class ServiceResponseTests
    {
        [Fact]
        public void ServiceResponse_Ok_ReturnsSuccessResponse()
        {
            var response = ServiceResponse<string>.Ok("Test Data", "Operação bem-sucedida");

            Assert.True(response.Success);
            Assert.Equal("Test Data", response.Data);
            Assert.Equal("Operação bem-sucedida", response.Message);
            Assert.Empty(response.Errors);
        }

        [Fact]
        public void ServiceResponse_Error_ReturnsErrorResponse()
        {
            var response = ServiceResponse<string>.Error("Erro de teste");

            Assert.False(response.Success);
            Assert.Null(response.Data);
            Assert.Equal("Erro de teste", response.Message);
            Assert.Single(response.Errors);
        }

        [Fact]
        public void ServiceResponse_NotFound_ReturnsNotFoundMessage()
        {
            var response = ServiceResponse<string>.NotFound("Usuário");

            Assert.False(response.Success);
            Assert.Contains("Usuário", response.Message);
        }

        [Fact]
        public void ServiceResponse_WithLinks_AddsLinksCorrectly()
        {
            var links = new List<Link>
            {
                new Link("/api/test", "self", "GET"),
                new Link("/api/test", "update", "PUT")
            };

            var response = ServiceResponse<string>.Ok("Data", "Success")
                .WithLinks(links);

            Assert.Equal(2, response.Links.Count);
            Assert.Contains(response.Links, l => l.Rel == "self");
            Assert.Contains(response.Links, l => l.Rel == "update");
        }

        [Fact]
        public void ServiceResponse_WithLink_AddsSingleLink()
        {
            var response = ServiceResponse<string>.Ok("Data", "Success")
                .WithLink(new Link("/api/test", "self", "GET"));

            Assert.Single(response.Links);
            Assert.Equal("self", response.Links[0].Rel);
        }

        [Fact]
        public void ServiceResponse_IsValid_SuccessWithNoErrors_ReturnsTrue()
        {
            var response = ServiceResponse<string>.Ok("Data", "Success");

            var isValid = response.IsValid;

            Assert.True(isValid);
        }

        [Fact]
        public void ServiceResponse_IsValid_ErrorWithErrors_ReturnsFalse()
        {
            var response = ServiceResponse<string>.Error("Test error");

            var isValid = response.IsValid;

            Assert.False(isValid);
        }
    }
}