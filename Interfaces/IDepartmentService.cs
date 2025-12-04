using EmployeeManagement.Api.DTOs;
namespace EmployeeManagement.Api.Interfaces
{
    public interface IDepartmentService
    {
        Task<List<DepartmentDto>> GetAll();
        Task<PagedResult<DepartmentDto>> GetPaged(int page, int pageSize, string? search);

        Task<DepartmentDto?> GetById(long id);
        Task<DepartmentDto> Create(CreateDepartmentDto dto);
        Task<bool> Update(long id, CreateDepartmentDto dto);
        Task<bool> Delete(long id);
    }
}

