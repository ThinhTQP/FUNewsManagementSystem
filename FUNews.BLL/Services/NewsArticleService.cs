using FUNews.BLL.UnitOfWorks;
using BusinessObjects.Entities;
using FUNews.DAL.Repositories;
using Microsoft.EntityFrameworkCore;

public class NewsArticleService : INewsArticleService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IGenericRepository<NewsArticle, string> _newsArticleRepository;
    private readonly IGenericRepository<Tag, int> _tagRepository;

    public NewsArticleService(IGenericRepository<NewsArticle, string> newsArticleRepository,
                              IGenericRepository<Tag, int> tagRepository,
                              IUnitOfWork unitOfWork)
    {
        _newsArticleRepository = newsArticleRepository;
        _tagRepository = tagRepository;
        _unitOfWork = unitOfWork;
    }

    // Lấy tất cả bài viết
    public async Task<IEnumerable<NewsArticle>> GetAllNewsArticlesAsync()
    {
        return await _newsArticleRepository
       .FindAll(null, a => a.Tags, a => a.Category, a => a.CreatedBy)
       .ToListAsync();
    }

    // Lấy bài viết theo ID
    public async Task<NewsArticle?> GetNewsArticleByIdAsync(string id)
    {
        return await _newsArticleRepository.FindById(id, "NewsArticleId", a => a.Tags, a => a.Category, a => a.CreatedBy);
    }

    // Thêm bài viết mới + Tags
    public async Task AddNewsArticleAsync(NewsArticle newsArticle, List<int> tagIds)
    {
        if (newsArticle == null)
            throw new ArgumentNullException(nameof(newsArticle));

        if (tagIds == null || !tagIds.Any())
            throw new ArgumentException("TagIds is null or empty.");

        var tags = await _tagRepository.FindAll(t => tagIds.Contains(t.TagId)).ToListAsync();

        if (!tags.Any())
            throw new Exception("Không tìm thấy tag nào tương ứng với TagIds.");

        newsArticle.Tags = tags;

        _newsArticleRepository.Create(newsArticle);
        await _unitOfWork.SaveChange();
    }


    // Cập nhật bài viết + Tags
    public async Task UpdateNewsArticleAsync(NewsArticle newsArticle, List<int> tagIds)
    {
        if (newsArticle == null)
            throw new ArgumentNullException(nameof(newsArticle));

        var existingArticle = await _newsArticleRepository.FindById(newsArticle.NewsArticleId, "NewsArticleId", a => a.Tags);
        if (existingArticle == null)
            throw new ArgumentException("News article not found");

        // Cập nhật thông tin bài viết
     
        existingArticle.NewsTitle = newsArticle.NewsTitle;
        existingArticle.Headline = newsArticle.Headline;
        existingArticle.NewsContent = newsArticle.NewsContent;
        existingArticle.CategoryId = newsArticle.CategoryId;
        existingArticle.NewsStatus = newsArticle.NewsStatus;

        //  Gán lại UpdatedById và ModifiedDate
        existingArticle.UpdatedById = newsArticle.UpdatedById;
        existingArticle.ModifiedDate = DateTime.Now;

        // Lấy danh sách tags từ ID
        var tags = await _tagRepository.FindAll(t => tagIds.Contains(t.TagId)).ToListAsync();

        // Xóa tags cũ và cập nhật tags mới
        existingArticle.Tags.Clear();
        existingArticle.Tags = tags;

        _newsArticleRepository.Update(existingArticle);
        await _unitOfWork.SaveChange();
    }

    // Xóa bài viết
    public async Task DeleteNewsArticleAsync(string id)
    {
        var newsArticle = await _newsArticleRepository.FindById(id, "NewsArticleId", a => a.Tags);
        if (newsArticle == null)
            throw new ArgumentException("News article not found");
        newsArticle.Tags.Clear();
        _newsArticleRepository.Update(newsArticle);
        await _unitOfWork.SaveChange();
        _newsArticleRepository.Delete(newsArticle);
        await _unitOfWork.SaveChange();
    }
    public async Task UpdateNewsArticleAsync(NewsArticle newsArticle)
    {
        _newsArticleRepository.Update(newsArticle);
        await _unitOfWork.SaveChange();
    }
    // Lấy bài viết cho Lecturer Active 
    public async Task<IEnumerable<NewsArticle>> GetActiveNewsForLecturerAsync()
    {
        return await _newsArticleRepository
            .FindAll(a => a.NewsStatus == true, a => a.Tags, a => a.Category, a => a.CreatedBy)
            .ToListAsync();
    }


}
