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
        private readonly SystemAccountService _accountService;
        private readonly IConfiguration _config;

        public SystemAccountsController(SystemAccountService accountService, IConfiguration config)
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
        public IActionResult Create()
        {
       
            return View();
        }

        [Authorize(Policy = "AdminOnly")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SystemAccount systemAccount)
        {
            if (ModelState.IsValid)
            {
                var maxId = (await _accountService.GetAllSystemAccountsAsync())
                        .Max(t => (short?)t.AccountId) ?? 0;
                systemAccount.AccountId = (short)(maxId + 1);
                await _accountService.AddSystemAccountAsync(systemAccount);
                return RedirectToAction(nameof(Index));
            }
          
            return View(systemAccount);
        }

        [Authorize]
        public async Task<IActionResult> Edit(short id)
        {
            var account = await _accountService.GetSystemAccountByIdAsync(id);
            if (account == null)
                return NotFound();
            return View(account);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(short id, SystemAccount systemAccount)
        {
            if (id != systemAccount.AccountId)
                return BadRequest();

            if (ModelState.IsValid)
            {
                await _accountService.UpdateSystemAccountAsync(systemAccount);
                return RedirectToAction(nameof(Index));
            }
            return View(systemAccount);
        }

        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Delete(short id)
        {
            var account = await _accountService.GetSystemAccountByIdAsync(id);
            if (account == null)
                return NotFound();
            return View(account);
        }

        [Authorize(Policy = "AdminOnly")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(short id)
        {
            await _accountService.DeleteSystemAccountAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
