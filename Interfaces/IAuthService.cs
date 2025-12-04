 using EmployeeManagement.Api.DTOs;
namespace EmployeeManagement.Api.Interfaces
{
    public interface IAuthService
    {
        Task<string> Login(LoginDto dto);
        Task<string> Register(RegisterDto dto);
        Task<PagedResult<UserDto>> GetUsersPaged(int page, int pageSize, string? search);
        Task<UserDto> CreateUser(CreateUserDto dto); // ADD THIS
    }
}
