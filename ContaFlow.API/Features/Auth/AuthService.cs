using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using BCrypt.Net;
using ContaFlow.API.Data;
using ContaFlow.API.Entities;
using ContaFlow.API.Features.Auth.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace ContaFlow.API.Features.Auth
{
    public class AuthService
    {
        private readonly ContaFlowDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthService(ContaFlowDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<LoginResponseDto> LoginAsync(LoginRequestDto request)
        {
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Username.ToLower() == request.Username.Trim().ToLower());

            if (usuario == null || !BCrypt.Net.BCrypt.Verify(request.Password, usuario.PasswordHash))
            {
                throw new UnauthorizedAccessException("Credenciales incorrectas. Verifique su usuario y contraseña.");
            }

            if (!usuario.Activo)
            {
                throw new InvalidOperationException("El usuario se encuentra inactivo en el sistema.");
            }

            usuario.UltimoAcceso = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            var token = GenerarTokenJwt(usuario);

            return new LoginResponseDto
            {
                Token = token,
                UsuarioId = usuario.Id,
                Username = usuario.Username,
                Nombre = usuario.Nombre,
                Email = usuario.Email ?? string.Empty,
                Rol = usuario.Rol
            };
        }

        private string GenerarTokenJwt(Usuario usuario)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"] ?? "ClaveSecretaPorDefectoParaContaFlow2026";
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                new Claim(ClaimTypes.Name, usuario.Username),
                new Claim(ClaimTypes.GivenName, usuario.Nombre),
                new Claim(ClaimTypes.Role, usuario.Rol)
            };

            var expirationMinutes = int.TryParse(jwtSettings["ExpirationMinutes"], out var mins) ? mins : 480;

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"] ?? "ContaFlow.API",
                audience: jwtSettings["Audience"] ?? "ContaFlow.Client",
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
