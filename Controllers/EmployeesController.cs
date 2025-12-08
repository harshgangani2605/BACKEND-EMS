using EmployeeManagement.Api.Attributes;
using EmployeeManagement.Api.Data;
using EmployeeManagement.Api.DTOs;
using EmployeeManagement.Api.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagement.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmployeesController : ControllerBase
    {
        private readonly IEmployeeService _service;
        private readonly AppDbContext _context;

        public EmployeesController(IEmployeeService service,AppDbContext context)
        {
            _context = context;
            _service = service;
        }
        [Authorize]
        [RequirePermission("employee.view")]
        [HttpGet]
        public async Task<IActionResult> GetAll(int page = 1,
        int pageSize = 10,
        string? search = null)

        {
            var query = _context.Employees
              .Include(e => e.Department)
              .Include(e => e.EmployeeSkills)
                  .ThenInclude(es => es.Skill)
              .AsQueryable();

            if(!IsAdmin())
            {
                var currentUser = GetCurrentUsername();
                if (currentUser != null)
                {
                    query = query.Where(e => e.CreatedBy == currentUser);
                }
                else { 
                    return Forbid();
                }
            }

            // SEARCH
            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.ToLower();
                query = query.Where(e =>
                    e.FullName.ToLower().Contains(s) ||
                    e.Email.ToLower().Contains(s));
            }

            // COUNT
            var totalItems = await query.CountAsync();

            // PAGINATION
            var items = await query
                .OrderBy(e => e.FullName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(e => new EmployeeDto
                {
                    Id = e.Id,
                    FullName = e.FullName,
                    Email = e.Email,
                    Salary = e.Salary,
                    JoinedOn = e.JoinedOn,
                    DepartmentId = e.DepartmentId,
                    DepartmentName = e.Department.Name,
                    SkillIds = e.EmployeeSkills.Select(x => x.SkillId).ToList(),
                    Skills = e.EmployeeSkills.Select(x => x.Skill.Name).ToList()
                })
                .ToListAsync();
            return Ok(
                new {
                    Page = page,
                    PageSize = pageSize,
                    TotalItems = totalItems,
                    Items = items
                }
                );
        }
        [Authorize]
        [RequirePermission("employee.view")]
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(long id)
        {
            var emp = await _service.GetById(id);
            if (emp == null) return NotFound();

            return Ok(emp);
        }
        [Authorize]
        [RequirePermission("employee.create")]
        [HttpPost]
        public async Task<IActionResult> Create(CreateEmployeeDto dto)
        {
            var emp = await _service.Create(dto);
            return Ok(emp);
        }
        [Authorize]
        [RequirePermission("employee.edit")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(long id, UpdateEmployeeDto dto)
        {
            var updated = await _service.Update(id, dto);
            if (!updated) return NotFound();

            return Ok(new {message = "updated"});
        }
        [Authorize]
        [RequirePermission("employee.delete")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(long id)
        {
            var deleted = await _service.Delete(id);
            if (!deleted) return NotFound();

            return Ok(new { message = "deleted" });
        }
        private string? GetCurrentUsername()
        {
            if (User?.Identity?.IsAuthenticated != true) return null;

            if (!string.IsNullOrEmpty(User.Identity?.Name))
                return User.Identity.Name;

            // common claim names
            return User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value
                   ?? User.FindFirst("name")?.Value
                   ?? User.FindFirst("preferred_username")?.Value
                   ?? User.FindFirst("username")?.Value;
        }

        private bool IsAdmin()
        {
            if (User == null) return false;

            // common role checks
            if (User.IsInRole("Admin")) return true;

            var roles = User.FindAll(System.Security.Claims.ClaimTypes.Role).Select(c => c.Value)
                        .Concat(User.FindAll("role").Select(c => c.Value));

            return roles.Any(r => string.Equals(r, "Admin", StringComparison.OrdinalIgnoreCase));
        }

    }
}
