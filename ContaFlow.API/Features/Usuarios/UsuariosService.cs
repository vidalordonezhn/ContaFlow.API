using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BCrypt.Net;
using ContaFlow.API.Data;
using ContaFlow.API.Entities;
using ContaFlow.API.Features.Usuarios.DTOs;
using Microsoft.EntityFrameworkCore;

namespace ContaFlow.API.Features.Usuarios
{
    public class UsuariosService
    {
        private readonly ContaFlowDbContext _context;

        public UsuariosService(ContaFlowDbContext context)
        {
            _context = context;
        }

        public async Task<List<UsuarioResponseDto>> GetUsuariosAsync()
        {
            return await _context.Usuarios
                .OrderBy(u => u.Nombre)
                .Select(u => new UsuarioResponseDto
                {
                    Id = u.Id,
                    Username = u.Username,
                    Nombre = u.Nombre,
                    Email = u.Email,
                    Telefono = u.Telefono,
                    Rol = u.Rol,
                    Activo = u.Activo,
                    UltimoAcceso = u.UltimoAcceso,
                    FechaCreacion = u.FechaCreacion
                })
                .ToListAsync();
        }

        public async Task<UsuarioResponseDto> RegistrarUsuarioAsync(UsuarioCreateDto dto)
        {
            var usernameNormalizado = dto.Username.Trim().ToLower();

            if (await _context.Usuarios.AnyAsync(u => u.Username.ToLower() == usernameNormalizado))
            {
                throw new InvalidOperationException($"El nombre de usuario '{dto.Username}' ya está registrado.");
            }

            var nuevoUsuario = new Usuario
            {
                Username = dto.Username.Trim(),
                Nombre = dto.Nombre.Trim(),
                Email = dto.Email?.Trim(),
                Telefono = dto.Telefono?.Trim(),
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Rol = dto.Rol,
                Activo = true
            };

            await _context.Usuarios.AddAsync(nuevoUsuario);
            await _context.SaveChangesAsync();

            return new UsuarioResponseDto
            {
                Id = nuevoUsuario.Id,
                Username = nuevoUsuario.Username,
                Nombre = nuevoUsuario.Nombre,
                Email = nuevoUsuario.Email,
                Telefono = nuevoUsuario.Telefono,
                Rol = nuevoUsuario.Rol,
                Activo = nuevoUsuario.Activo,
                UltimoAcceso = nuevoUsuario.UltimoAcceso,
                FechaCreacion = nuevoUsuario.FechaCreacion
            };
        }

        public async Task<UsuarioResponseDto> ActualizarUsuarioAsync(int id, UsuarioUpdateDto dto)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null)
            {
                throw new KeyNotFoundException("Usuario no encontrado.");
            }

            usuario.Nombre = dto.Nombre.Trim();
            usuario.Email = dto.Email?.Trim();
            usuario.Telefono = dto.Telefono?.Trim();
            usuario.Rol = dto.Rol;
            usuario.Activo = dto.Activo;

            await _context.SaveChangesAsync();

            return new UsuarioResponseDto
            {
                Id = usuario.Id,
                Username = usuario.Username,
                Nombre = usuario.Nombre,
                Email = usuario.Email,
                Telefono = usuario.Telefono,
                Rol = usuario.Rol,
                Activo = usuario.Activo,
                UltimoAcceso = usuario.UltimoAcceso,
                FechaCreacion = usuario.FechaCreacion
            };
        }

        public async Task CambiarPasswordAsync(string username, string newPassword)
        {
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Username.ToLower() == username.Trim().ToLower());

            if (usuario == null)
            {
                throw new InvalidOperationException("El usuario especificado no existe.");
            }

            usuario.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            await _context.SaveChangesAsync();
        }

        public async Task ToggleStatusAsync(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null)
            {
                throw new KeyNotFoundException("Usuario no encontrado.");
            }

            usuario.Activo = !usuario.Activo;
            await _context.SaveChangesAsync();
        }
    }
}
