using Data;
using Data.Entities;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Service.Interfaces;
using Service.Models.Location;
using Service.Models.Tag;
using Service.Results;

namespace Service.Implementations;

public class LocationService : BaseService, ILocationService
{
    private readonly IMapper _mapper;

    public LocationService(IUnitOfWork unitOfWork, IMapper mapper) : base(unitOfWork)
    {
        _mapper = mapper;
    }

    public async Task<Result<LocationViewModel>> Create(LocationCreateModel model)
    {
        var entity = _unitOfWork.Repo<Location>().Add(_mapper.Map<Location>(model));

        _unitOfWork.Repo<LocationTag>().AddRange(
            model.Tags.Select(id => new LocationTag()
                {
                    LocationId = entity.Id,
                    TagId = id
                }
            )
        );
        await _unitOfWork.SaveChangesAsync();

        // result
        var tags = await _unitOfWork.Repo<Tag>().Query().Where(e => model.Tags.Contains(e.Id)).ToListAsync();
        var viewModel = _mapper.Map<LocationViewModel>(entity);
        viewModel.Tags = _mapper.Map<List<TagViewModel>>(tags);
        return viewModel;
    }

    public async Task<Result> Delete(Guid id)
    {
        var entity = _unitOfWork.Repo<Location>().FirstOrDefault(e => e.Id == id);
        if (entity is null) return Error.NotFound();

        _unitOfWork.Repo<Location>().Remove(entity);

        await _unitOfWork.SaveChangesAsync();

        return Result.Success();
    }

    public async Task<Result<LocationViewModel>> Find(Guid id)
    {
        var entity = await _unitOfWork
            .Repo<Location>()
            .Query()
            .Select(e => new
            {
                e.Id,
                e.Name,
                e.Country,
                e.City,
                e.Address,
                e.Longitude,
                e.Latitude,
                e.Description,
                Tags = e.LocationTags.Select(lt => lt.Tag).ToList(),
                Attachments = e.LocationAttachments.Select(la => la.Attachment).ToList()
            })
            // .SelectMany(e => e.LocationTags)
            // .SelectMany(e => e.LocationAttachments.Select(a => a.Attachment))
            .FirstOrDefaultAsync(e => e.Id == id);

        if (entity is null) return Error.NotFound();

        var viewModel = _mapper.Map<LocationViewModel>(entity);
        viewModel.Tags = _mapper.Map<List<TagViewModel>>(entity.Tags);

        return viewModel;
    }
}