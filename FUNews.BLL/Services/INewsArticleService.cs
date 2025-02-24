using System;
using System.Linq;
using System.Text;
using BusinessObjects.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

public interface INewsArticleService
{
    Task<IEnumerable<NewsArticle>> GetAllNewsArticlesAsync();
    Task<NewsArticle?> GetNewsArticleByIdAsync(string id);
    Task AddNewsArticleAsync(NewsArticle newsArticle, List<int> tagIds);
    Task UpdateNewsArticleAsync(NewsArticle newsArticle, List<int> tagIds);
    Task DeleteNewsArticleAsync(string id);
    Task UpdateNewsArticleAsync(NewsArticle newsArticle);
    Task<IEnumerable<NewsArticle>> GetActiveNewsForLecturerAsync();
}
