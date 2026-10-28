using System;
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
    public class CalendarController : Controller
    {
        private readonly ITaskService _taskService;

        public CalendarController(ITaskService taskService)
        {
            _taskService = taskService;
        }

        [HttpGet]
        [Route("calendar")]
        public async Task<IActionResult> Index()
        {
            int userId = User.GetUserId();
            var subjects = await _taskService.GetUserSubjectsAsync(userId);

            ViewBag.GlobalSubjects = subjects;
            ViewBag.ActiveNav = "calendar";

            return View(subjects);
        }

        [HttpGet]
        [Route("api/calendar/events")]
        public async Task<IActionResult> GetEvents(
            [FromQuery(Name = "subject_id")] int? subjectId = null,
            string? status = null)
        {
            int userId = User.GetUserId();

            var filters = new TaskFilterParams
            {
                SubjectId = subjectId,
                Status = status
            };

            var tasks = await _taskService.GetUserTasksAsync(userId, filters, "deadline_asc");

            var events = tasks.Select(t =>
            {
                string borderColor = t.SubjectColor;
                string bgColor = t.SubjectColor;

                if (t.Status == "completed")
                {
                    bgColor = "#10B981";
                    borderColor = "#059669";
                }
                else if (t.IsOverdue)
                {
                    borderColor = "#EF4444";
                }

                string typePrefix = t.TaskType.Length >= 4 ? t.TaskType[..4] : t.TaskType;

                return new
                {
                    id = t.Id.ToString(),
                    title = $"[{typePrefix}] {t.Title}",
                    start = t.DueDate.ToString("yyyy-MM-ddTHH:mm:ss"),
                    backgroundColor = bgColor,
                    borderColor = borderColor,
                    textColor = "#FFFFFF",
                    extendedProps = new
                    {
                        id = t.Id,
                        raw_title = t.Title,
                        task_type = t.TaskType,
                        priority = t.Priority,
                        status = t.Status,
                        subject_id = t.SubjectId,
                        subject_name = t.SubjectName,
                        subject_code = t.SubjectCode,
                        subject_color = t.SubjectColor,
                        is_overdue = t.IsOverdue,
                        countdown = t.Countdown,
                        due_date_formatted = t.DueDateFormatted,
                        due_date_raw = t.DueDateRaw,
                        due_time_raw = t.DueTimeRaw,
                        description = t.Description ?? string.Empty
                    }
                };
            }).ToList();

            return Json(events);
        }
    }
}
