using MetaFlow.API.Models.Auth;
using MetaFlow.API.Services;
using Xunit;

namespace MetaFlow.Tests.UnitTests
{
    public class AuthServiceTests
    {
        private readonly AuthService _authService;

        public AuthServiceTests()
        {
            // Criando instância sem dependências para testes básicos
            _authService = new AuthService(null!, null!);
        }

        [Fact]
        public void HashPassword_ValidPassword_ReturnsHashedPassword()
        {
            // Arrange
            var password = "TestPassword123";

            // Act
            var hashedPassword = _authService.HashPassword(password);

            // Assert
            Assert.NotNull(hashedPassword);
            Assert.NotEqual(password, hashedPassword);
            Assert.True(hashedPassword.Length > 0);
        }

        [Fact]
        public void VerifyPassword_CorrectPassword_ReturnsTrue()
        {
            // Arrange
            var password = "TestPassword123";
            var hashedPassword = _authService.HashPassword(password);

            // Act
            var result = _authService.VerifyPassword(password, hashedPassword);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void VerifyPassword_WrongPassword_ReturnsFalse()
        {
            // Arrange
            var correctPassword = "TestPassword123";
            var wrongPassword = "WrongPassword123";
            var hashedPassword = _authService.HashPassword(correctPassword);

            // Act
            var result = _authService.VerifyPassword(wrongPassword, hashedPassword);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void VerifyPassword_EmptyPassword_ReturnsFalse()
        {
            // Arrange
            var hashedPassword = _authService.HashPassword("SomePassword");

            // Act
            var result = _authService.VerifyPassword("", hashedPassword);

            // Assert
            Assert.False(result);
        }
    }
}