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
            _authService = new AuthService(null!, null!);
        }

        [Fact]
        public void HashPassword_ValidPassword_ReturnsHashedPassword()
        {
            var password = "TestPassword123";

            var hashedPassword = _authService.HashPassword(password);

            Assert.NotNull(hashedPassword);
            Assert.NotEqual(password, hashedPassword);
            Assert.True(hashedPassword.Length > 0);
        }

        [Fact]
        public void VerifyPassword_CorrectPassword_ReturnsTrue()
        {
            var password = "TestPassword123";
            var hashedPassword = _authService.HashPassword(password);

             var result = _authService.VerifyPassword(password, hashedPassword);

             Assert.True(result);
        }

        [Fact]
        public void VerifyPassword_WrongPassword_ReturnsFalse()
        {
            var correctPassword = "TestPassword123";
            var wrongPassword = "WrongPassword123";
            var hashedPassword = _authService.HashPassword(correctPassword);

             var result = _authService.VerifyPassword(wrongPassword, hashedPassword);

             Assert.False(result);
        }

        [Fact]
        public void VerifyPassword_EmptyPassword_ReturnsFalse()
        {
             var hashedPassword = _authService.HashPassword("SomePassword");

             var result = _authService.VerifyPassword("", hashedPassword);

             Assert.False(result);
        }
    }
}