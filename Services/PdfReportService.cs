using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using CampusDeadline.Data;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace CampusDeadline.Services
{
    public class PdfReportService : IPdfReportService
    {
        private readonly ApplicationDbContext _db;

        public PdfReportService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<byte[]> GenerateAcademicReportPdfAsync(int userId)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId);
            var userName = user?.Name ?? "Student";
            var userEmail = user?.Email ?? "student@university.edu";

            var subjects = await _db.Subjects
                .Include(s => s.Tasks)
                .Where(s => s.UserId == userId)
                .OrderBy(s => s.Semester)
                .ThenBy(s => s.SubjectName)
                .ToListAsync();

            var tasks = await _db.Tasks
                .Include(t => t.Subject)
                .Where(t => t.UserId == userId)
                .OrderBy(t => t.DueDate)
                .ToListAsync();

            var now = DateTime.Now;
            int total = tasks.Count;
            int completed = tasks.Count(t => t.Status == "completed");
            int pending = tasks.Count(t => t.Status != "completed");
            int overdue = tasks.Count(t => t.Status != "completed" && t.DueDate < now);
            double rate = total > 0 ? Math.Round((double)completed / total * 100, 1) : 0;

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(36);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Lato"));

                    // Header
                    page.Header().Column(col =>
                    {
                        col.Item().Row(row =>
                        {
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text("CampusDeadline").FontSize(20).Bold().FontColor("#6B21A8");
                                c.Item().Text("Official Student Academic Performance & Deadline Report").FontSize(11).FontColor("#6B7280");
                            });

                            row.ConstantItem(150).Column(c =>
                            {
                                c.Item().AlignRight().Text($"Student: {userName}").Bold();
                                c.Item().AlignRight().Text(userEmail).FontSize(9).FontColor("#6B7280");
                                c.Item().AlignRight().Text($"Date: {now:MMM dd, yyyy}").FontSize(9).FontColor("#6B7280");
                            });
                        });

                        col.Item().PaddingVertical(10).LineHorizontal(1.5f).LineColor("#E5E7EB");
                    });

                    // Content
                    page.Content().Column(col =>
                    {
                        // 1. KPI Summary Cards
                        col.Item().Text("Academic Summary Metrics").FontSize(13).Bold().FontColor("#1F2937");
                        col.Item().PaddingTop(6).PaddingBottom(14).Row(row =>
                        {
                            row.RelativeItem().Border(1).BorderColor("#DDD6FE").Background("#F5F3FF").Padding(8).Column(c =>
                            {
                                c.Item().Text("TOTAL TASKS").FontSize(8).Bold().FontColor("#7C3AED");
                                c.Item().Text($"{total}").FontSize(16).Bold().FontColor("#1F2937");
                            });
                            row.Spacing(8);
                            row.RelativeItem().Border(1).BorderColor("#A7F3D0").Background("#ECFDF5").Padding(8).Column(c =>
                            {
                                c.Item().Text("COMPLETED").FontSize(8).Bold().FontColor("#059669");
                                c.Item().Text($"{completed}").FontSize(16).Bold().FontColor("#059669");
                            });
                            row.Spacing(8);
                            row.RelativeItem().Border(1).BorderColor("#DDD6FE").Background("#FAF5FF").Padding(8).Column(c =>
                            {
                                c.Item().Text("PENDING").FontSize(8).Bold().FontColor("#9333EA");
                                c.Item().Text($"{pending}").FontSize(16).Bold().FontColor("#9333EA");
                            });
                            row.Spacing(8);
                            row.RelativeItem().Border(1).BorderColor("#FECACA").Background("#FEF2F2").Padding(8).Column(c =>
                            {
                                c.Item().Text("OVERDUE").FontSize(8).Bold().FontColor("#DC2626");
                                c.Item().Text($"{overdue}").FontSize(16).Bold().FontColor("#DC2626");
                            });
                            row.Spacing(8);
                            row.RelativeItem().Border(1).BorderColor("#DDD6FE").Background("#F5F3FF").Padding(8).Column(c =>
                            {
                                c.Item().Text("COMPLETION RATE").FontSize(8).Bold().FontColor("#7C3AED");
                                c.Item().Text($"{rate}%").FontSize(16).Bold().FontColor("#1F2937");
                            });
                        });

                        // 2. Enrolled Subjects Table
                        col.Item().Text("Enrolled Course Breakdown").FontSize(13).Bold().FontColor("#1F2937");
                        col.Item().PaddingTop(6).PaddingBottom(14).Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(3); // Subject
                                columns.RelativeColumn(1.2f); // Code
                                columns.RelativeColumn(1); // Sem
                                columns.RelativeColumn(1); // Total
                                columns.RelativeColumn(1.2f); // Comp
                                columns.RelativeColumn(1.2f); // Pend
                                columns.RelativeColumn(1.4f); // Rate
                            });

                            table.Header(header =>
                            {
                                header.Cell().Background("#4C1D95").Padding(5).Text("Subject Name").Bold().FontColor(Colors.White);
                                header.Cell().Background("#4C1D95").Padding(5).Text("Code").Bold().FontColor(Colors.White);
                                header.Cell().Background("#4C1D95").Padding(5).Text("Sem").Bold().FontColor(Colors.White);
                                header.Cell().Background("#4C1D95").Padding(5).Text("Total").Bold().FontColor(Colors.White);
                                header.Cell().Background("#4C1D95").Padding(5).Text("Completed").Bold().FontColor(Colors.White);
                                header.Cell().Background("#4C1D95").Padding(5).Text("Pending").Bold().FontColor(Colors.White);
                                header.Cell().Background("#4C1D95").Padding(5).Text("Progress").Bold().FontColor(Colors.White);
                            });

                            foreach (var s in subjects)
                            {
                                int sTot = s.Tasks.Count;
                                int sComp = s.Tasks.Count(t => t.Status == "completed");
                                int sPend = s.Tasks.Count(t => t.Status != "completed");
                                double sRate = sTot > 0 ? Math.Round((double)sComp / sTot * 100) : 0;

                                table.Cell().BorderBottom(0.5f).BorderColor("#E5E7EB").Padding(4).Text(s.SubjectName);
                                table.Cell().BorderBottom(0.5f).BorderColor("#E5E7EB").Padding(4).Text(s.SubjectCode ?? "—");
                                table.Cell().BorderBottom(0.5f).BorderColor("#E5E7EB").Padding(4).Text($"Sem {s.Semester}");
                                table.Cell().BorderBottom(0.5f).BorderColor("#E5E7EB").Padding(4).Text($"{sTot}");
                                table.Cell().BorderBottom(0.5f).BorderColor("#E5E7EB").Padding(4).Text($"{sComp}");
                                table.Cell().BorderBottom(0.5f).BorderColor("#E5E7EB").Padding(4).Text($"{sPend}");
                                table.Cell().BorderBottom(0.5f).BorderColor("#E5E7EB").Padding(4).Text($"{sRate}%").Bold();
                            }
                        });

                        // 3. Academic Tasks Table
                        col.Item().Text("Academic Tasks & Deadlines").FontSize(13).Bold().FontColor("#1F2937");
                        col.Item().PaddingTop(6).Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(3); // Title
                                columns.RelativeColumn(1.2f); // Subject
                                columns.RelativeColumn(1.3f); // Category
                                columns.RelativeColumn(1); // Priority
                                columns.RelativeColumn(1.8f); // Due Date
                                columns.RelativeColumn(1.2f); // Status
                            });

                            table.Header(header =>
                            {
                                header.Cell().Background("#4C1D95").Padding(5).Text("Task Title").Bold().FontColor(Colors.White);
                                header.Cell().Background("#4C1D95").Padding(5).Text("Course").Bold().FontColor(Colors.White);
                                header.Cell().Background("#4C1D95").Padding(5).Text("Type").Bold().FontColor(Colors.White);
                                header.Cell().Background("#4C1D95").Padding(5).Text("Priority").Bold().FontColor(Colors.White);
                                header.Cell().Background("#4C1D95").Padding(5).Text("Due Date").Bold().FontColor(Colors.White);
                                header.Cell().Background("#4C1D95").Padding(5).Text("Status").Bold().FontColor(Colors.White);
                            });

                            foreach (var t in tasks)
                            {
                                string statLabel = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(t.Status.Replace('_', ' '));
                                table.Cell().BorderBottom(0.5f).BorderColor("#E5E7EB").Padding(4).Text(t.Title).SemiBold();
                                table.Cell().BorderBottom(0.5f).BorderColor("#E5E7EB").Padding(4).Text(t.Subject?.SubjectCode ?? t.Subject?.SubjectName ?? "—");
                                table.Cell().BorderBottom(0.5f).BorderColor("#E5E7EB").Padding(4).Text(t.TaskType);
                                table.Cell().BorderBottom(0.5f).BorderColor("#E5E7EB").Padding(4).Text(CultureInfo.CurrentCulture.TextInfo.ToTitleCase(t.Priority));
                                table.Cell().BorderBottom(0.5f).BorderColor("#E5E7EB").Padding(4).Text(t.DueDate.ToString("MMM dd, yyyy hh:mm tt"));
                                table.Cell().BorderBottom(0.5f).BorderColor("#E5E7EB").Padding(4).Text(statLabel);
                            }
                        });
                    });

                    // Footer
                    page.Footer().Row(row =>
                    {
                        row.RelativeItem().Text("CampusDeadline — Student Academic Productivity OS").FontSize(8).FontColor("#9CA3AF");
                        row.RelativeItem().AlignRight().Text(text =>
                        {
                            text.Span("Page ");
                            text.CurrentPageNumber();
                            text.Span(" of ");
                            text.TotalPages();
                        });
                    });
                });
            });

            return await Task.FromResult(document.GeneratePdf());
        }
    }
}
