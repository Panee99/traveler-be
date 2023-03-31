using Service.Models.Tag;
using Shared.ResultExtensions;

namespace Service.Interfaces;

public interface ITagService
{
    Task<Result<TagViewModel>> Create(TagCreateModel model);

    Task<Result<TagViewModel>> Update(Guid id, TagUpdateModel model);

    Task<Result> Delete(Guid id);

    Task<Result<TagViewModel>> Find(Guid id);

    Task<Result<ICollection<TagViewModel>>> Filter(TagFilterModel model);
}