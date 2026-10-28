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
    public class AnalyticsService : IAnalyticsService
    {
        private readonly ApplicationDbContext _db;

        public AnalyticsService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<ProgressAnalyticsDto> GetProgressAnalyticsAsync(int userId, string period = "all")
        {
            period = period?.ToLower().Trim() ?? "all";
            var now = DateTime.Now;
            DateTime? cutoff = period switch
            {
                "7d" => now.AddDays(-7),
                "30d" => now.AddDays(-30),
                "semester" => now.AddDays(-120),
                _ => null
            };

            var tasksQuery = _db.Tasks
                .Include(t => t.Subject)
                .Where(t => t.UserId == userId);

            if (cutoff.HasValue)
            {
                tasksQuery = tasksQuery.Where(t => t.CreatedAt >= cutoff.Value || t.DueDate >= cutoff.Value);
            }

            var allTasks = await tasksQuery.ToListAsync();
            var subjects = await _db.Subjects
                .Include(s => s.Tasks)
                .Where(s => s.UserId == userId)
                .ToListAsync();

            int total = allTasks.Count;
            int completed = allTasks.Count(t => t.Status == "completed");
            int pending = allTasks.Count(t => t.Status == "pending");
            int inProgress = allTasks.Count(t => t.Status == "in_progress");
            int overdue = allTasks.Count(t => t.Status != "completed" && t.DueDate < now);
            double completionRate = total > 0 ? Math.Round((double)completed / total * 100, 1) : 0;

            // Find top performing course
            string bestSubject = "None";
            double bestRate = 0;
            Subject? worstOverdueSubject = null;
            int maxOverdueCount = 0;

            foreach (var s in subjects)
            {
                int sTot = s.Tasks.Count;
                int sComp = s.Tasks.Count(t => t.Status == "completed");
                int sOver = s.Tasks.Count(t => t.Status != "completed" && t.DueDate < now);
                double sRate = sTot > 0 ? (double)sComp / sTot * 100 : 0;

                if (sTot > 0 && sRate >= bestRate)
                {
                    bestRate = Math.Round(sRate, 1);
                    bestSubject = s.SubjectName;
                }

                if (sOver > maxOverdueCount)
                {
                    maxOverdueCount = sOver;
                    worstOverdueSubject = s;
                }
            }

            // Status Distribution Chart
            var statusChart = new StatusDistributionDto
            {
                Labels = new List<string> { "Completed", "In Progress", "Pending", "Overdue" },
                Data = new List<int> { completed, inProgress, pending, overdue }
            };

            // Subject Distribution Chart
            var subjectChart = new SubjectDistributionDto();
            foreach (var s in subjects)
            {
                subjectChart.Labels.Add(s.SubjectCode ?? s.SubjectName);
                subjectChart.Completed.Add(s.Tasks.Count(t => t.Status == "completed"));
                subjectChart.Incomplete.Add(s.Tasks.Count(t => t.Status != "completed"));
            }

            // 14-day cumulative completion trend
            var trendStartDate = now.Date.AddDays(-13);
            var historyCompletions = await _db.TaskStatusHistories
                .Include(h => h.TaskItem)
                .Where(h => h.TaskItem != null && h.TaskItem.UserId == userId && h.NewStatus == "completed" && h.ChangedAt >= trendStartDate)
                .Select(h => h.ChangedAt.Date)
                .ToListAsync();

            var completionsByDay = historyCompletions.GroupBy(d => d).ToDictionary(g => g.Key, g => g.Count());

            var trendChart = new TrendChartDto();
            int cumulative = 0;
            for (int i = 13; i >= 0; i--)
            {
                var day = now.Date.AddDays(-i);
                cumulative += completionsByDay.GetValueOrDefault(day, 0);
                trendChart.Labels.Add(day.ToString("MMM dd", CultureInfo.InvariantCulture));
                trendChart.Data.Add(cumulative);
            }

            // Category Breakdown Chart
            var categoryTypes = new List<string> { "Assignment", "Practical", "Examination", "Project", "Other" };
            var categoryChart = new CategoryChartDto
            {
                Labels = categoryTypes,
                Data = categoryTypes.Select(type => allTasks.Count(t => t.TaskType == type)).ToList()
            };

            // Rule-based Academic Insights
            var insights = new List<InsightDto>();
            var weekAgo = now.AddDays(-7);
            int weeklyCompleted = historyCompletions.Count(d => d >= weekAgo.Date);

            if (weeklyCompleted > 0)
            {
                insights.Add(new InsightDto
                {
                    Icon = "check-circle-2",
                    Type = "success",
                    Title = "Weekly Momentum",
                    Text = $"You completed {weeklyCompleted} academic task{(weeklyCompleted > 1 ? "s" : "")} this past week. Great job keeping the pace!"
                });
            }
            else
            {
                insights.Add(new InsightDto
                {
                    Icon = "clock",
                    Type = "info",
                    Title = "Start Your Momentum",
                    Text = "No tasks completed yet this week. Complete your next deadline to build your streak!"
                });
            }

            if (overdue > 0)
            {
                if (worstOverdueSubject != null)
                {
                    insights.Add(new InsightDto
                    {
                        Icon = "alert-triangle",
                        Type = "warning",
                        Title = "Overdue Attention Required",
                        Text = $"You have {overdue} overdue task{(overdue > 1 ? "s" : "")}, primarily in {worstOverdueSubject.SubjectName}. Prioritize completing or rescheduling."
                    });
                }
                else
                {
                    insights.Add(new InsightDto
                    {
                        Icon = "alert-triangle",
                        Type = "warning",
                        Title = "Overdue Deadlines",
                        Text = $"You have {overdue} overdue task{(overdue > 1 ? "s" : "")}. Review your task list to catch up."
                    });
                }
            }
            else
            {
                insights.Add(new InsightDto
                {
                    Icon = "award",
                    Type = "success",
                    Title = "Zero Overdue Tasks",
                    Text = "All deadlines are up to date! You currently have zero overdue tasks across all enrolled subjects."
                });
            }

            if (bestSubject != "None")
            {
                insights.Add(new InsightDto
                {
                    Icon = "trending-up",
                    Type = "primary",
                    Title = "Top Performing Course",
                    Text = $"{bestSubject} leads with {bestRate}% tasks completed."
                });
            }

            int pendingCount = pending + inProgress;
            if (pendingCount > 0)
            {
                insights.Add(new InsightDto
                {
                    Icon = "calendar",
                    Type = "info",
                    Title = "Upcoming Schedule",
                    Text = $"You have {pendingCount} pending task{(pendingCount > 1 ? "s" : "")} scheduled. Plan your study blocks accordingly."
                });
            }

            return new ProgressAnalyticsDto
            {
                Period = period,
                Summary = new SummaryKpisDto
                {
                    TotalTasks = total,
                    CompletedTasks = completed,
                    PendingTasks = pendingCount,
                    OverdueTasks = overdue,
                    CompletionRate = completionRate,
                    BestSubject = bestSubject,
                    BestSubjectRate = bestRate
                },
                Charts = new ProgressChartsDto
                {
                    StatusDistribution = statusChart,
                    SubjectDistribution = subjectChart,
                    CompletionTrend = trendChart,
                    TypeDistribution = categoryChart
                },
                Insights = insights
            };
        }
    }
}
