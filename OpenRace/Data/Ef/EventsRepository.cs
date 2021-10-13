using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OpenRace.Entities;

namespace OpenRace.Data.Ef
{
    public class EventsRepository : EfRepository<RaceEvent>
    {
        public EventsRepository(AppDbContext dbContext) : base(dbContext)
        {
        }

        public IAsyncEnumerable<RaceEvent> GetAllRaceEvents(Guid raceId)
            => _dbContext.Events.AsQueryable()
                .Where(it => it.RaceId == raceId)
                .OrderBy(it => it.TimeStamp)
                .AsAsyncEnumerable();
        
        public IAsyncEnumerable<RaceEvent> GetRaceEvents(Guid raceId, int distance)
            => _dbContext.Events.AsQueryable()
                .Where(it => it.RaceId == raceId && it.Distance == distance)
                .OrderBy(it => it.TimeStamp)
                .AsAsyncEnumerable();
    }
}