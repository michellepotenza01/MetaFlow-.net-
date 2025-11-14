using MetaFlow.API.Models.Auth;
using MetaFlow.API.Repositories;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using MetaFlow.API.Models;
using MetaFlow.API.DTOs;

namespace MetaFlow.API.Services
{
    public interface IAuthService
    {
        Task<ServiceResponse<LoginResponse>> LoginAsync(LoginRequest request);
        Task<ServiceResponse<Usuario>> RegistrarAsync(UsuarioRequestDto usuarioDto);
        string HashPassword(string password);
        bool VerifyPassword(string password, string passwordHash);
    }

    public class AuthService : IAuthService
    {
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly IConfiguration _configuration;

        public AuthService(IUsuarioRepository usuarioRepository, IConfiguration configuration)
        {
            _usuarioRepository = usuarioRepository;
            _configuration = configuration;
        }

        public async Task<ServiceResponse<LoginResponse>> LoginAsync(LoginRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Senha))
                    return ServiceResponse<LoginResponse>.Error("Email e senha são obrigatórios");

                var usuario = await _usuarioRepository.GetByEmailAsync(request.Email);
                if (usuario is null)
                    return ServiceResponse<LoginResponse>.Error("Credenciais inválidas");

                if (!VerifyPassword(request.Senha, usuario.SenhaHash))
                    return ServiceResponse<LoginResponse>.Error("Credenciais inválidas");

                var token = GenerateJwtToken(usuario);
                
                return ServiceResponse<LoginResponse>.Ok(token, "Login realizado com sucesso");
            }
            catch (Exception ex)
            {
                return ServiceResponse<LoginResponse>.Error($"Erro durante login: {ex.Message}");
            }
        }

        public async Task<ServiceResponse<Usuario>> RegistrarAsync(UsuarioRequestDto usuarioDto)
        {
            try
            {
                if (await _usuarioRepository.EmailExistsAsync(usuarioDto.Email))
                    return ServiceResponse<Usuario>.Conflict("Email já cadastrado");

                var usuario = new Usuario
                {
                    Id = Guid.NewGuid(),
                    Nome = usuarioDto.Nome.Trim(),
                    Email = usuarioDto.Email.Trim(),
                    SenhaHash = HashPassword(usuarioDto.Senha),
                    Profissao = usuarioDto.Profissao?.Trim(),
                    ObjetivoProfissional = usuarioDto.ObjetivoProfissional?.Trim(),
                    CriadoEm = DateTime.UtcNow,
                    AtualizadoEm = DateTime.UtcNow
                };

                await _usuarioRepository.AddAsync(usuario);
                
                return ServiceResponse<Usuario>.Ok(usuario, "Usuário registrado com sucesso");
            }
            catch (Exception ex)
            {
                return ServiceResponse<Usuario>.Error($"Erro durante registro: {ex.Message}");
            }
        }

        private LoginResponse GenerateJwtToken(Usuario usuario)
        {
            var jwtKey = _configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key não configurada");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            
            var expires = DateTime.UtcNow.AddHours(24);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                new Claim(ClaimTypes.Name, usuario.Nome),
                new Claim(ClaimTypes.Email, usuario.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"] ?? "MetaFlowAPI",
                audience: _configuration["Jwt:Audience"] ?? "MetaFlowClient",
                claims: claims,
                expires: expires,
                signingCredentials: creds);

            return new LoginResponse
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                TokenType = "Bearer",
                ExpiresAt = expires,
                UsuarioId = usuario.Id,
                Nome = usuario.Nome,
                Email = usuario.Email,
                ExpiresInHours = 24,
                Message = "Login realizado com sucesso"
            };
        }

        public string HashPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Senha não pode ser vazia", nameof(password));

            return BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);
        }

        public bool VerifyPassword(string password, string passwordHash)
        {
            if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(passwordHash))
                return false;

            return BCrypt.Net.BCrypt.Verify(password, passwordHash);
        }
    }
}