using EmployeeManagement.Api.Data;
using EmployeeManagement.Api.DTOs;
using EmployeeManagement.Api.Entities;
using EmployeeManagement.Api.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagement.Api.Services
{
    public class SkillService : ISkillService
    {
        private readonly AppDbContext _context;

        public SkillService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<SkillDto>> GetAll()
        {
            return await _context.Skills
                .Select(x => new SkillDto
                {
                    Id = x.Id,
                    Name = x.Name
                })
                .ToListAsync();
        }


        public async Task<SkillDto?> GetById(long id)
        {
            var skill = await _context.Skills.FindAsync(id);
            if (skill == null) return null;

            return new SkillDto
            {
                Id = skill.Id,
                Name = skill.Name
            };
        }

        public async Task<SkillDto> Create(CreateSkillDto dto)
        {
            bool nameExists = await _context.Skills
                    .AnyAsync(x => x.Name == dto.Name);

            if (nameExists)
                throw new Exception("Skill name already exists");
            var skill = new Skill
            {
                Name = dto.Name,
                CreatedBy = "system"
            };

            _context.Skills.Add(skill);
            await _context.SaveChangesAsync();

            return new SkillDto
            {
                Id = skill.Id,
                Name = skill.Name
            };
        }

        public async Task<bool> Update(long id, CreateSkillDto dto)
        {
            var skill = await _context.Skills.FindAsync(id);
            if (skill == null) return false;

            skill.Name = dto.Name;
            skill.LastModifiedAt = DateTimeOffset.UtcNow;

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> Delete(long id)
        {
            var skill = await _context.Skills.FindAsync(id);
            if (skill == null) return false;

            _context.Skills.Remove(skill);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}
