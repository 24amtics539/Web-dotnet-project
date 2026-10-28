using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using CampusDeadline.Data;
using CampusDeadline.Models.Entities;
using CampusDeadline.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace CampusDeadline.Services
{
    public class TaskService : ITaskService
    {
        private readonly ApplicationDbContext _db;

        public TaskService(ApplicationDbContext db)
        {
            _db = db;
        }

        public (string Countdown, bool IsOverdue, string Urgency) FormatCountdown(DateTime dueDate, string status)
        {
            var now = DateTime.Now;
            var diff = dueDate - now;

            if (diff.TotalSeconds < 0)
            {
                var pastDiff = now - dueDate;
                var days = pastDiff.Days;
                var hours = pastDiff.Hours;
                var mins = Math.Max(1, pastDiff.Minutes);

                bool isOverdue = status != "completed";
                string countdown;

                if (days == 0 && hours == 0)
                    countdown = $"Overdue by {mins}m";
                else if (days == 0)
                    countdown = $"Overdue by {hours}h";
                else if (days == 1)
                    countdown = "Overdue by 1 day";
                else
                    countdown = $"Overdue by {days} days";

                return (countdown, isOverdue, "overdue");
            }
            else
            {
                var days = diff.Days;
                var hours = diff.Hours;
                var mins = Math.Max(1, diff.Minutes);

                string countdown;
                string urgency;

                if (days == 0 && hours == 0)
                {
                    countdown = $"Due in {mins}m";
                    urgency = "urgent";
                }
                else if (days == 0)
                {
                    countdown = $"Due in {hours}h";
                    urgency = "urgent";
                }
                else if (days == 1)
                {
                    countdown = "Due tomorrow";
                    urgency = "approaching";
                }
                else if (days <= 7)
                {
                    countdown = $"Due in {days} days";
                    urgency = "normal";
                }
                else
                {
                    countdown = $"Due in {days} days";
                    urgency = "distant";
                }

                return (countdown, false, urgency);
            }
        }

        public TaskDto MapToDto(TaskItem task)
        {
            var (countdown, isOverdue, urgency) = FormatCountdown(task.DueDate, task.Status);

            return new TaskDto
            {
                Id = task.Id,
                UserId = task.UserId,
                SubjectId = task.SubjectId,
                SubjectName = task.Subject?.SubjectName ?? "General",
                SubjectCode = task.Subject?.SubjectCode,
                SubjectColor = task.Subject?.Color ?? "#B98AFF",
                Title = task.Title,
                Description = task.Description,
                TaskType = task.TaskType,
                Priority = task.Priority,
                Status = task.Status,
                DueDate = task.DueDate,
                DueDateFormatted = task.DueDate.ToString("MMM dd, yyyy 'at' hh:mm tt", CultureInfo.InvariantCulture),
                DueDateShort = task.DueDate.ToString("MMM dd", CultureInfo.InvariantCulture),
                DueTimeFormatted = task.DueDate.ToString("hh:mm tt", CultureInfo.InvariantCulture),
                DueDateRaw = task.DueDate.ToString("yyyy-MM-dd"),
                DueTimeRaw = task.DueDate.ToString("HH:mm"),
                Countdown = countdown,
                IsOverdue = isOverdue,
                Urgency = urgency,
                CreatedAt = task.CreatedAt
            };
        }

        public async Task<DashboardStatsDto> GetDashboardStatsAsync(int userId)
        {
            var now = DateTime.Now;
            var sevenDaysLater = now.AddDays(7);

            var tasks = await _db.Tasks
                .Where(t => t.UserId == userId)
                .Select(t => new { t.Id, t.Status, t.DueDate })
                .ToListAsync();

            int total = tasks.Count;
            int completed = tasks.Count(t => t.Status == "completed");
            int pending = tasks.Count(t => t.Status != "completed");
            int overdue = tasks.Count(t => t.Status != "completed" && t.DueDate < now);
            int upcoming7d = tasks.Count(t => t.Status != "completed" && t.DueDate >= now && t.DueDate <= sevenDaysLater);

            double completionRate = total > 0 ? Math.Round((double)completed / total * 100, 1) : 0;

            // Find nearest incomplete deadline
            var nearestTask = await _db.Tasks
                .Include(t => t.Subject)
                .Where(t => t.UserId == userId && t.Status != "completed" && t.DueDate >= now)
                .OrderBy(t => t.DueDate)
                .FirstOrDefaultAsync();

            NearestDeadlineDto? nearestDto = null;
            if (nearestTask != null)
            {
                var (countdown, _, _) = FormatCountdown(nearestTask.DueDate, nearestTask.Status);
                nearestDto = new NearestDeadlineDto
                {
                    Id = nearestTask.Id,
                    Title = nearestTask.Title,
                    SubjectName = nearestTask.Subject?.SubjectName ?? "General",
                    SubjectColor = nearestTask.Subject?.Color ?? "#B98AFF",
                    DueDateLabel = nearestTask.DueDate.ToString("MMM dd, hh:mm tt", CultureInfo.InvariantCulture),
                    Countdown = countdown
                };
            }

            return new DashboardStatsDto
            {
                TotalTasks = total,
                CompletedTasks = completed,
                PendingTasks = pending,
                OverdueTasks = overdue,
                Upcoming7d = upcoming7d,
                CompletionPercentage = completionRate,
                NearestDeadline = nearestDto
            };
        }

        public async Task<HeatmapDto> GetActivityHeatmapAsync(int userId, int weeks = 12)
        {
            var now = DateTime.Now.Date;
            int totalDays = weeks * 7;
            var startDate = now.AddDays(-(totalDays - 1));

            // Align startDate to nearest previous Monday (DayOfWeek.Monday)
            int diffToMonday = ((int)startDate.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
            startDate = startDate.AddDays(-diffToMonday);

            var startDateTime = startDate;

            // Query history changes
            var historyRecords = await _db.TaskStatusHistories
                .Include(h => h.TaskItem)
                .Where(h => h.TaskItem != null && h.TaskItem.UserId == userId && h.ChangedAt >= startDateTime)
                .Select(h => h.ChangedAt.Date)
                .ToListAsync();

            // Query task creations
            var taskCreations = await _db.Tasks
                .Where(t => t.UserId == userId && t.CreatedAt >= startDateTime)
                .Select(t => t.CreatedAt.Date)
                .ToListAsync();

            var countByDay = new Dictionary<DateTime, int>();
            foreach (var d in historyRecords.Concat(taskCreations))
            {
                countByDay[d] = countByDay.GetValueOrDefault(d, 0) + 1;
            }

            var weeksList = new List<List<HeatmapDayDto>>();
            var currentDay = startDate;
            int activeDaysCount = 0;
            int totalActions = 0;

            // Generate week columns (each week has 7 days Monday -> Sunday)
            while (currentDay <= now || currentDay.DayOfWeek != DayOfWeek.Monday)
            {
                var week = new List<HeatmapDayDto>();
                for (int i = 0; i < 7; i++)
                {
                    int c = countByDay.GetValueOrDefault(currentDay.Date, 0);
                    if (c > 0)
                    {
                        activeDaysCount++;
                        totalActions += c;
                    }

                    int level = 0;
                    if (c == 1) level = 1;
                    else if (c == 2) level = 2;
                    else if (c >= 3 && c <= 4) level = 3;
                    else if (c >= 5) level = 4;

                    week.Add(new HeatmapDayDto
                    {
                        Date = currentDay.ToString("yyyy-MM-dd"),
                        FormattedDate = currentDay.ToString("ddd, MMM dd", CultureInfo.InvariantCulture),
                        Count = c,
                        Level = level
                    });

                    currentDay = currentDay.AddDays(1);
                }
                weeksList.Add(week);
                if (currentDay > now && currentDay.DayOfWeek == DayOfWeek.Monday)
                    break;
            }

            return new HeatmapDto
            {
                TotalActiveDays = activeDaysCount,
                TotalActions = totalActions,
                Weeks = weeksList
            };
        }

        public async Task<List<TaskDto>> GetUserTasksAsync(int userId, TaskFilterParams? filters = null, string sortBy = "deadline_asc", string? search = null)
        {
            filters ??= new TaskFilterParams();
            var query = _db.Tasks
                .Include(t => t.Subject)
                .Where(t => t.UserId == userId);

            // Search query
            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLower();
                query = query.Where(t => t.Title.ToLower().Contains(term) || (t.Description != null && t.Description.ToLower().Contains(term)));
            }

            // Subject Filter
            if (filters.SubjectId.HasValue && filters.SubjectId.Value > 0)
            {
                query = query.Where(t => t.SubjectId == filters.SubjectId.Value);
            }

            // Category / Task Type
            if (!string.IsNullOrWhiteSpace(filters.TaskType))
            {
                query = query.Where(t => t.TaskType == filters.TaskType);
            }

            // Priority
            if (!string.IsNullOrWhiteSpace(filters.Priority))
            {
                query = query.Where(t => t.Priority == filters.Priority.ToLower());
            }

            // Status
            if (!string.IsNullOrWhiteSpace(filters.Status))
            {
                query = query.Where(t => t.Status == filters.Status.ToLower());
            }

            // Quick filter
            var now = DateTime.Now;
            if (filters.Filter == "overdue")
            {
                query = query.Where(t => t.Status != "completed" && t.DueDate < now);
            }
            else if (filters.Filter == "upcoming")
            {
                query = query.Where(t => t.Status != "completed" && t.DueDate >= now);
            }
            else if (filters.Filter == "today")
            {
                var todayEnd = now.Date.AddDays(1).AddTicks(-1);
                query = query.Where(t => t.DueDate >= now.Date && t.DueDate <= todayEnd);
            }

            // Sorting
            query = sortBy switch
            {
                "deadline_desc" => query.OrderByDescending(t => t.DueDate),
                "priority_high" => query.OrderByDescending(t => t.Priority == "high" ? 3 : t.Priority == "medium" ? 2 : 1).ThenBy(t => t.DueDate),
                "created_desc" => query.OrderByDescending(t => t.CreatedAt),
                _ => query.OrderBy(t => t.DueDate)
            };

            var list = await query.ToListAsync();
            return list.Select(MapToDto).ToList();
        }

        public async Task<List<TaskDto>> GetUpcomingExamsAsync(int userId, int limit = 4)
        {
            var now = DateTime.Now;
            var exams = await _db.Tasks
                .Include(t => t.Subject)
                .Where(t => t.UserId == userId && t.TaskType == "Examination" && t.Status != "completed" && t.DueDate >= now)
                .OrderBy(t => t.DueDate)
                .Take(limit)
                .ToListAsync();

            return exams.Select(MapToDto).ToList();
        }

        public async Task<TaskDto?> GetTaskByIdAsync(int taskId, int userId)
        {
            var task = await _db.Tasks
                .Include(t => t.Subject)
                .FirstOrDefaultAsync(t => t.Id == taskId && t.UserId == userId);

            return task != null ? MapToDto(task) : null;
        }

        public async Task<(int TaskId, string? Error)> CreateTaskAsync(int userId, TaskFormViewModel model)
        {
            if (string.IsNullOrWhiteSpace(model.Title))
                return (0, "Task title is required.");

            // Verify subject belongs to user
            var subjectExists = await _db.Subjects.AnyAsync(s => s.Id == model.SubjectId && s.UserId == userId);
            if (!subjectExists)
                return (0, "Selected subject was not found or belongs to another user.");

            DateTime parsedDueDate;
            if (!string.IsNullOrWhiteSpace(model.DueDate))
            {
                if (!DateTime.TryParse(model.DueDate, CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedDueDate) &&
                    !DateTime.TryParse(model.DueDate, out parsedDueDate))
                {
                    return (0, "Invalid due date format.");
                }
            }
            else if (!string.IsNullOrWhiteSpace(model.DueDateOnly))
            {
                var timePart = string.IsNullOrWhiteSpace(model.DueTimeOnly) ? "23:59:00" : model.DueTimeOnly;
                var combinedStr = $"{model.DueDateOnly} {timePart}";
                if (!DateTime.TryParse(combinedStr, out parsedDueDate))
                {
                    return (0, "Invalid date or time specified.");
                }
            }
            else
            {
                parsedDueDate = DateTime.Now.AddDays(1);
            }

            var now = DateTime.Now;
            var task = new TaskItem
            {
                UserId = userId,
                SubjectId = model.SubjectId,
                Title = model.Title.Trim(),
                Description = model.Description?.Trim(),
                TaskType = string.IsNullOrWhiteSpace(model.TaskType) ? "Assignment" : model.TaskType,
                Priority = string.IsNullOrWhiteSpace(model.Priority) ? "medium" : model.Priority.ToLower(),
                Status = string.IsNullOrWhiteSpace(model.Status) ? "pending" : model.Status.ToLower(),
                DueDate = parsedDueDate,
                CreatedAt = now,
                UpdatedAt = now
            };

            _db.Tasks.Add(task);
            await _db.SaveChangesAsync();

            // Record initial history
            _db.TaskStatusHistories.Add(new TaskStatusHistory
            {
                TaskItemId = task.Id,
                OldStatus = null,
                NewStatus = task.Status,
                ChangedAt = now
            });
            await _db.SaveChangesAsync();

            return (task.Id, null);
        }

        public async Task<(bool Success, string? Error)> UpdateTaskAsync(int taskId, int userId, TaskFormViewModel model)
        {
            var task = await _db.Tasks.FirstOrDefaultAsync(t => t.Id == taskId && t.UserId == userId);
            if (task == null)
                return (false, "Task not found.");

            if (string.IsNullOrWhiteSpace(model.Title))
                return (false, "Task title is required.");

            var subjectExists = await _db.Subjects.AnyAsync(s => s.Id == model.SubjectId && s.UserId == userId);
            if (!subjectExists)
                return (false, "Selected subject does not exist.");

            DateTime parsedDueDate;
            if (!string.IsNullOrWhiteSpace(model.DueDate))
            {
                if (!DateTime.TryParse(model.DueDate, CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedDueDate) &&
                    !DateTime.TryParse(model.DueDate, out parsedDueDate))
                {
                    return (false, "Invalid due date format.");
                }
            }
            else if (!string.IsNullOrWhiteSpace(model.DueDateOnly))
            {
                var timePart = string.IsNullOrWhiteSpace(model.DueTimeOnly) ? "23:59:00" : model.DueTimeOnly;
                var combinedStr = $"{model.DueDateOnly} {timePart}";
                if (!DateTime.TryParse(combinedStr, out parsedDueDate))
                {
                    return (false, "Invalid date or time format.");
                }
            }
            else
            {
                parsedDueDate = task.DueDate;
            }

            string oldStatus = task.Status;
            string newStatus = string.IsNullOrWhiteSpace(model.Status) ? task.Status : model.Status.ToLower();

            task.Title = model.Title.Trim();
            task.SubjectId = model.SubjectId;
            task.Description = model.Description?.Trim();
            task.TaskType = string.IsNullOrWhiteSpace(model.TaskType) ? task.TaskType : model.TaskType;
            task.Priority = string.IsNullOrWhiteSpace(model.Priority) ? task.Priority : model.Priority.ToLower();
            task.Status = newStatus;
            task.DueDate = parsedDueDate;
            task.UpdatedAt = DateTime.Now;

            if (oldStatus != newStatus)
            {
                _db.TaskStatusHistories.Add(new TaskStatusHistory
                {
                    TaskItemId = task.Id,
                    OldStatus = oldStatus,
                    NewStatus = newStatus,
                    ChangedAt = DateTime.Now
                });
            }

            await _db.SaveChangesAsync();
            return (true, null);
        }

        public async Task<(bool Success, string? Error)> UpdateTaskStatusAsync(int taskId, int userId, string newStatus)
        {
            newStatus = newStatus?.Trim().ToLower() ?? "";
            if (newStatus != "pending" && newStatus != "in_progress" && newStatus != "completed")
                return (false, "Invalid task status specified.");

            var task = await _db.Tasks.FirstOrDefaultAsync(t => t.Id == taskId && t.UserId == userId);
            if (task == null)
                return (false, "Task not found.");

            if (task.Status != newStatus)
            {
                string old = task.Status;
                task.Status = newStatus;
                task.UpdatedAt = DateTime.Now;

                _db.TaskStatusHistories.Add(new TaskStatusHistory
                {
                    TaskItemId = task.Id,
                    OldStatus = old,
                    NewStatus = newStatus,
                    ChangedAt = DateTime.Now
                });

                await _db.SaveChangesAsync();
            }

            return (true, null);
        }

        public async Task<(bool Success, string? Error)> DeleteTaskAsync(int taskId, int userId)
        {
            var task = await _db.Tasks.FirstOrDefaultAsync(t => t.Id == taskId && t.UserId == userId);
            if (task == null)
                return (false, "Task not found.");

            _db.Tasks.Remove(task);
            await _db.SaveChangesAsync();
            return (true, null);
        }

        public async Task<List<SubjectDto>> GetUserSubjectsAsync(int userId, string? search = null, string? semester = null)
        {
            var query = _db.Subjects
                .Include(s => s.Tasks)
                .Where(s => s.UserId == userId);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLower();
                query = query.Where(s => s.SubjectName.ToLower().Contains(term) || (s.SubjectCode != null && s.SubjectCode.ToLower().Contains(term)));
            }

            if (!string.IsNullOrWhiteSpace(semester) && int.TryParse(semester, out int semVal))
            {
                query = query.Where(s => s.Semester == semVal);
            }

            var subjects = await query.OrderBy(s => s.Semester).ThenBy(s => s.SubjectName).ToListAsync();

            return subjects.Select(s =>
            {
                int tot = s.Tasks.Count;
                int comp = s.Tasks.Count(t => t.Status == "completed");
                int pend = s.Tasks.Count(t => t.Status != "completed");
                double rate = tot > 0 ? Math.Round((double)comp / tot * 100, 1) : 0;

                return new SubjectDto
                {
                    Id = s.Id,
                    UserId = s.UserId,
                    SubjectName = s.SubjectName,
                    SubjectCode = s.SubjectCode,
                    Semester = s.Semester,
                    Color = s.Color,
                    TotalTasks = tot,
                    CompletedTasks = comp,
                    PendingTasks = pend,
                    CompletionRate = rate,
                    CreatedAt = s.CreatedAt
                };
            }).ToList();
        }

        public async Task<SubjectDto?> GetSubjectByIdAsync(int subjectId, int userId)
        {
            var s = await _db.Subjects
                .Include(x => x.Tasks)
                .FirstOrDefaultAsync(x => x.Id == subjectId && x.UserId == userId);

            if (s == null) return null;

            int tot = s.Tasks.Count;
            int comp = s.Tasks.Count(t => t.Status == "completed");
            int pend = s.Tasks.Count(t => t.Status != "completed");
            double rate = tot > 0 ? Math.Round((double)comp / tot * 100, 1) : 0;

            return new SubjectDto
            {
                Id = s.Id,
                UserId = s.UserId,
                SubjectName = s.SubjectName,
                SubjectCode = s.SubjectCode,
                Semester = s.Semester,
                Color = s.Color,
                TotalTasks = tot,
                CompletedTasks = comp,
                PendingTasks = pend,
                CompletionRate = rate,
                CreatedAt = s.CreatedAt
            };
        }

        public async Task<(int SubjectId, string? Error)> CreateSubjectAsync(int userId, SubjectFormViewModel model)
        {
            if (string.IsNullOrWhiteSpace(model.SubjectName))
                return (0, "Subject name is required.");

            if (model.Semester < 1 || model.Semester > 12)
                return (0, "Semester must be between 1 and 12.");

            // Check duplicate code
            if (!string.IsNullOrWhiteSpace(model.SubjectCode))
            {
                var cleanCode = model.SubjectCode.Trim().ToUpper();
                bool exists = await _db.Subjects.AnyAsync(s => s.UserId == userId && s.SubjectCode != null && s.SubjectCode.ToUpper() == cleanCode);
                if (exists)
                    return (0, $"A subject with code '{cleanCode}' already exists for your account.");
                model.SubjectCode = cleanCode;
            }

            var subject = new Subject
            {
                UserId = userId,
                SubjectName = model.SubjectName.Trim(),
                SubjectCode = model.SubjectCode?.Trim().ToUpper(),
                Semester = model.Semester,
                Color = string.IsNullOrWhiteSpace(model.Color) ? "#B98AFF" : model.Color,
                CreatedAt = DateTime.Now
            };

            _db.Subjects.Add(subject);
            await _db.SaveChangesAsync();

            return (subject.Id, null);
        }

        public async Task<(bool Success, string? Error)> UpdateSubjectAsync(int subjectId, int userId, SubjectFormViewModel model)
        {
            var subject = await _db.Subjects.FirstOrDefaultAsync(s => s.Id == subjectId && s.UserId == userId);
            if (subject == null)
                return (false, "Subject not found.");

            if (string.IsNullOrWhiteSpace(model.SubjectName))
                return (false, "Subject name is required.");

            if (model.Semester < 1 || model.Semester > 12)
                return (false, "Semester must be between 1 and 12.");

            if (!string.IsNullOrWhiteSpace(model.SubjectCode))
            {
                var cleanCode = model.SubjectCode.Trim().ToUpper();
                bool exists = await _db.Subjects.AnyAsync(s => s.UserId == userId && s.Id != subjectId && s.SubjectCode != null && s.SubjectCode.ToUpper() == cleanCode);
                if (exists)
                    return (false, $"A subject with code '{cleanCode}' already exists.");
                subject.SubjectCode = cleanCode;
            }
            else
            {
                subject.SubjectCode = null;
            }

            subject.SubjectName = model.SubjectName.Trim();
            subject.Semester = model.Semester;
            subject.Color = string.IsNullOrWhiteSpace(model.Color) ? subject.Color : model.Color;

            await _db.SaveChangesAsync();
            return (true, null);
        }

        public async Task<(bool Success, string? Error)> DeleteSubjectAsync(int subjectId, int userId)
        {
            var subject = await _db.Subjects.FirstOrDefaultAsync(s => s.Id == subjectId && s.UserId == userId);
            if (subject == null)
                return (false, "Subject not found.");

            _db.Subjects.Remove(subject);
            await _db.SaveChangesAsync();
            return (true, null);
        }
    }
}
