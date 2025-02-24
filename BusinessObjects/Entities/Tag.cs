using BusinessObjects.Entities;
using System;
using System.Collections.Generic;

namespace BusinessObjects.Entities;

public partial class Tag : IEntity<int>
{
    public int TagId { get; set; }
    public int Id => TagId;
    public string? TagName { get; set; }

    public string? Note { get; set; }

    public virtual ICollection<NewsArticle> NewsArticles { get; set; } = new List<NewsArticle>();
}
