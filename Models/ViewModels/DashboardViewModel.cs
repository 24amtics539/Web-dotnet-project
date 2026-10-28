using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CampusDeadline.Models.ViewModels
{
    public class NearestDeadlineDto
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("subject_name")]
        public string SubjectName { get; set; } = string.Empty;

        [JsonPropertyName("subject_color")]
        public string SubjectColor { get; set; } = "#B98AFF";

        [JsonPropertyName("due_date_label")]
        public string DueDateLabel { get; set; } = string.Empty;

        [JsonPropertyName("countdown")]
        public string Countdown { get; set; } = string.Empty;
    }

    public class DashboardStatsDto
    {
        [JsonPropertyName("total_tasks")]
        public int TotalTasks { get; set; }

        [JsonPropertyName("completed_tasks")]
        public int CompletedTasks { get; set; }

        [JsonPropertyName("pending_tasks")]
        public int PendingTasks { get; set; }

        [JsonPropertyName("overdue_tasks")]
        public int OverdueTasks { get; set; }

        [JsonPropertyName("upcoming_7d")]
        public int Upcoming7d { get; set; }

        [JsonPropertyName("completion_percentage")]
        public double CompletionPercentage { get; set; }

        [JsonPropertyName("nearest_deadline")]
        public NearestDeadlineDto? NearestDeadline { get; set; }
    }

    public class HeatmapDayDto
    {
        [JsonPropertyName("date")]
        public string Date { get; set; } = string.Empty; // YYYY-MM-DD

        [JsonPropertyName("formatted_date")]
        public string FormattedDate { get; set; } = string.Empty; // Mon, Oct 27

        [JsonPropertyName("count")]
        public int Count { get; set; }

        [JsonPropertyName("level")]
        public int Level { get; set; } // 0 to 4
    }

    public class HeatmapDto
    {
        [JsonPropertyName("total_active_days")]
        public int TotalActiveDays { get; set; }

        [JsonPropertyName("total_actions")]
        public int TotalActions { get; set; }

        [JsonPropertyName("weeks")]
        public List<List<HeatmapDayDto>> Weeks { get; set; } = new();
    }

    public class DashboardViewModel
    {
        public DashboardStatsDto Stats { get; set; } = new();
        public HeatmapDto Heatmap { get; set; } = new();
        public List<TaskDto> UpcomingTasks { get; set; } = new();
        public List<TaskDto> InProgressTasks { get; set; } = new();
        public List<TaskDto> ActiveTasks { get; set; } = new();
        public List<TaskDto> UpcomingExams { get; set; } = new();
        public List<SubjectDto> Subjects { get; set; } = new();
    }
}
