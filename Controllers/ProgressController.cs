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
    public class ProgressController : Controller
    {
        private readonly IAnalyticsService _analyticsService;
        private readonly ITaskService _taskService;
        private readonly IPdfReportService _pdfReportService;

        public ProgressController(
            IAnalyticsService analyticsService,
            ITaskService taskService,
            IPdfReportService pdfReportService)
        {
            _analyticsService = analyticsService;
            _taskService = taskService;
            _pdfReportService = pdfReportService;
        }

        [HttpGet]
        [Route("progress")]
        public async Task<IActionResult> Index(string period = "all")
        {
            int userId = User.GetUserId();

            var analytics = await _analyticsService.GetProgressAnalyticsAsync(userId, period);
            var subjects = await _taskService.GetUserSubjectsAsync(userId);

            var viewModel = new ProgressViewModel
            {
                Analytics = analytics,
                CurrentPeriod = period
            };

            ViewBag.GlobalSubjects = subjects;
            ViewBag.ActiveNav = "progress";

            return View(viewModel);
        }

        [HttpGet]
        [Route("api/progress")]
        public async Task<IActionResult> ApiProgress(string period = "all")
        {
            int userId = User.GetUserId();
            var analytics = await _analyticsService.GetProgressAnalyticsAsync(userId, period);
            return Json(analytics);
        }

        [HttpGet]
        [Route("progress/export.pdf")]
        public async Task<IActionResult> ExportPdf()
        {
            int userId = User.GetUserId();
            var pdfBytes = await _pdfReportService.GenerateAcademicReportPdfAsync(userId);

            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string filename = $"CampusDeadline_Academic_Report_{timestamp}.pdf";

            return File(pdfBytes, "application/pdf", filename);
        }
    }
}
