using EmployeeManagement.Api.DTOs;

namespace EmployeeManagement.Api.Interfaces
{
    public interface ILeaveService
    {
        Task ApplyLeaveAsync(ApplyLeaveDto dto,string user);

        Task<List<LeaveListDto>> GetMyLeavesAsync(long userId);

        Task<List<LeaveListDto>> GetAllLeavesAsync();
        Task<LeaveListDto?> GetByIdAsync(long id);
        Task UpdateStatusAsync(long id, string status, string actionBy);
        Task<List<LeaveListDto>> GetPendingLeavesAsync();
        Task DeleteLeaveAsync(long id, long userId);
        Task UpdateLeaveAsync(long id, long userId, UpdateLeaveDto dto);
    }
}
