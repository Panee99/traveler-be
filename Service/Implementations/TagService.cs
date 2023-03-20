using Data.EFCore;
using Data.Entities;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Service.Interfaces;
using Service.Models.Tag;
using Shared.ResultExtensions;

namespace Service.Implementations;

public class TagService : BaseService, ITagService
{
    private readonly IMapper _mapper;

    public TagService(IUnitOfWork unitOfWork, IMapper mapper) : base(unitOfWork)
    {
        _mapper = mapper;
    }

    public async Task<Result<TagViewModel>> Create(TagCreateModel model)
    {
        var entity = _unitOfWork.Repo<Tag>().Add(_mapper.Map<Tag>(model));
        await _unitOfWork.SaveChangesAsync();
        return _mapper.Map<TagViewModel>(entity);
    }

    public async Task<Result<TagViewModel>> Update(Guid id, TagUpdateModel model)
    {
        var entity = await _unitOfWork.Repo<Tag>().FirstOrDefaultAsync(e => e.Id == id);

        if (entity is null) return Error.NotFound();

        if (model.Name != null) entity.Name = model.Name;
        if (model.Type != null) entity.Type = model.Type.Value;

        entity = _unitOfWork.Repo<Tag>().Update(entity);
        await _unitOfWork.SaveChangesAsync();
        return _mapper.Map<TagViewModel>(entity);
    }

    public async Task<Result> Delete(Guid id)
    {
        var entity = await _unitOfWork.Repo<Tag>().FirstOrDefaultAsync(e => e.Id == id);
        if (entity is null) return Error.NotFound();
        _unitOfWork.Repo<Tag>().Remove(entity);

        await _unitOfWork.SaveChangesAsync();
        return Result.Success();
    }

    public async Task<Result<TagViewModel>> Find(Guid id)
    {
        var entity = await _unitOfWork.Repo<Tag>().FirstOrDefaultAsync(e => e.Id == id);
        if (entity is null) return Error.NotFound();
        return _mapper.Map<TagViewModel>(entity);
    }

    public async Task<Result<ICollection<TagViewModel>>> Filter(TagFilterModel model)
    {
        var query = _unitOfWork.Repo<Tag>().Query();

        if (model.Type != null) query = query.Where(e => e.Type == model.Type);
        if (model.Name != null) query = query.Where(e => e.Name.Contains(model.Name));

        return _mapper.Map<List<TagViewModel>>(await query.ToListAsync());
    }
}