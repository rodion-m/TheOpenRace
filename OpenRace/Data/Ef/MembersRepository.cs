using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OpenRace.Entities;

namespace OpenRace.Data.Ef
{
    public class MembersRepository : EfRepository<Member>
    {
        public MembersRepository(AppDbContext dbContext) : base(dbContext)
        {
        }

        public IAsyncEnumerable<Member> GetAdults() 
            => _dbContext.Members.AsQueryable().Where(it => it.Age >= Member.AdultsAge).AsAsyncEnumerable();
        
        public IAsyncEnumerable<Member> GetChildren() 
            => _dbContext.Members.AsQueryable().Where(it => it.Age < Member.AdultsAge).AsAsyncEnumerable();

        // public override Task AddOrUpdateAsync(Member entity, CancellationToken cancellationToken = default)
        // {
        //     // return _dbContext.Members.Upsert(entity)
        //     //     .On(c => new { c.FullName, c.Email })
        //     //     .RunAsync(cancellationToken);
        // }

        public Task<Member?> GetLastPaidMemberOrNull()
        {
            return _dbContext.Members.Where(it => it.Number != null)
                .OrderByDescending(it => it.Number)
                .Include(it => it.Payment)
                .FirstOrDefaultAsync();
        }
        
        public async Task<IReadOnlyList<Member>> GetUnpaidMembers()
        {
            return await _dbContext.Members.Where(it => it.Payment!.PaidAt == null)
                .OrderByDescending(it => it.CreatedAt)
                .Include(it => it.Payment)
                .ToListAsync();
        }
        
        public Task<Member?> GetLastMemberNumberByDistance(int distance)
        {
            return _dbContext.Members.Where(it => it.Distance == distance && it.Number != null)
                .OrderByDescending(it => it.Number)
                .Include(it => it.Payment)
                .FirstOrDefaultAsync();
        }
        
        public Task<Member?> GetLastMemberNumber()
        {
            return _dbContext.Members.Where(it => it.Number != null)
                .OrderByDescending(it => it.Number)
                .Include(it => it.Payment)
                .FirstOrDefaultAsync();
        }
        
        public override Task<Member> GetById(Guid id, CancellationToken cancellationToken = default)
        {
            return _dbContext.Members
                .Include(it => it.Payment)
                .FirstAsync(it => it.Id == id, cancellationToken);
        }
        
        public Task<Member?> GetByIdOrNull(Guid id, CancellationToken cancellationToken = default)
        {
            return _dbContext.Members
                .Include(it => it.Payment)
                .FirstOrDefaultAsync(it => it.Id == id, cancellationToken);
        }

        public IAsyncEnumerable<Member> GetSubscribedMembers()
        {
            return _dbContext.Members.Where(it => it.Subscribed).ToAsyncEnumerable();
        }
    }
}