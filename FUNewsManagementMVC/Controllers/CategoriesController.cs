using FUNews.BLL.Services;
using FUNews.DAL.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Threading.Tasks;

namespace FUNews.MVC.Controllers
{
    [Authorize(Policy = "StaffOnly")]
    public class CategoriesController : Controller
    {
        private readonly CategoryService _categoryService;

        public CategoriesController(CategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        // GET: Categories
        [Authorize(Policy = "StaffOnly")]
        public async Task<IActionResult> Index()
        {
            var categories = await _categoryService.GetAllCategoriesAsync();
            return View(categories);
        }

        // GET: Categories/Details/5
        public async Task<IActionResult> Details(short id)
        {
            var category = await _categoryService.GetCategoryByIdAsync(id);
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }

        // GET: Categories/Create
        public async Task<IActionResult> Create()
        {
            // Lấy tất cả các danh mục từ service để hiển thị trong dropdown
            var parentCategories = await _categoryService.GetAllCategoriesAsync();
            // Gán danh sách vào ViewBag để hiển thị trong select list
            ViewBag.ParentCategoryId = new SelectList(parentCategories, "CategoryId", "CategoryName");

            return View();
        }


        // POST: Categories/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CategoryId,CategoryName,CategoryDesciption,ParentCategoryId,IsActive")] Category category)
        {
            if (ModelState.IsValid)
            {
                await _categoryService.AddCategoryAsync(category);
                return RedirectToAction(nameof(Index));
            }
            // Nếu có lỗi, truyền lại danh sách ParentCategoryId vào ViewBag để giữ giá trị trong dropdown
            var parentCategories = await _categoryService.GetAllCategoriesAsync();
            ViewBag.ParentCategoryId = new SelectList(parentCategories, "CategoryId", "CategoryName", category.ParentCategoryId);
            return View(category);
        }

        // GET: Categories/Edit/5
        public async Task<IActionResult> Edit(short id)
        {
            var category = await _categoryService.GetCategoryByIdAsync(id);
            var parentCategories = await _categoryService.GetAllCategoriesAsync();
            ViewBag.ParentCategoryId = new SelectList(parentCategories, "CategoryId", "CategoryName");
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }

        // POST: Categories/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(short id, [Bind("CategoryId,CategoryName,CategoryDesciption,ParentCategoryId,IsActive")] Category category)
        {
            if (id != category.CategoryId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                await _categoryService.UpdateCategoryAsync(category);
                return RedirectToAction(nameof(Index));
            }
            var parentCategories = await _categoryService.GetAllCategoriesAsync();
            ViewBag.ParentCategoryId = new SelectList(parentCategories, "CategoryId", "CategoryName", category.ParentCategoryId);
            return View(category);
        }

        // GET: Categories/Delete/5
        public async Task<IActionResult> Delete(short id)
        {
            var category = await _categoryService.GetCategoryByIdAsync(id);
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }

        // POST: Categories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(short id)
        {
            try
            {
                await _categoryService.DeleteCategoryAsync(id);
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
            catch (ArgumentException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
