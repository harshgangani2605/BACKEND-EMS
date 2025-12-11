using EmployeeManagement.Api.DTOs;
using System.Security.Claims;

namespace EmployeeManagement.Api.Interfaces
{
    public interface IEmployeeService
    {
        Task<List<EmployeeDto>> GetAll();
        Task<EmployeeDto?> GetById(long id);

        Task<EmployeeDto> Create(CreateEmployeeDto dto, string username, ClaimsPrincipal user);
        Task<bool> Update(long id, UpdateEmployeeDto dto);
        Task<bool> Delete(long id);
        
    }
}
