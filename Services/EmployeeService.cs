using EmployeeManagement.Api.Data;
using EmployeeManagement.Api.DTOs;
using EmployeeManagement.Api.Entities;
using EmployeeManagement.Api.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagement.Api.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly AppDbContext _context;

        public EmployeeService(AppDbContext context)
        {
            _context = context;
        }

        // ---------------------- GET ALL ----------------------
        public async Task<List<EmployeeDto>> GetAll()
        {
            return await _context.Employees
                .Include(e => e.Department)
                .Include(e => e.EmployeeSkills)
                    .ThenInclude(es => es.Skill)
                .Select(e => new EmployeeDto
                {
                    Id = e.Id,
                    FullName = e.FullName,
                    Email = e.Email,
                    Salary = e.Salary,
                    JoinedOn = e.JoinedOn,

                    DepartmentId = e.DepartmentId,       // ✔ FIXED
                    DepartmentName = e.Department.Name,

                    SkillIds = e.EmployeeSkills.Select(s => s.SkillId).ToList(),
                    Skills = e.EmployeeSkills.Select(s => s.Skill.Name).ToList()
                })
                .ToListAsync();
        }

        // ---------------------- GET BY ID ----------------------
        public async Task<EmployeeDto?> GetById(long id)
        {
            var e = await _context.Employees
                .Include(x => x.Department)
                .Include(x => x.EmployeeSkills)
                    .ThenInclude(es => es.Skill)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (e == null) return null;

            return new EmployeeDto
            {
                Id = e.Id,
                FullName = e.FullName,
                Email = e.Email,
                Salary = e.Salary,
                JoinedOn = e.JoinedOn,

                DepartmentId = e.DepartmentId,   // ✔ FIXED
                DepartmentName = e.Department.Name,

                SkillIds = e.EmployeeSkills.Select(x => x.SkillId).ToList(),   // ✔ FIXED
                Skills = e.EmployeeSkills.Select(x => x.Skill.Name).ToList()
            };
        }

        // ---------------------- CREATE ----------------------
        public async Task<EmployeeDto> Create(CreateEmployeeDto dto)
        {
            var emp = new Employee
            {
                FullName = dto.FullName,
                Email = dto.Email,
                Salary = dto.Salary,
                JoinedOn = dto.JoinedOn,
                DepartmentId = dto.DepartmentId,
                CreatedBy = "system"
            };

            _context.Employees.Add(emp);
            await _context.SaveChangesAsync();

            foreach (var skillId in dto.SkillIds)
            {
                _context.EmployeeSkills.Add(new EmployeeSkill
                {
                    EmployeeId = emp.Id,
                    SkillId = skillId
                });
            }

            await _context.SaveChangesAsync();

            return await GetById(emp.Id);
        }

        // ---------------------- UPDATE ----------------------
        public async Task<bool> Update(long id, UpdateEmployeeDto dto)
        {
            var emp = await _context.Employees
                .Include(x => x.EmployeeSkills)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (emp == null) return false;

            emp.FullName = dto.FullName;
            emp.Email = dto.Email;
            emp.Salary = dto.Salary;
            emp.JoinedOn = dto.JoinedOn;
            emp.DepartmentId = dto.DepartmentId;
            emp.LastModifiedAt = DateTimeOffset.UtcNow;

            _context.EmployeeSkills.RemoveRange(emp.EmployeeSkills);

            foreach (var skillId in dto.SkillIds)
            {
                _context.EmployeeSkills.Add(new EmployeeSkill
                {
                    EmployeeId = emp.Id,
                    SkillId = skillId
                });
            }

            await _context.SaveChangesAsync();
            return true;
        }

        // ---------------------- DELETE ----------------------
        public async Task<bool> Delete(long id)
        {
            var emp = await _context.Employees
                .Include(x => x.EmployeeSkills)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (emp == null) return false;

            _context.EmployeeSkills.RemoveRange(emp.EmployeeSkills);
            _context.Employees.Remove(emp);

            await _context.SaveChangesAsync();

            return true;
        }
    }
}
