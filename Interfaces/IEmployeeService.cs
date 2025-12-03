using EmployeeManagement.Api.DTOs;

namespace EmployeeManagement.Api.Interfaces
{
    public interface IEmployeeService
    {
        Task<List<EmployeeDto>> GetAll();
        Task<EmployeeDto?> GetById(long id);
        Task<EmployeeDto> Create(CreateEmployeeDto dto);
        Task<bool> Update(long id, UpdateEmployeeDto dto);
        Task<bool> Delete(long id);
    }
}
