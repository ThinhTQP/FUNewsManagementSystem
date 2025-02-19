using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FUNews.DAL;
using FUNews.DAL.Entities;
using FUNews.BLL.Services;
using Microsoft.AspNetCore.Authorization;

namespace FUNewsManagementMVC.Controllers
{
    [Authorize(Policy = "StaffOnly")]
    public class TagsController : Controller
    {
        private readonly ITagService _tagService;

        public TagsController(ITagService tagService)
        {
            _tagService = tagService;
        }

        // GET: Tags
        public async Task<IActionResult> Index()
        {
            var tags = await _tagService.GetAllTagsAsync();
            return View(tags);
        }

        // GET: Tags/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var tag = await _tagService.GetTagByIdAsync(id);
            if (tag == null)
            {
                return NotFound();
            }
            return View(tag);
        }

        // GET: Tags/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Tags/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TagId,TagName,Note")] Tag tag)
        {

            if (ModelState.IsValid)
            {
                // Lấy TagId lớn nhất và +1
                var maxId = (await _tagService.GetAllTagsAsync()).Max(t => (int?)t.TagId) ?? 0;
                tag.TagId = maxId + 1;

                await _tagService.AddTagAsync(tag);
                return RedirectToAction(nameof(Index));
            }
            return View(tag);
        }

        // GET: Tags/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var tag = await _tagService.GetTagByIdAsync(id);
            if (tag == null)
            {
                return NotFound();
            }
            return View(tag);
        }

        // POST: Tags/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("TagId,TagName,Note")] Tag tag)
        {
            if (id != tag.TagId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                await _tagService.UpdateTagAsync(tag);
                return RedirectToAction(nameof(Index));
            }
                
            return View(tag);
        }

        // GET: Tags/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var tag = await _tagService.GetTagByIdAsync(id);
            if (tag == null)
            {
                return NotFound();
            }
            return View(tag);
           
        }

        // POST: Tags/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                await _tagService.DeleteTagAsync(id);
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
