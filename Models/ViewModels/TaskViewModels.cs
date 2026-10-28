using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;

namespace CampusDeadline.Models.ViewModels
{
    public class TaskFormViewModel
    {
        [JsonPropertyName("task_id")]
        [FromForm(Name = "task_id")]
        public int? TaskId { get; set; }

        [Required(ErrorMessage = "Task title is required.")]
        [StringLength(200, MinimumLength = 3, ErrorMessage = "Title must be between 3 and 200 characters.")]
        [JsonPropertyName("title")]
        [FromForm(Name = "title")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please select a subject.")]
        [JsonPropertyName("subject_id")]
        [FromForm(Name = "subject_id")]
        public int SubjectId { get; set; }

        [Required]
        [JsonPropertyName("task_type")]
        [FromForm(Name = "task_type")]
        public string TaskType { get; set; } = "Assignment"; // Assignment, Practical, Examination, Project, Other

        [Required]
        [JsonPropertyName("priority")]
        [FromForm(Name = "priority")]
        public string Priority { get; set; } = "medium"; // low, medium, high

        [Required]
        [JsonPropertyName("status")]
        [FromForm(Name = "status")]
        public string Status { get; set; } = "pending"; // pending, in_progress, completed

        [JsonPropertyName("due_date_only")]
        [FromForm(Name = "due_date_only")]
        public string? DueDateOnly { get; set; }

        [JsonPropertyName("due_time_only")]
        [FromForm(Name = "due_time_only")]
        public string? DueTimeOnly { get; set; }

        [JsonPropertyName("due_date")]
        [FromForm(Name = "due_date")]
        public string? DueDate { get; set; }

        [StringLength(2000, ErrorMessage = "Description cannot exceed 2000 characters.")]
        [JsonPropertyName("description")]
        [FromForm(Name = "description")]
        public string? Description { get; set; }
    }

    public class TaskDto
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("user_id")]
        public int UserId { get; set; }

        [JsonPropertyName("subject_id")]
        public int SubjectId { get; set; }

        [JsonPropertyName("subject_name")]
        public string SubjectName { get; set; } = string.Empty;

        [JsonPropertyName("subject_code")]
        public string? SubjectCode { get; set; }

        [JsonPropertyName("subject_color")]
        public string SubjectColor { get; set; } = "#B98AFF";

        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("task_type")]
        public string TaskType { get; set; } = "Assignment";

        [JsonPropertyName("priority")]
        public string Priority { get; set; } = "medium";

        [JsonPropertyName("status")]
        public string Status { get; set; } = "pending";

        [JsonPropertyName("due_date")]
        public DateTime DueDate { get; set; }

        [JsonPropertyName("due_date_formatted")]
        public string DueDateFormatted { get; set; } = string.Empty;

        [JsonPropertyName("due_date_short")]
        public string DueDateShort { get; set; } = string.Empty;

        [JsonPropertyName("due_time_formatted")]
        public string DueTimeFormatted { get; set; } = string.Empty;

        [JsonPropertyName("due_date_raw")]
        public string DueDateRaw { get; set; } = string.Empty;

        [JsonPropertyName("due_time_raw")]
        public string DueTimeRaw { get; set; } = string.Empty;

        [JsonPropertyName("countdown")]
        public string Countdown { get; set; } = string.Empty;

        [JsonPropertyName("is_overdue")]
        public bool IsOverdue { get; set; }

        [JsonPropertyName("urgency")]
        public string Urgency { get; set; } = "normal";

        [JsonPropertyName("created_at")]
        public DateTime CreatedAt { get; set; }
    }

    public class TaskFilterParams
    {
        public string? Search { get; set; }
        public int? SubjectId { get; set; }
        public string? TaskType { get; set; }
        public string? Priority { get; set; }
        public string? Status { get; set; }
        public string? Filter { get; set; } // Quick filter: overdue, upcoming, today
        public string SortBy { get; set; } = "deadline_asc";
    }

    public class TasksListViewModel
    {
        public List<TaskDto> Tasks { get; set; } = new();
        public List<SubjectDto> Subjects { get; set; } = new();
        public DashboardStatsDto Stats { get; set; } = new();
        public TaskFilterParams CurrentFilters { get; set; } = new();
        public List<string> TaskTypes { get; set; } = new() { "Assignment", "Practical", "Examination", "Project", "Other" };
        public List<string> Priorities { get; set; } = new() { "low", "medium", "high" };
        public List<string> Statuses { get; set; } = new() { "pending", "in_progress", "completed" };
    }
}
