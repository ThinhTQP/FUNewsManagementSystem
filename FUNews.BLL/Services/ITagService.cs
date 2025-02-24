using System;
using System.Linq;
using System.Text;
using BusinessObjects.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

public interface ITagService
{
    Task<IEnumerable<Tag>> GetAllTagsAsync();
    Task<Tag?> GetTagByIdAsync(int id);
    Task AddTagAsync(Tag tag);
    Task UpdateTagAsync(Tag tag);
    Task DeleteTagAsync(int id);
}
