using BookApp.Entities;
using System;
using System.Collections.Generic;

namespace FUNews.DAL.Entities;

public partial class SystemAccount : IEntity<short>
{
    public short AccountId { get; set; }
    short IEntity<short>.Id => AccountId;
    public string? AccountName { get; set; }

    public string? AccountEmail { get; set; }

    public int? AccountRole { get; set; }

    public string? AccountPassword { get; set; }

    public virtual ICollection<NewsArticle> NewsArticles { get; set; } = new List<NewsArticle>();
}
