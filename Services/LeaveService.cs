using EmployeeManagement.Api.Data;
using EmployeeManagement.Api.DTOs;
using EmployeeManagement.Api.Entities;
using EmployeeManagement.Api.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagement.Api.Services
{
    public class LeaveService : ILeaveService
    {
        private readonly AppDbContext _context;

        public LeaveService(AppDbContext context)
        {
            _context = context;
        }

        // =========================
        // APPLY LEAVE (ALL ROLES)
        // =========================
        public async Task ApplyLeaveAsync(ApplyLeaveDto dto, string uc)
        {
            var user = await _context.AppUsers
                .FirstOrDefaultAsync(u => u.FullName == uc);

            if (user == null)
                throw new Exception("User not found");

            var leave = new LeaveRequest
            {
                UserId = user.Id,
                UserName = user.FullName,
                LeaveType = dto.LeaveType,
                FromDate = dto.FromDate,
                ToDate = dto.ToDate,
                Reason = dto.Reason,
                Status = "Pending"
            };

            _context.LeaveRequests.Add(leave);
            await _context.SaveChangesAsync();
        }

        // =========================
        // USER: SEE OWN LEAVES
        // =========================
        public async Task<List<LeaveListDto>> GetMyLeavesAsync(long userId)
        {
            return await _context.LeaveRequests
                .Where(l => l.UserId == userId)
                .OrderByDescending(l => l.CreatedAt)
                .Select(l => new LeaveListDto
                {
                    Id = l.Id,
                    UserName = l.UserName,
                    LeaveType = l.LeaveType,
                    FromDate = l.FromDate,
                    ToDate = l.ToDate,
                    Status = l.Status
                })
                .ToListAsync();
        }

        // =========================
        // ADMIN / HR: SEE ALL
        // =========================
        public async Task<List<LeaveListDto>> GetAllLeavesAsync()
        {
            return await _context.LeaveRequests
                .OrderByDescending(l => l.CreatedAt)
                .Select(l => new LeaveListDto
                {
                    Id = l.Id,
                    UserName = l.UserName,
                    LeaveType = l.LeaveType,
                    FromDate = l.FromDate,
                    ToDate = l.ToDate,
                    Status = l.Status
                })
                .ToListAsync();
        }

        // =========================
        // 🔥 GET LEAVE BY ID
        // =========================
        public async Task<LeaveListDto?> GetByIdAsync(long id)
        {
            return await _context.LeaveRequests
                .Where(l => l.Id == id)
                .Select(l => new LeaveListDto
                {
                    Id = l.Id,
                    UserName = l.UserName,
                    LeaveType = l.LeaveType,
                    FromDate = l.FromDate,
                    ToDate = l.ToDate,
                    Status = l.Status,
                    Reason = l.Reason
                })
                .FirstOrDefaultAsync();
        }

        // =========================
        // 🔥 APPROVE / REJECT
        // =========================
        public async Task UpdateStatusAsync(long id, string status, string actionBy)
        {
            var leave = await _context.LeaveRequests.FindAsync(id);

            if (leave == null)
                throw new Exception("Leave not found");

            if (leave.Status != "Pending")
                throw new Exception("Leave already processed");

            leave.Status = status;
            await _context.SaveChangesAsync();
        }

        // =========================
        // 🔥 GET PENDING LEAVES
        // =========================
        public async Task<List<LeaveListDto>> GetPendingLeavesAsync()
        {
            return await _context.LeaveRequests
                .Where(l => l.Status == "Pending")
                .OrderBy(l => l.FromDate)
                .Select(l => new LeaveListDto
                {
                    Id = l.Id,
                    UserName = l.UserName,
                    LeaveType = l.LeaveType,
                    FromDate = l.FromDate,
                    ToDate = l.ToDate,
                    Status = l.Status
                })
                .ToListAsync();
        }

        // =========================
        // 🔥 DELETE LEAVE (OWN + PENDING)
        // =========================
        public async Task DeleteLeaveAsync(long id, long userId)
        {
            var leave = await _context.LeaveRequests
                .FirstOrDefaultAsync(l => l.Id == id && l.UserId == userId);

            if (leave == null)
                throw new Exception("Leave not found");

            if (leave.Status != "Pending")
                throw new Exception("Cannot delete approved/rejected leave");

            _context.LeaveRequests.Remove(leave);
            await _context.SaveChangesAsync();
        }
        public async Task UpdateLeaveAsync(long id, long userId, UpdateLeaveDto dto)
        {
            var leave = await _context.LeaveRequests
                .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);

            if (leave == null)
                throw new Exception("Leave not found");

            // ❗ Important rule
            if (leave.Status != "Pending")
                throw new Exception("Only pending leave can be edited");

            // Update fields
            leave.LeaveType = dto.LeaveType;
            leave.FromDate = dto.FromDate;
            leave.ToDate = dto.ToDate;
            leave.Reason = dto.Reason;

            await _context.SaveChangesAsync();
        }

    }
}
