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
        private readonly ICategoryService _categoryService;

        public CategoriesController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

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
        [Authorize(Policy = "StaffOnly")]
        public async Task<IActionResult> CreatePartial()
        {
            var parentCategories = await _categoryService.GetActiveCategoriesAsync();
            ViewBag.ParentCategoryId = new SelectList(parentCategories, "CategoryId", "CategoryName");

            return PartialView("CreatePartial", new Category());
        }


        // POST: Categories/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePartial(
    string CategoryName,
    string CategoryDesciption,
    short? ParentCategoryId,
    bool? IsActive)
        {
            if (string.IsNullOrWhiteSpace(CategoryName) || string.IsNullOrWhiteSpace(CategoryDesciption))
            {
                ModelState.AddModelError("", "Tên danh mục và mô tả không được để trống.");
            }

            if (ModelState.IsValid)
            {
                var category = new Category
                {
                    CategoryName = CategoryName,
                    CategoryDesciption = CategoryDesciption,
                    ParentCategoryId = ParentCategoryId,
                    IsActive = IsActive
                };

                await _categoryService.AddCategoryAsync(category);
                return RedirectToAction(nameof(Index));
            }

            var parentCategories = await _categoryService.GetAllCategoriesAsync();
            ViewBag.ParentCategoryId = new SelectList(parentCategories, "CategoryId", "CategoryName", ParentCategoryId);

            return PartialView("CreatePartial");
        }


        // GET: Categories/Edit/5
        public async Task<IActionResult> EditPartial(short id)
        {
            var category = await _categoryService.GetCategoryByIdAsync(id);
            var parentCategories = await _categoryService.GetActiveCategoriesAsync();
            ViewBag.ParentCategoryId = new SelectList(parentCategories, "CategoryId", "CategoryName");
            if (category == null)
            {
                return NotFound();
            }
            return PartialView("EditPartial", category);
        }

        // POST: Categories/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "StaffOnly")]
        public async Task<IActionResult> EditPartial(
    short id,
    string CategoryName,
    string CategoryDesciption,
    short? ParentCategoryId,
    bool? IsActive)
        {
            // Lấy danh mục từ database
            var category = await _categoryService.GetCategoryByIdAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            // Cập nhật thông tin danh mục
            category.CategoryName = CategoryName;
            category.CategoryDesciption = CategoryDesciption;
            category.ParentCategoryId = ParentCategoryId;
            category.IsActive = IsActive;

            // Lưu cập nhật vào database
            await _categoryService.UpdateCategoryAsync(category);
            return RedirectToAction(nameof(Index));
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
                return Json(new { success = true });
            }
            catch (InvalidOperationException ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}
