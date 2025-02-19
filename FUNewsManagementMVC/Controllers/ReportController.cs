using FUNews.BLL.Services;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FUNewsManagementMVC.Controllers
{
    public class ReportController : Controller
    {
        private readonly INewsArticleService _newsArticleService;

        public ReportController(INewsArticleService newsArticleService)
        {
            _newsArticleService = newsArticleService;
        }

        public async Task<IActionResult> Index()
        {
            var articles = await _newsArticleService.GetAllNewsArticlesAsync();
            return View(articles);
        }

        public async Task<IActionResult> Filter(DateTime? startDate, DateTime? endDate)
        {
            var articles = await _newsArticleService.GetAllNewsArticlesAsync();

            if (startDate.HasValue)
                articles = articles.Where(a => a.ModifiedDate >= startDate.Value).ToList();
            if (endDate.HasValue)
                articles = articles.Where(a => a.ModifiedDate <= endDate.Value).ToList();

            articles = articles.OrderByDescending(a => a.ModifiedDate).ToList();

            return View("Index", articles);
        }

        public async Task<IActionResult> ExportToPDF(DateTime? startDate, DateTime? endDate)
        {
            var articles = await _newsArticleService.GetAllNewsArticlesAsync();

            if (startDate.HasValue)
                articles = articles.Where(a => a.ModifiedDate >= startDate.Value).ToList();
            if (endDate.HasValue)
                articles = articles.Where(a => a.ModifiedDate <= endDate.Value).ToList();

            using (MemoryStream stream = new MemoryStream())
            {
                Document pdfDoc = new Document(PageSize.A4);
                PdfWriter.GetInstance(pdfDoc, stream);
                pdfDoc.Open();
                pdfDoc.Add(new Paragraph("FPT University News Report"));
                pdfDoc.Add(new Paragraph("\n"));

                foreach (var article in articles)
                {
                    pdfDoc.Add(new Paragraph($"Title: {article.NewsTitle}"));
                    pdfDoc.Add(new Paragraph($"Headline: {article.Headline}"));
                    pdfDoc.Add(new Paragraph($"Content: {article.NewsContent.Substring(0, Math.Min(150, article.NewsContent.Length))}..."));
                    pdfDoc.Add(new Paragraph($"Date: {article.ModifiedDate?.ToString("dd/MM/yyyy")}"));
                    pdfDoc.Add(new Paragraph("------------------------------------------------------"));
                }

                pdfDoc.Close();
                return File(stream.ToArray(), "application/pdf", "NewsReport.pdf");
            }
        }

        public async Task<IActionResult> NewDetails(string id)
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
