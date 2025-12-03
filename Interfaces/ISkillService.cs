using EmployeeManagement.Api.DTOs;

namespace EmployeeManagement.Api.Interfaces
{
    public interface ISkillService
    {
        Task<List<SkillDto>> GetAll();
        Task<SkillDto?> GetById(long id);
        Task<SkillDto> Create(CreateSkillDto dto);
        Task<bool> Update(long id, CreateSkillDto dto);
        Task<bool> Delete(long id);
    }
}
