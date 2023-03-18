using Data;
using Data.Entities;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Service.Interfaces;
using Service.Models.Tag;
using Service.Results;

namespace Service.Implementations;

public class TagService : BaseService, ITagService
{
    private readonly IMapper _mapper;

    public TagService(IUnitOfWork unitOfWork, IMapper mapper) : base(unitOfWork)
    {
        _mapper = mapper;
    }

    public Result<TagViewModel> Create(TagCreateModel model)
    {
        var entity = _unitOfWork.Repo<Tag>().Add(_mapper.Map<Tag>(model));
        _unitOfWork.SaveChanges();
        return _mapper.Map<TagViewModel>(entity);
    }

    public Result<TagViewModel> Update(Guid id, TagUpdateModel model)
    {
        var entity = _unitOfWork.Repo<Tag>().FirstOrDefault(e => e.Id == id);

        if (entity is null) return Error.NotFound();

        if (model.Name != null) entity.Name = model.Name;
        if (model.Type != null) entity.Type = model.Type.Value;

        entity = _unitOfWork.Repo<Tag>().Update(entity);
        _unitOfWork.SaveChanges();
        return _mapper.Map<TagViewModel>(entity);
    }

    public Result Delete(Guid id)
    {
        var entity = _unitOfWork.Repo<Tag>().FirstOrDefault(e => e.Id == id);
        if (entity is null) return Error.NotFound();
        _unitOfWork.Repo<Tag>().Remove(entity);

        _unitOfWork.SaveChanges();
        return Result.Success();
    }

    public Result<TagViewModel> Find(Guid id)
    {
        var entity = _unitOfWork.Repo<Tag>().FirstOrDefault(e => e.Id == id);
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