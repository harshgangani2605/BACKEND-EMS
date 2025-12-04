using EmployeeManagement.Api.Data;
using EmployeeManagement.Api.Entities;
using EmployeeManagement.Api.DTOs;
using Microsoft.EntityFrameworkCore;
using EmployeeManagement.Api.Interfaces;

namespace EmployeeManagement.Api.Services
{
    public class DepartmentService : IDepartmentService
    {
        private readonly AppDbContext _context;

        public DepartmentService(AppDbContext context)
        {
            _context = context;
        }

        // ------------------------------------------------------
        // PAGINATION + SEARCH (NEW - REQUIRED)
        // ------------------------------------------------------
        public async Task<Interfaces.PagedResult<DepartmentDto>> GetPaged(int page, int pageSize, string? search)
        {
            var query = _context.Departments.AsQueryable();

            // SEARCH
            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.ToLower();
                query = query.Where(d => d.Name.ToLower().Contains(s));
            }

            // TOTAL COUNT
            var totalItems = await query.CountAsync();

            // PAGE ITEMS
            var items = await query
                .OrderBy(d => d.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(d => new DepartmentDto
                {
                    Id = d.Id,
                    Name = d.Name
                })
                .ToListAsync();

            return new Interfaces.PagedResult<DepartmentDto>
            {
                Page = page,
                PageSize = pageSize,
                TotalItems = totalItems,
                TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize),
                Items = items
            };
        }

        // ------------------------------------------------------
        // GET ALL (Still allowed for dropdowns)
        // ------------------------------------------------------
        public async Task<List<DepartmentDto>> GetAll()
        {
            return await _context.Departments
                .Select(x => new DepartmentDto
                {
                    Id = x.Id,
                    Name = x.Name
                })
                .ToListAsync();
        }

        // ------------------------------------------------------
        // GET BY ID
        // ------------------------------------------------------
        public async Task<DepartmentDto?> GetById(long id)
        {
            var dept = await _context.Departments.FindAsync(id);
            if (dept == null) return null;

            return new DepartmentDto
            {
                Id = dept.Id,
                Name = dept.Name
            };
        }

        // ------------------------------------------------------
        // CREATE
        // ------------------------------------------------------
        public async Task<DepartmentDto> Create(CreateDepartmentDto dto)
        {
            bool nameExists = await _context.Departments
                .AnyAsync(x => x.Name == dto.Name);

            if (nameExists)
                throw new Exception("Department name already exists");

            var dept = new Department
            {
                Name = dto.Name,
                CreatedBy = "system"
            };

            _context.Departments.Add(dept);
            await _context.SaveChangesAsync();

            return new DepartmentDto
            {
                Id = dept.Id,
                Name = dept.Name
            };
        }

        // ------------------------------------------------------
        // UPDATE
        // ------------------------------------------------------
        public async Task<bool> Update(long id, CreateDepartmentDto dto)
        {
            var dept = await _context.Departments.FindAsync(id);
            if (dept == null) return false;

            dept.Name = dto.Name;
            dept.LastModifiedAt = DateTimeOffset.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        // ------------------------------------------------------
        // DELETE
        // ------------------------------------------------------
        public async Task<bool> Delete(long id)
        {
            var dept = await _context.Departments.FindAsync(id);
            if (dept == null) return false;

            _context.Departments.Remove(dept);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}
