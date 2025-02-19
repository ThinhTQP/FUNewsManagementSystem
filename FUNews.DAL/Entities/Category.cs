using BookApp.Entities;
using System;
using System.Collections.Generic;

namespace FUNews.DAL.Entities;

public partial class Category : IEntity<short>
{
    public short CategoryId { get; set; }
    short IEntity<short>.Id => CategoryId;
    public string CategoryName { get; set; } = null!;

    public string CategoryDesciption { get; set; } = null!;

    public short? ParentCategoryId { get; set; }

    public bool? IsActive { get; set; }

    public virtual ICollection<Category> InverseParentCategory { get; set; } = new List<Category>();

    public virtual ICollection<NewsArticle> NewsArticles { get; set; } = new List<NewsArticle>();

    public virtual Category? ParentCategory { get; set; }
}
