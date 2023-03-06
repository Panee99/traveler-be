﻿using Data.Entities;
using Data.Repositories.Interfaces;

namespace Data.Repositories.Implementations;

public class TravellerRepository : Repository<Traveller>, ITravellerRepository
{
    public TravellerRepository(TravelerDbContext context) : base(context)
    {
    }
}