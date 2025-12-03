using EmployeeManagement.Api.Attributes;
using EmployeeManagement.Api.DTOs;
using EmployeeManagement.Api.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeManagement.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] 
    public class AdminController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<AppRole> _roleManager;

        public AdminController(UserManager<AppUser> userManager, RoleManager<AppRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [RequirePermission("user.view")]
        [HttpGet("users")]
        public async Task<IActionResult> GetUsers()
        {
            var users = _userManager.Users.ToList();
            var result = new List<UserDto>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);

                result.Add(new UserDto
                {
                    Id = user.Id,
                    Email = user.Email!,
                    FullName = user.FullName ?? "",
                    Roles = roles.ToList()
                });
            }

            return Ok(result);
        }
        [RequirePermission("user.view")]
        [HttpGet("user")]
        public async Task<IActionResult> GetUser([FromQuery] string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return NotFound("User not found");

            var roles = await _userManager.GetRolesAsync(user);

            var result = new UserDto
            {
                Id = user.Id,
                Email = user.Email!,
                FullName = user.FullName ?? "",
                Roles = roles.ToList()
            };

            return Ok(result);
        }

        [RequirePermission("user.create")]
        [HttpPost("create-user")]
        public async Task<IActionResult> CreateUser(RegisterDto dto)
        {
            var exists = await _userManager.FindByEmailAsync(dto.Email);
            if (exists != null)
                return BadRequest("User already exists");

            var user = new AppUser
            {
                FullName = dto.FullName,
                Email = dto.Email,
                UserName = dto.Email
            };

            var result = await _userManager.CreateAsync(user, dto.Password);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            // Assign role from dto OR default User role
            var role = string.IsNullOrWhiteSpace(dto.Role) ? "User" : dto.Role;

            if (!await _roleManager.RoleExistsAsync(role))
                await _roleManager.CreateAsync(new AppRole { Name = role });

            await _userManager.AddToRoleAsync(user, role);

            return Ok(new { message = "create succesfully" });
        }
        [RequirePermission("user.edit")]
        [HttpPost("change-role")]
        public async Task<IActionResult> ChangeRole(ChangeUserRoleDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null)
                return NotFound("User not found");

            if (!await _roleManager.RoleExistsAsync(dto.Role))
                return BadRequest("Role does not exist");

            // Remove old roles
            var oldRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, oldRoles);

            // Add new role
            var result = await _userManager.AddToRoleAsync(user, dto.Role);

            if (result.Succeeded)
                return StatusCode(500, "Failed to update role");

            return Ok(new { message = "updated sucessfully" });
        }

        [RequirePermission("user.delete")]
        [HttpDelete("delete-user")]
        public async Task<IActionResult> DeleteUser([FromQuery] string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return NotFound("User not found");

            var result = await _userManager.DeleteAsync(user);

            if (!result.Succeeded)
                return StatusCode(500, "Delete failed");

            return Ok(new { message = "deleted  sucessfully" });
        }
    }
}
