using Service.Models.Tag;
using Shared.ResultExtensions;

namespace Service.Interfaces;

public interface ITagService
{
    Result<TagViewModel> Create(TagCreateModel model);

    Result<TagViewModel> Update(Guid id, TagUpdateModel model);
    
    Result Delete(Guid id);

    Result<TagViewModel> Find(Guid id);

    Task<Result<ICollection<TagViewModel>>> Filter(TagFilterModel model);
}