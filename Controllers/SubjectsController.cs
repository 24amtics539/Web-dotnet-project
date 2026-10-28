using System.Threading.Tasks;
using CampusDeadline.Extensions;
using CampusDeadline.Models.ViewModels;
using CampusDeadline.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CampusDeadline.Controllers
{
    [Authorize]
    public class SubjectsController : Controller
    {
        private readonly ITaskService _taskService;

        public SubjectsController(ITaskService taskService)
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
        [Route("subjects")]
        public async Task<IActionResult> Index(string? search = null, string? semester = null)
        {
            int userId = User.GetUserId();

            var subjects = await _taskService.GetUserSubjectsAsync(userId, search, semester);
            var allSubjects = await _taskService.GetUserSubjectsAsync(userId);

            var viewModel = new SubjectsListViewModel
            {
                Subjects = subjects,
                CurrentSearch = search,
                CurrentSemester = semester
            };

            ViewBag.GlobalSubjects = allSubjects;
            ViewBag.ActiveNav = "subjects";

            return View(viewModel);
        }

        [HttpPost]
        [Route("subjects/create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SubjectFormViewModel model)
        {
            int userId = User.GetUserId();

            var (subjectId, error) = await _taskService.CreateSubjectAsync(userId, model);

            if (!string.IsNullOrEmpty(error))
            {
                if (IsAjaxRequest())
                    return BadRequest(new { success = false, error });

                TempData["Error"] = error;
                return RedirectToAction(nameof(Index));
            }

            if (IsAjaxRequest())
            {
                var created = await _taskService.GetSubjectByIdAsync(subjectId, userId);
                return Json(new { success = true, message = "Subject added successfully.", subject = created });
            }

            TempData["Success"] = "Subject added successfully!";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        [Route("subjects/{id:int}")]
        public async Task<IActionResult> Details(int id)
        {
            int userId = User.GetUserId();
            var subject = await _taskService.GetSubjectByIdAsync(id, userId);

            if (subject == null)
            {
                return NotFound(new { success = false, error = "Subject not found." });
            }

            return Json(new { success = true, subject });
        }

        [HttpPost]
        [Route("subjects/{id:int}/edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, SubjectFormViewModel model)
        {
            int userId = User.GetUserId();

            var (success, error) = await _taskService.UpdateSubjectAsync(id, userId, model);

            if (!success)
            {
                if (IsAjaxRequest())
                    return BadRequest(new { success = false, error });

                TempData["Error"] = error ?? "Failed to update subject.";
                return RedirectToAction(nameof(Index));
            }

            if (IsAjaxRequest())
            {
                var updated = await _taskService.GetSubjectByIdAsync(id, userId);
                return Json(new { success = true, message = "Subject updated successfully.", subject = updated });
            }

            TempData["Success"] = "Subject updated successfully!";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [Route("subjects/{id:int}/delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            int userId = User.GetUserId();
            var (success, error) = await _taskService.DeleteSubjectAsync(id, userId);

            if (!success)
            {
                if (IsAjaxRequest())
                    return BadRequest(new { success = false, error });

                TempData["Error"] = error ?? "Failed to delete subject.";
                return RedirectToAction(nameof(Index));
            }

            if (IsAjaxRequest())
            {
                return Json(new { success = true, message = "Subject and tasks deleted successfully." });
            }

            TempData["Info"] = "Subject deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}
