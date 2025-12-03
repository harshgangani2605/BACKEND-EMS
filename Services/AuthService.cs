using EmployeeManagement.Api.DTOs;
using EmployeeManagement.Api.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagement.Api.Services
{
    public class AuthService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<AppRole> _roleManager;
        private readonly JwtService _jwtService;

        public AuthService(
            UserManager<AppUser> userManager,
            RoleManager<AppRole> roleManager,
            JwtService jwtService)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _jwtService = jwtService;
        }

        // ================================================================
        // REGISTER USER (for public register)
        // ================================================================
        public async Task<string?> Register(RegisterDto dto)
        {
            var exists = await _userManager.FindByEmailAsync(dto.Email);
            if (exists != null) return "User already exists";

            var user = new AppUser
            {
                FullName = dto.FullName,
                Email = dto.Email,
                UserName = dto.Email
            };

            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
                return "User creation failed";

            // Ensure role exists
            if (!await _roleManager.RoleExistsAsync("User"))
                await _roleManager.CreateAsync(new AppRole { Name = "User" });

            await _userManager.AddToRoleAsync(user, "User");

            return null;
        }

        // ================================================================
        // LOGIN
        // ================================================================
        public async Task<string?> Login(LoginDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null) return null;

            bool valid = await _userManager.CheckPasswordAsync(user, dto.Password);
            if (!valid) return null;

            var roles = await _userManager.GetRolesAsync(user);
            return _jwtService.GenerateToken(user, roles);
        }
        // CREATE USER (Admin Only)
        public async Task<UserDto?> CreateUser(CreateUserDto dto)
        {
            var exists = await _userManager.FindByEmailAsync(dto.Email);
            if (exists != null) return null;

            var user = new AppUser
            {
                FullName = dto.FullName,
                Email = dto.Email,
                UserName = dto.Email
            };

            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded) return null;

            if (!await _roleManager.RoleExistsAsync(dto.Role))
                await _roleManager.CreateAsync(new AppRole { Name = dto.Role });

            await _userManager.AddToRoleAsync(user, dto.Role);

            return new UserDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email!,
                Roles = new List<string> { dto.Role }
            };
        }
    }
}
