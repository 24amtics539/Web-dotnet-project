using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CampusDeadline.Models.ViewModels
{
    public class InsightDto
    {
        [JsonPropertyName("icon")]
        public string Icon { get; set; } = "check-circle";

        [JsonPropertyName("type")]
        public string Type { get; set; } = "success";

        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("text")]
        public string Text { get; set; } = string.Empty;
    }

    public class SummaryKpisDto
    {
        [JsonPropertyName("total_tasks")]
        public int TotalTasks { get; set; }

        [JsonPropertyName("completed_tasks")]
        public int CompletedTasks { get; set; }

        [JsonPropertyName("pending_tasks")]
        public int PendingTasks { get; set; }

        [JsonPropertyName("overdue_tasks")]
        public int OverdueTasks { get; set; }

        [JsonPropertyName("completion_rate")]
        public double CompletionRate { get; set; }

        [JsonPropertyName("best_subject")]
        public string BestSubject { get; set; } = "None";

        [JsonPropertyName("best_subject_rate")]
        public double BestSubjectRate { get; set; }
    }

    public class StatusDistributionDto
    {
        [JsonPropertyName("labels")]
        public List<string> Labels { get; set; } = new() { "Completed", "In Progress", "Pending", "Overdue" };

        [JsonPropertyName("data")]
        public List<int> Data { get; set; } = new();
    }

    public class SubjectDistributionDto
    {
        [JsonPropertyName("labels")]
        public List<string> Labels { get; set; } = new();

        [JsonPropertyName("completed")]
        public List<int> Completed { get; set; } = new();

        [JsonPropertyName("incomplete")]
        public List<int> Incomplete { get; set; } = new();
    }

    public class TrendChartDto
    {
        [JsonPropertyName("labels")]
        public List<string> Labels { get; set; } = new();

        [JsonPropertyName("data")]
        public List<int> Data { get; set; } = new();
    }

    public class CategoryChartDto
    {
        [JsonPropertyName("labels")]
        public List<string> Labels { get; set; } = new() { "Assignment", "Practical", "Examination", "Project", "Other" };

        [JsonPropertyName("data")]
        public List<int> Data { get; set; } = new();
    }

    public class ProgressChartsDto
    {
        [JsonPropertyName("status_distribution")]
        public StatusDistributionDto StatusDistribution { get; set; } = new();

        [JsonPropertyName("subject_distribution")]
        public SubjectDistributionDto SubjectDistribution { get; set; } = new();

        [JsonPropertyName("completion_trend")]
        public TrendChartDto CompletionTrend { get; set; } = new();

        [JsonPropertyName("type_distribution")]
        public CategoryChartDto TypeDistribution { get; set; } = new();
    }

    public class ProgressAnalyticsDto
    {
        [JsonPropertyName("summary")]
        public SummaryKpisDto Summary { get; set; } = new();

        [JsonPropertyName("charts")]
        public ProgressChartsDto Charts { get; set; } = new();

        [JsonPropertyName("insights")]
        public List<InsightDto> Insights { get; set; } = new();

        [JsonPropertyName("period")]
        public string Period { get; set; } = "all";
    }

    public class ProgressViewModel
    {
        public ProgressAnalyticsDto Analytics { get; set; } = new();
        public string CurrentPeriod { get; set; } = "all";
    }
}
