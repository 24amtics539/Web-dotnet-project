using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;

namespace CampusDeadline.Models.ViewModels
{
    public class SubjectFormViewModel
    {
        [JsonPropertyName("subject_id")]
        [FromForm(Name = "subject_id")]
        public int? SubjectId { get; set; }

        [Required(ErrorMessage = "Subject name is required.")]
        [StringLength(150, MinimumLength = 2, ErrorMessage = "Subject name must be between 2 and 150 characters.")]
        [JsonPropertyName("subject_name")]
        [FromForm(Name = "subject_name")]
        public string SubjectName { get; set; } = string.Empty;

        [StringLength(50, ErrorMessage = "Subject code cannot exceed 50 characters.")]
        [JsonPropertyName("subject_code")]
        [FromForm(Name = "subject_code")]
        public string? SubjectCode { get; set; }

        [Required(ErrorMessage = "Semester is required.")]
        [Range(1, 12, ErrorMessage = "Semester must be between 1 and 12.")]
        [JsonPropertyName("semester")]
        [FromForm(Name = "semester")]
        public int Semester { get; set; } = 1;

        [MaxLength(20)]
        [JsonPropertyName("color")]
        [FromForm(Name = "color")]
        public string Color { get; set; } = "#B98AFF";
    }

    public class SubjectDto
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("user_id")]
        public int UserId { get; set; }

        [JsonPropertyName("subject_name")]
        public string SubjectName { get; set; } = string.Empty;

        [JsonPropertyName("subject_code")]
        public string? SubjectCode { get; set; }

        [JsonPropertyName("semester")]
        public int Semester { get; set; }

        [JsonPropertyName("color")]
        public string Color { get; set; } = "#B98AFF";

        [JsonPropertyName("total_tasks")]
        public int TotalTasks { get; set; }

        [JsonPropertyName("completed_tasks")]
        public int CompletedTasks { get; set; }

        [JsonPropertyName("pending_tasks")]
        public int PendingTasks { get; set; }

        [JsonPropertyName("completion_rate")]
        public double CompletionRate { get; set; }

        [JsonPropertyName("created_at")]
        public DateTime CreatedAt { get; set; }
    }

    public class SubjectsListViewModel
    {
        public List<SubjectDto> Subjects { get; set; } = new();
        public string? CurrentSearch { get; set; }
        public string? CurrentSemester { get; set; }
    }
}
