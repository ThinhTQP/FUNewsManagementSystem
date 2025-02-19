using Azure;
using FUNews.BLL.Services;
using FUNews.DAL.Entities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;

namespace FUNewsManagementMVC.Controllers
{
    [Authorize]
    public class SystemAccountsController : Controller
    {
        private readonly ISystemAccountService _accountService;
        private readonly IConfiguration _config;

        public SystemAccountsController(ISystemAccountService accountService, IConfiguration config)
        {
            _accountService = accountService;
            _config = config;
        }

        // ======== AUTHENTICATION ========

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity.IsAuthenticated)
            {
                // Lấy role của user để điều hướng chính xác
                var role = User.FindFirst(ClaimTypes.Role)?.Value;
                return role switch
                {
                    "Admin" => RedirectToAction("Index", "SystemAccounts"),
                    "Staff" => RedirectToAction("Index", "Categories"),
                    "Lecturer" => RedirectToAction("Index", "Lecturer"),
                    _ => RedirectToAction("Index", "Home")
                };
            }
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            var defaultAdminEmail = _config["DefaultAccount:Email"];
            var defaultAdminPassword = _config["DefaultAccount:Password"];

            if (email == defaultAdminEmail && password == defaultAdminPassword)
            {
                var adminClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, "Default Admin"),
                    new Claim(ClaimTypes.Email, defaultAdminEmail),
                    new Claim(ClaimTypes.Role, "Admin")
                };

                var adminIdentity = new ClaimsIdentity(adminClaims, CookieAuthenticationDefaults.AuthenticationScheme);
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(adminIdentity));
                return RedirectToAction("Index", "SystemAccounts");
            }

            var user = (await _accountService.GetAllSystemAccountsAsync())
                .FirstOrDefault(u => u.AccountEmail == email && u.AccountPassword == password);

            if (user == null)
            {
                ViewBag.Error = "Invalid credentials!";
                return View();
            }

            var role = user.AccountRole switch
            {
                1 => "Staff",
                2 => "Lecturer",
                _ => "Lecturer"
            };

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.AccountId.ToString()),
                new Claim(ClaimTypes.Name, user.AccountName ?? ""),
                new Claim(ClaimTypes.Email, user.AccountEmail ?? ""),
                new Claim(ClaimTypes.Role, role)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

            return role switch
            {
                "Staff" => RedirectToAction("Index", "NewsArticles"),
                "Lecturer" => RedirectToAction("LecturerNews", "NewsArticles"),
                _ => RedirectToAction("LecturerNews", "NewsArticles")
            };
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "SystemAccounts");
        }

        [AllowAnonymous]
        public IActionResult AccessDenied() => View();

        // ======== ACCOUNT MANAGEMENT (CRUD) ========
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Index()
        {
            var accounts = await _accountService.GetAllSystemAccountsAsync();
            return View(accounts);
        }

        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Details(short id)
        {
            var account = await _accountService.GetSystemAccountByIdAsync(id);
            if (account == null)
                return NotFound();
            return View(account);
        }

        [Authorize(Policy = "AdminOnly")]
        public IActionResult CreatePartial()
        {
            return PartialView("CreatePartial", new SystemAccount());
        }

        [Authorize(Policy = "AdminOnly")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePartial(SystemAccount systemAccount)
        {
            if (ModelState.IsValid)
            {
                var maxId = (await _accountService.GetAllSystemAccountsAsync())
                        .Max(t => (short?)t.AccountId) ?? 0;
                systemAccount.AccountId = (short)(maxId + 1);
                await _accountService.AddSystemAccountAsync(systemAccount);
                return RedirectToAction(nameof(Index)); // ✅ Redirect khi thành công
            }

            return PartialView("CreatePartial", systemAccount); // ❌ Hiện lại form khi có lỗi
        }

        [HttpGet]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> EditPartial(short id)
        {
            var account = await _accountService.GetSystemAccountByIdAsync(id);
            if (account == null) return NotFound();
            return PartialView("EditPartial", account);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> EditPartial(SystemAccount systemAccount)
        {
            if (ModelState.IsValid)
            {
                await _accountService.UpdateSystemAccountAsync(systemAccount);
                return RedirectToAction(nameof(Index)); // ✅ Redirect khi thành công
            }
            return PartialView("EditPartial", systemAccount); // ❌ Hiện form lại khi lỗi
        }


        [Authorize(Policy = "AdminOnly")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(short id)
        {
            await _accountService.DeleteSystemAccountAsync(id);
            return RedirectToAction(nameof(Index));
        }



        [Authorize]
        public async Task<IActionResult> EditProfile()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !short.TryParse(userIdClaim, out short userId))
            {
                return Unauthorized();
            }

            var account = await _accountService.GetSystemAccountByIdAsync(userId);
            if (account == null) return NotFound();

            return View("EditProfile", account);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> EditProfile(SystemAccount systemAccount, string OldPassword, string NewPassword)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !short.TryParse(userIdClaim, out short userId))
            {
                return Unauthorized();
            }

            if (userId != systemAccount.AccountId) return Forbid();

            var existingAccount = await _accountService.GetSystemAccountByIdAsync(userId);
            if (existingAccount == null) return NotFound();

            // Kiểm tra nếu người dùng nhập mật khẩu cũ và mới
            if (!string.IsNullOrEmpty(OldPassword) && !string.IsNullOrEmpty(NewPassword))
            {
                if (existingAccount.AccountPassword != OldPassword)
                {
                    TempData["ErrorMessage"] = "Old password is incorrect!";
                    return View("EditProfile", systemAccount);
                }

                // Cập nhật mật khẩu mới
                existingAccount.AccountPassword = NewPassword;
            }

            // Cập nhật thông tin tài khoản khác (trừ Email và Role)
            existingAccount.AccountName = systemAccount.AccountName;

            await _accountService.UpdateSystemAccountAsync(existingAccount);
            TempData["SuccessMessage"] = "Profile updated successfully!";

            return RedirectToAction("EditProfile");
        }
    }
}
