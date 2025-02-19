using FUNews.BLL.Services;
using FUNews.DAL.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;
using System.Threading.Tasks;

namespace FUNews.MVC.Controllers
{

    public class NewsArticlesController : Controller
    {
        private readonly NewsArticleService _newsArticleService;
        private readonly CategoryService _categoryService;
        private readonly TagService _tagService;

        public NewsArticlesController(NewsArticleService newsArticleService, CategoryService categoryService, TagService tagService)
        {
            _newsArticleService = newsArticleService;
            _categoryService = categoryService;
            _tagService = tagService;
        }

        // GET: NewsArticles
        [Authorize(Policy = "StaffOnly")]
        public async Task<IActionResult> Index()
        {
            var articles = await _newsArticleService.GetAllNewsArticlesAsync();
            return View(articles);
        }

        // GET: NewsArticles/Details/5
        [Authorize(Policy = "StaffOnly")]
        public async Task<IActionResult> Details(string id)
        {
            var article = await _newsArticleService.GetNewsArticleByIdAsync(id);
            if (article == null)
            {
                return NotFound();
            }
            return View(article);
        }

        // GET: NewsArticles/Create

        [Authorize(Policy = "StaffOnly")]
        public async Task<IActionResult> CreatePartial()
        {
            // Fetch categories and tags for the select lists
            ViewBag.CategoryId = new SelectList(await _categoryService.GetActiveCategoriesAsync(), "CategoryId", "CategoryName");
            var tags = await _tagService.GetAllTagsAsync();
            ViewBag.TagIds = new MultiSelectList(tags, "TagId", "TagName");

            // Return the partial view with an empty NewsArticle
            return PartialView("CreatePartial", new NewsArticle());
        }

        // POST: NewsArticles/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "StaffOnly")]
        public async Task<IActionResult> CreatePartial([Bind("NewsArticleId,NewsTitle,Headline,NewsSource,NewsContent,CategoryId,NewsStatus,CreatedById,UpdatedById,CreatedDate,ModifiedDate")] NewsArticle newsArticle, [FromForm] List<int> TagIds)
        {


            if (string.IsNullOrEmpty(newsArticle.NewsArticleId))
            {
                newsArticle.NewsArticleId = Guid.NewGuid().ToString("N").Substring(0, 20); // Generate a 20-character GUID
            }

            // Set created and updated user IDs from claims
            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(userIdValue) && short.TryParse(userIdValue, out short staffId))
            {
                newsArticle.CreatedById = staffId;
                newsArticle.UpdatedById = staffId;
            }

            // Set creation and modification timestamps
            newsArticle.CreatedDate = DateTime.Now;
            newsArticle.ModifiedDate = DateTime.Now;

            // Save the article and its tags
            await _newsArticleService.AddNewsArticleAsync(newsArticle, TagIds);



            // Return the updated view
            return RedirectToAction(nameof(Index));
        }


        // GET: NewsArticles/Edit/5
        [Authorize(Policy = "StaffOnly")]
        public async Task<IActionResult> EditPartial(string id)
        {
            var article = await _newsArticleService.GetNewsArticleByIdAsync(id);
            if (article == null)
            {
                return NotFound();
            }

            var selectedTagIds = article.Tags?.Select(t => t.TagId).ToList() ?? new List<int>();
            ViewBag.SelectedTagIds = selectedTagIds;

            ViewBag.CategoryId = new SelectList(
                await _categoryService.GetActiveCategoriesAsync(),
                "CategoryId",
                "CategoryName",
                article.CategoryId);

            var allTags = await _tagService.GetAllTagsAsync();
            ViewBag.TagIds = allTags
                .Select(t => new { Value = t.TagId, Text = t.TagName })
                .ToList();

            return PartialView("EditPartial", article);
        }


        // POST: NewsArticles/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "StaffOnly")]
        public async Task<IActionResult> EditPartial(string id, [Bind("NewsArticleId,NewsTitle,Headline,NewsSource,NewsContent,CategoryId,NewsStatus,CreatedById,UpdatedById,CreatedDate,ModifiedDate")] NewsArticle newsArticle, [FromForm] List<int> TagIds)
        {
            Console.WriteLine($"TagIds count: {TagIds?.Count ?? 0}");
            if (id != newsArticle.NewsArticleId)
            {
                return NotFound();
            }
            if (newsArticle == null)
            {
                return NotFound(); // Nếu null, trả về NotFound
            }
     


            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(userIdValue) && short.TryParse(userIdValue, out short staffId))
            {
                newsArticle.UpdatedById = staffId;
            }

            newsArticle.ModifiedDate = DateTime.Now;

            Console.WriteLine($"[DEBUG] ID: {id}");
            Console.WriteLine($"[DEBUG] NewsArticleId: {newsArticle.NewsArticleId}");
            Console.WriteLine($"[DEBUG] NewsTitle: {newsArticle.NewsTitle}");
            Console.WriteLine($"[DEBUG] Headline: {newsArticle.Headline}");
            Console.WriteLine($"[DEBUG] CategoryId: {newsArticle.CategoryId}");
            Console.WriteLine($"[DEBUG] TagIds Count: {TagIds?.Count ?? 0}");
            Console.WriteLine($"[DEBUG] TagIds: {string.Join(", ", TagIds ?? new List<int>())}");


            await _newsArticleService.UpdateNewsArticleAsync(newsArticle, TagIds);
            return RedirectToAction(nameof(Index));
        }


        // GET: NewsArticles/Delete/5
        [Authorize(Policy = "StaffOnly")]
        public async Task<IActionResult> Delete(string id)
        {
            var article = await _newsArticleService.GetNewsArticleByIdAsync(id);
            if (article == null)
            {
                return NotFound();
            }
            return View(article);
        }

        // POST: NewsArticles/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "StaffOnly")]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var newsArticle = await _newsArticleService.GetNewsArticleByIdAsync(id);
            if (newsArticle == null)
            {
                return NotFound();
            }

            // Cập nhật trạng thái thành Inactive
            newsArticle.NewsStatus = false;
            await _newsArticleService.UpdateNewsArticleAsync(newsArticle);

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Policy = "LecturerOnly")]
        public async Task<IActionResult> LecturerNews()
        {
            var news = await _newsArticleService.GetActiveNewsForLecturerAsync();
            return View(news);
        }
        [Authorize(Policy = "LecturerOnly")]
        public async Task<IActionResult> LecturerNewDetails(string id)
        {
            var article = await _newsArticleService.GetNewsArticleByIdAsync(id);
            if (article == null)
            {
                return NotFound();
            }

            return View(article);
        }
    }
}
