using System.Linq;
using System.Threading.Tasks;
using CampusDeadline.Extensions;
using CampusDeadline.Models.ViewModels;
using CampusDeadline.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CampusDeadline.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly ITaskService _taskService;

        public DashboardController(ITaskService taskService)
        {
            _taskService = taskService;
        }

        [HttpGet]
        [Route("")]
        [Route("dashboard")]
        public async Task<IActionResult> Index()
        {
            int userId = User.GetUserId();

            var stats = await _taskService.GetDashboardStatsAsync(userId);
            var heatmap = await _taskService.GetActivityHeatmapAsync(userId, 12);

            var pendingTasks = await _taskService.GetUserTasksAsync(userId, new TaskFilterParams { Status = "pending" }, "deadline_asc");
            var inProgressTasks = await _taskService.GetUserTasksAsync(userId, new TaskFilterParams { Status = "in_progress" }, "deadline_asc");
            var activeTasks = await _taskService.GetUserTasksAsync(userId, new TaskFilterParams { Filter = "upcoming" }, "deadline_asc");
            var upcomingExams = await _taskService.GetUpcomingExamsAsync(userId, 4);
            var subjects = await _taskService.GetUserSubjectsAsync(userId);

            var viewModel = new DashboardViewModel
            {
                Stats = stats,
                Heatmap = heatmap,
                UpcomingTasks = pendingTasks.Take(6).ToList(),
                InProgressTasks = inProgressTasks.Take(4).ToList(),
                ActiveTasks = activeTasks.Take(6).ToList(),
                UpcomingExams = upcomingExams,
                Subjects = subjects
            };

            ViewBag.GlobalSubjects = subjects;
            ViewBag.ActiveNav = "dashboard";

            return View(viewModel);
        }
    }
}
