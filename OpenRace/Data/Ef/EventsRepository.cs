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
                .ThenBy(it => it.MemberNumber)
                .AsAsyncEnumerable();
        
        public IAsyncEnumerable<RaceEvent> GetRaceEvents(Guid raceId, EventType eventType)
            => _dbContext.Events.AsQueryable()
                .Where(it => it.RaceId == raceId && it.EventType == eventType)
                .AsAsyncEnumerable();
        
        public IAsyncEnumerable<RaceEvent> GetRaceEvents(Guid raceId, EventType eventType1, EventType eventType2)
            => _dbContext.Events.AsQueryable()
                .Where(it => it.RaceId == raceId && (it.EventType == eventType1 || it.EventType == eventType2))
                .AsAsyncEnumerable();

        public Task<RaceEvent?> GetLastEventByCreatorOrNull(string creatorName)
        {
            return _dbContext.Events.AsQueryable()
                .Where(it => it.CreatorName == creatorName)
                .OrderByDescending(it => it.TimeStamp)
                .FirstOrDefaultAsync();
        }

        public ValueTask<ILookup<Member, RaceEvent[]>> GetResults(Guid raceId, int distance, Gender? gender, bool? children)
        {
            var q = from m in _dbContext.Members.AsQueryable()
                join e in _dbContext.Events on m.Number equals e.MemberNumber
                where e.RaceId == raceId && e.Distance == distance
                group e by m into g
                select new { member = g.Key, events = g };

            if (gender != null) q = q.Where(it => it.member.Gender == gender);
            if (children == false) q = q.Where(it => it.member.Age >= Member.AdultsAge);
            else if (children == true) q = q.Where(it => it.member.Age < Member.AdultsAge);
             return q.AsAsyncEnumerable()
                .ToLookupAsync(it => it.member, arg => arg.events.ToArray());
            // return _dbContext.Members
            //     .GroupJoin(
            //         _dbContext.Events, member => member.Number,
            //         @event => @event.MemberNumber,
            //         (Member member, IEnumerable<RaceEvent> events) => new { member, events }
            //     )
            //     .AsAsyncEnumerable()
            //     .ToLookupAsync(it => it.member, arg => arg.events.ToArray());
        }
    }
}