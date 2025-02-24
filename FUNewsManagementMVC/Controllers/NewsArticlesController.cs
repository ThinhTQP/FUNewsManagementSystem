using FUNews.BLL.Services;
using BusinessObjects.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;
using System.Threading.Tasks;

namespace FUNews.MVC.Controllers
{

    public class NewsArticlesController : Controller
    {
        private readonly INewsArticleService _newsArticleService;
        private readonly ICategoryService _categoryService;
        private readonly ITagService _tagService;

        public NewsArticlesController(INewsArticleService newsArticleService, ICategoryService categoryService, ITagService tagService)
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

        //search
        public async Task<IActionResult> Search(string searchString)
        {
            var articles = await _newsArticleService.GetAllNewsArticlesAsync();

            if (!string.IsNullOrEmpty(searchString))
            {
                articles = articles.Where(a => a.NewsTitle.Contains(searchString, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            return View("Index", articles);
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

        public async Task<IActionResult> CreatePartial(
    string? NewsTitle,
    string Headline,
    string? NewsSource,
    string? NewsContent,
    short? CategoryId,
    bool NewsStatus,
    [FromForm] List<int> TagIds)
        {
            var newsArticle = new NewsArticle
            {
                NewsArticleId = Guid.NewGuid().ToString("N").Substring(0, 20), 
                NewsTitle = NewsTitle,
                Headline = Headline,
                NewsSource = NewsSource,
                NewsContent = NewsContent,
                CategoryId = CategoryId,
                NewsStatus = NewsStatus,
                CreatedDate = DateTime.Now,
                ModifiedDate = DateTime.Now
            };

            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(userIdValue) && short.TryParse(userIdValue, out short staffId))
            {
                newsArticle.CreatedById = staffId;
                newsArticle.UpdatedById = staffId;
            }

            await _newsArticleService.AddNewsArticleAsync(newsArticle, TagIds);

            return RedirectToAction(nameof(Index));
        }



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
        public async Task<IActionResult> EditPartial(
    string id,
    string? NewsTitle,
    string Headline,
    string? NewsSource,
    string? NewsContent,
    short? CategoryId,
    bool NewsStatus,
    [FromForm] List<int> TagIds)
        {
            var newsArticle = await _newsArticleService.GetNewsArticleByIdAsync(id);
            if (newsArticle == null)
            {
                return NotFound();
            }

            if (id != newsArticle.NewsArticleId)
            {
                return NotFound();
            }

            newsArticle.NewsTitle = NewsTitle;
            newsArticle.Headline = Headline;
            newsArticle.NewsSource = NewsSource;
            newsArticle.NewsContent = NewsContent;
            newsArticle.CategoryId = CategoryId;
            newsArticle.NewsStatus = NewsStatus;

            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(userIdValue) && short.TryParse(userIdValue, out short staffId))
            {
                newsArticle.UpdatedById = staffId;
            }

            newsArticle.ModifiedDate = DateTime.Now;

            Console.WriteLine($"[DEBUG] ID: {id}");
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
