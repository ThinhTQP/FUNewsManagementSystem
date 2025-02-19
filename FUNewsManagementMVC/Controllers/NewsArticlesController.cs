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
        public async Task<IActionResult> Create()
        {
            ViewBag.CategoryId = new SelectList(await _categoryService.GetAllCategoriesAsync(), "CategoryId", "CategoryName");
            ViewBag.TagIds = new MultiSelectList(await _tagService.GetAllTagsAsync(), "TagId", "TagName");
            return View();
        }

        // POST: NewsArticles/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "StaffOnly")]
        public async Task<IActionResult> Create([Bind("NewsArticleId,NewsTitle,Headline,NewsSource,NewsContent,CategoryId,NewsStatus,CreatedById,UpdatedById,CreatedDate,ModifiedDate")] NewsArticle newsArticle, [FromForm]  List<int> TagIds)
        {
            if (string.IsNullOrEmpty(newsArticle.NewsArticleId))
            {
                newsArticle.NewsArticleId = Guid.NewGuid().ToString("N").Substring(0, 20); // 20 ký tự
            }


            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
            Console.WriteLine($"UserId Claim: {userIdValue}");

            if (!string.IsNullOrEmpty(userIdValue) && short.TryParse(userIdValue, out short staffId))
            {
                newsArticle.CreatedById = staffId;
                newsArticle.UpdatedById = staffId;
                Console.WriteLine($"Assigned CreatedById: {staffId}");
            }
            else
            {
                Console.WriteLine("Failed to parse AccountId from claim.");
            }

        

        newsArticle.CreatedDate = DateTime.Now;
            newsArticle.ModifiedDate = DateTime.Now;

            await _newsArticleService.AddNewsArticleAsync(newsArticle, TagIds);
            return RedirectToAction(nameof(Index));
        }

        // GET: NewsArticles/Edit/5
        [Authorize(Policy = "StaffOnly")]
        public async Task<IActionResult> Edit(string id)
        {
            var article = await _newsArticleService.GetNewsArticleByIdAsync(id);
            if (article == null)
            {
                return NotFound();
            }

            ViewBag.CategoryId = new SelectList(
                await _categoryService.GetAllCategoriesAsync(),
                "CategoryId",
                "CategoryName",
                article.CategoryId);

            ViewBag.TagIds = new MultiSelectList(
                await _tagService.GetAllTagsAsync(),
                "TagId",
                "TagName",
                article.Tags.Select(t => t.TagId));

            return View(article);
        }

        // POST: NewsArticles/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "StaffOnly")]
        public async Task<IActionResult> Edit(
    string id,
    [Bind("NewsArticleId,NewsTitle,Headline,NewsSource,NewsContent,CategoryId,NewsStatus,CreatedById,UpdatedById,CreatedDate,ModifiedDate")] NewsArticle newsArticle,
    [FromForm] List<int> TagIds)
        {
            if (id != newsArticle.NewsArticleId)
            {
                return NotFound();
            }

            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(userIdValue) && short.TryParse(userIdValue, out short staffId))
            {
                newsArticle.UpdatedById = staffId;
            }

            newsArticle.ModifiedDate = DateTime.Now;

            if (ModelState.IsValid)
            {
                await _newsArticleService.UpdateNewsArticleAsync(newsArticle, TagIds);
                return RedirectToAction(nameof(Index));
            }

            ViewBag.CategoryId = new SelectList(
                await _categoryService.GetAllCategoriesAsync(),
                "CategoryId",
                "CategoryName",
                newsArticle.CategoryId);

            ViewBag.TagIds = new MultiSelectList(
                await _tagService.GetAllTagsAsync(),
                "TagId",
                "TagName",
                TagIds);

            return View(newsArticle);
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
            await _newsArticleService.DeleteNewsArticleAsync(id);
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
