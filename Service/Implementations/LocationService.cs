using Data;
using Data.Entities;
using MapsterMapper;
using Service.Interfaces;
using Service.Models.Location;
using Service.Results;

namespace Service.Implementations;

public class LocationService : ILocationService
{
    private readonly IUnitOfWork _unitOfWork;

    private readonly IMapper _mapper;

    public LocationService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public Result<Guid> Create(LocationCreateModel model)
    {
        var entity = _unitOfWork
            .Repo<Location>()
            .Add(_mapper.Map<Location>(model));

        _unitOfWork.SaveChanges();
        
        return entity.Id;
    }

    public Result<LocationViewModel> Find(Guid id)
    {
        var entity = _unitOfWork
            .Repo<Location>()
            .FirstOrDefault(e => e.Id == id);

        if (entity is null) return Error.NotFound();

        return _mapper.Map<LocationViewModel>(entity);
    }
}