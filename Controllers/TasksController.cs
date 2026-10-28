using System;
using System.Threading.Tasks;
using CampusDeadline.Extensions;
using CampusDeadline.Models.ViewModels;
using CampusDeadline.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CampusDeadline.Controllers
{
    [Authorize]
    public class TasksController : Controller
    {
        private readonly ITaskService _taskService;

        public TasksController(ITaskService taskService)
        {
            _taskService = taskService;
        }

        private bool IsAjaxRequest()
        {
            return Request.Headers["X-Requested-With"] == "XMLHttpRequest" ||
                   Request.Headers["Accept"].ToString().Contains("application/json") ||
                   Request.ContentType?.Contains("application/json") == true;
        }

        [HttpGet]
        [Route("tasks")]
        public async Task<IActionResult> Index(
            string? search = null,
            [FromQuery(Name = "subject_id")] int? subjectId = null,
            [FromQuery(Name = "task_type")] string? taskType = null,
            string? priority = null,
            string? status = null,
            string? filter = null,
            [FromQuery(Name = "sort_by")] string sortBy = "deadline_asc",
            string? format = null)
        {
            int userId = User.GetUserId();

            var filterParams = new TaskFilterParams
            {
                Search = search,
                SubjectId = subjectId,
                TaskType = taskType,
                Priority = priority,
                Status = status,
                Filter = filter,
                SortBy = sortBy
            };

            var tasks = await _taskService.GetUserTasksAsync(userId, filterParams, sortBy, search);
            var subjects = await _taskService.GetUserSubjectsAsync(userId);
            var stats = await _taskService.GetDashboardStatsAsync(userId);

            if (IsAjaxRequest() && format == "json")
            {
                return Json(new { success = true, tasks, count = tasks.Count });
            }

            var viewModel = new TasksListViewModel
            {
                Tasks = tasks,
                Subjects = subjects,
                Stats = stats,
                CurrentFilters = filterParams
            };

            ViewBag.GlobalSubjects = subjects;
            ViewBag.ActiveNav = "tasks";

            return View(viewModel);
        }

        [HttpPost]
        [Route("tasks/create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TaskFormViewModel model)
        {
            int userId = User.GetUserId();

            var (taskId, error) = await _taskService.CreateTaskAsync(userId, model);

            if (!string.IsNullOrEmpty(error))
            {
                if (IsAjaxRequest())
                    return BadRequest(new { success = false, error });

                TempData["Error"] = error;
                return RedirectToAction(nameof(Index));
            }

            if (IsAjaxRequest())
            {
                var createdTask = await _taskService.GetTaskByIdAsync(taskId, userId);
                return Json(new { success = true, message = "Academic task created successfully.", task = createdTask });
            }

            TempData["Success"] = "Task created successfully!";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        [Route("tasks/{id:int}")]
        public async Task<IActionResult> Details(int id)
        {
            int userId = User.GetUserId();
            var task = await _taskService.GetTaskByIdAsync(id, userId);

            if (task == null)
            {
                return NotFound(new { success = false, error = "Task not found." });
            }

            return Json(new { success = true, task });
        }

        [HttpPost]
        [Route("tasks/{id:int}/edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, TaskFormViewModel model)
        {
            int userId = User.GetUserId();

            var (success, error) = await _taskService.UpdateTaskAsync(id, userId, model);

            if (!success)
            {
                if (IsAjaxRequest())
                    return BadRequest(new { success = false, error });

                TempData["Error"] = error ?? "Failed to update task.";
                return RedirectToAction(nameof(Index));
            }

            if (IsAjaxRequest())
            {
                var updated = await _taskService.GetTaskByIdAsync(id, userId);
                return Json(new { success = true, message = "Task updated successfully.", task = updated });
            }

            TempData["Success"] = "Task updated successfully!";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [Route("tasks/{id:int}/delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            int userId = User.GetUserId();
            var (success, error) = await _taskService.DeleteTaskAsync(id, userId);

            if (!success)
            {
                if (IsAjaxRequest())
                    return BadRequest(new { success = false, error });

                TempData["Error"] = error ?? "Failed to delete task.";
                return RedirectToAction(nameof(Index));
            }

            if (IsAjaxRequest())
            {
                return Json(new { success = true, message = "Task deleted successfully." });
            }

            TempData["Info"] = "Task deleted successfully.";
            return RedirectToAction(nameof(Index));
        }

        public class StatusUpdateModel
        {
            [System.Text.Json.Serialization.JsonPropertyName("status")]
            public string Status { get; set; } = string.Empty;
        }

        [HttpPost]
        [Route("tasks/{id:int}/status")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Status(int id, [FromBody] StatusUpdateModel? jsonModel)
        {
            int userId = User.GetUserId();
            string status = jsonModel?.Status ?? "";

            if (string.IsNullOrWhiteSpace(status) && Request.HasFormContentType)
            {
                status = Request.Form["status"].ToString();
            }

            var (success, error) = await _taskService.UpdateTaskStatusAsync(id, userId, status);

            if (!success)
            {
                if (IsAjaxRequest())
                    return BadRequest(new { success = false, error });

                TempData["Error"] = error ?? "Failed to update status.";
                return RedirectToAction(nameof(Index));
            }

            var task = await _taskService.GetTaskByIdAsync(id, userId);
            var stats = await _taskService.GetDashboardStatsAsync(userId);

            if (IsAjaxRequest())
            {
                return Json(new
                {
                    success = true,
                    message = $"Task marked as {status.Replace('_', ' ')}.",
                    task,
                    stats
                });
            }

            TempData["Success"] = $"Task marked as {status.Replace('_', ' ')}.";
            return Redirect(Request.Headers["Referer"].ToString() ?? Url.Action(nameof(Index))!);
        }
    }
}
