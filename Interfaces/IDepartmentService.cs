using EmployeeManagement.Api.DTOs;
namespace EmployeeManagement.Api.Interfaces
{
    public interface IDepartmentService
    {
        Task<List<DepartmentDto>> GetAll();
        Task<DepartmentDto?> GetById(long id);
        Task<DepartmentDto> Create(CreateDepartmentDto dto);
        Task<bool> Update(long id, CreateDepartmentDto dto);
        Task<bool> Delete(long id);
    }
}

