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

        public async Task<Dictionary<Member, RaceEvent[]>> GetMemberAndEvents(Guid raceId, int distance, Gender? gender,
            bool? children)
        {
            // TODO переписать в SQL
            // var q = from m in _dbContext.Members.AsQueryable()
            //     join e in _dbContext.Events.AsQueryable() on m.Number equals e.MemberNumber
            //     where e.RaceId == raceId && e.Distance == distance
            //     group e by m
            //     into g
            //     select new { member = g.Key, events = g };
            
            // return _dbContext.Members
            //     .GroupJoin(
            //         _dbContext.Events, member => member.Number,
            //         @event => @event.MemberNumber,
            //         (Member member, IEnumerable<RaceEvent> events) => new { member, events }
            //     )
            //     .AsAsyncEnumerable()
            //     .ToLookupAsync(it => it.member, arg => arg.events.ToArray());

            // System.InvalidOperationException: The LINQ expression 'DbSet<Member>()
            //     .Join(
            //         inner: DbSet<RaceEvent>(),
            //         outerKeySelector: m => m.Number,
            //         innerKeySelector: r => (Nullable<int>)r.MemberNumber,
            //         resultSelector: (m, r) => new TransparentIdentifier<Member, RaceEvent>(
            //             Outer = m,
            //             Inner = r
            //         ))
            //     .Where(ti => ti.Inner.RaceId == __raceId_0 && ti.Inner.Distance == __distance_1)
            //     .GroupBy(
            //         keySelector: ti => ti.Outer,
            //         elementSelector: ti => ti.Inner)' could not be translated. 

            var q = _dbContext.Members.AsQueryable();
            if (gender != null) q = q.Where(it => it.Gender == gender);
            if (children == false) q = q.Where(it => it.Age >= Member.AdultsAge);
            else if (children == true) q = q.Where(it => it.Age < Member.AdultsAge);
            var members = await q.ToListAsync();
            var events = await GetRaceEvents(raceId, distance).ToLookupAsync(it => it.MemberNumber);
            members.RemoveAll(it => !events.Contains(it.Number.GetValueOrDefault()));
            var dict = members.ToDictionary(member => member, member => events[member.Number!.Value].ToArray());
            return dict;
        }

        public async Task DeleteEvents(Guid raceId)
        {
            var events = await GetAllRaceEvents(raceId).ToListAsync();
            await _dbContext.BulkDeleteAsync(events);
            await _dbContext.SaveChangesAsync();
        }
    }
}