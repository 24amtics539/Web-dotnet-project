using System.Collections.Generic;
using System.Threading.Tasks;
using CampusDeadline.Models.ViewModels;

namespace CampusDeadline.Services
{
    public interface ITaskService
    {
        Task<DashboardStatsDto> GetDashboardStatsAsync(int userId);
        Task<HeatmapDto> GetActivityHeatmapAsync(int userId, int weeks = 12);
        Task<List<TaskDto>> GetUserTasksAsync(int userId, TaskFilterParams? filters = null, string sortBy = "deadline_asc", string? search = null);
        Task<List<TaskDto>> GetUpcomingExamsAsync(int userId, int limit = 4);
        Task<TaskDto?> GetTaskByIdAsync(int taskId, int userId);
        Task<(int TaskId, string? Error)> CreateTaskAsync(int userId, TaskFormViewModel model);
        Task<(bool Success, string? Error)> UpdateTaskAsync(int taskId, int userId, TaskFormViewModel model);
        Task<(bool Success, string? Error)> UpdateTaskStatusAsync(int taskId, int userId, string newStatus);
        Task<(bool Success, string? Error)> DeleteTaskAsync(int taskId, int userId);

        Task<List<SubjectDto>> GetUserSubjectsAsync(int userId, string? search = null, string? semester = null);
        Task<SubjectDto?> GetSubjectByIdAsync(int subjectId, int userId);
        Task<(int SubjectId, string? Error)> CreateSubjectAsync(int userId, SubjectFormViewModel model);
        Task<(bool Success, string? Error)> UpdateSubjectAsync(int subjectId, int userId, SubjectFormViewModel model);
        Task<(bool Success, string? Error)> DeleteSubjectAsync(int subjectId, int userId);
    }
}
