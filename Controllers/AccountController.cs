using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using CampusDeadline.Data;
using CampusDeadline.Models.Entities;
using CampusDeadline.Models.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CampusDeadline.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _db;

        public AccountController(ApplicationDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        [Route("login")]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Dashboard");
            }

            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        [HttpPost]
        [Route("login")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var cleanEmail = model.Email.Trim().ToLowerInvariant();
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == cleanEmail);

            if (user == null || !BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash))
            {
                ModelState.AddModelError(string.Empty, "Invalid email address or password. Please verify and try again.");
                TempData["Error"] = "Invalid email address or password. Please verify and try again.";
                return View(model);
            }

            // Create Authentication Claims
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Name, user.Name),
                new(ClaimTypes.Email, user.Email),
                new(ClaimTypes.Role, user.Role)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = model.RememberMe,
                ExpiresUtc = DateTimeOffset.UtcNow.AddDays(14)
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            // Set Session state
            HttpContext.Session.SetInt32("UserId", user.Id);
            HttpContext.Session.SetString("UserName", user.Name);
            HttpContext.Session.SetString("UserEmail", user.Email);
            HttpContext.Session.SetString("UserRole", user.Role);

            TempData["Success"] = $"Welcome back, {user.Name}!";

            if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
            {
                return Redirect(model.ReturnUrl);
            }

            return RedirectToAction("Index", "Dashboard");
        }

        [HttpGet]
        [Route("register")]
        public IActionResult Register()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Dashboard");
            }

            return View(new RegisterViewModel());
        }

        [HttpPost]
        [Route("register")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var cleanEmail = model.Email.Trim().ToLowerInvariant();
            bool exists = await _db.Users.AnyAsync(u => u.Email == cleanEmail);
            if (exists)
            {
                ModelState.AddModelError("Email", "An account with this email address already exists.");
                TempData["Error"] = "An account with this email address already exists.";
                return View(model);
            }

            var now = DateTime.Now;
            var user = new User
            {
                Name = model.Name.Trim(),
                Email = cleanEmail,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
                Role = "student",
                CreatedAt = now
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            // Give new student default semester courses
            var starterSubjects = new List<Subject>
            {
                new() { UserId = user.Id, SubjectName = "Data Structures & Algorithms", SubjectCode = "CS301", Semester = 5, Color = "#B98AFF", CreatedAt = now },
                new() { UserId = user.Id, SubjectName = "Database Management Systems", SubjectCode = "CS302", Semester = 5, Color = "#8B5CF6", CreatedAt = now },
                new() { UserId = user.Id, SubjectName = "Operating Systems", SubjectCode = "CS303", Semester = 5, Color = "#38BDF8", CreatedAt = now }
            };
            _db.Subjects.AddRange(starterSubjects);
            await _db.SaveChangesAsync();

            // Add first welcome task
            var firstTask = new TaskItem
            {
                UserId = user.Id,
                SubjectId = starterSubjects[0].Id,
                Title = "Complete Course Orientation & Syllabus Review",
                Description = "Welcome to CampusDeadline! Review upcoming course deadlines, add assignments, and track your study schedule.",
                TaskType = "Assignment",
                Priority = "medium",
                DueDate = now.AddDays(3).Date.AddHours(23).AddMinutes(59),
                Status = "pending",
                CreatedAt = now,
                UpdatedAt = now
            };
            _db.Tasks.Add(firstTask);
            await _db.SaveChangesAsync();

            // Sign in
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Name, user.Name),
                new(ClaimTypes.Email, user.Email),
                new(ClaimTypes.Role, user.Role)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                new AuthenticationProperties { IsPersistent = true });

            HttpContext.Session.SetInt32("UserId", user.Id);
            HttpContext.Session.SetString("UserName", user.Name);
            HttpContext.Session.SetString("UserEmail", user.Email);
            HttpContext.Session.SetString("UserRole", user.Role);

            TempData["Success"] = $"Welcome to CampusDeadline, {user.Name}! Your academic dashboard is ready.";
            return RedirectToAction("Index", "Dashboard");
        }

        [HttpPost]
        [Route("logout")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.Session.Clear();
            TempData["Info"] = "You have been signed out safely.";
            return RedirectToAction("Login", "Account");
        }
    }
}
