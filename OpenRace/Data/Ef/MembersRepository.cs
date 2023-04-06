using System;
using System.Collections.Generic;
using System.Collections.Immutable;
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

        public Task<List<Member>> GetAdults() 
            => _dbContext.Members.Where(it => it.Age >= Member.AdultsAge).ToListAsync();
        
        public Task<List<Member>> GetChildren() 
            => _dbContext.Members.Where(it => it.Age < Member.AdultsAge).ToListAsync();

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
        
        public async Task<IReadOnlyList<Member>> GetUnpaidMembers(CancellationToken cancellationToken = default)
        {
            return await _dbContext.Members.Where(it => it.Payment!.PaidAt == null)
                .OrderByDescending(it => it.CreatedAt)
                .Include(it => it.Payment)
                .ToListAsync(cancellationToken: cancellationToken);
        }
        
        public Task<Member?> GetLastMemberNumberByDistance(int distance, CancellationToken cancellationToken = default)
        {
            return _dbContext.Members.Where(it => it.Distance == distance && it.Number != null)
                .OrderByDescending(it => it.Number)
                .Include(it => it.Payment)
                .FirstOrDefaultAsync(cancellationToken: cancellationToken);
        }
        
        public Task<Member?> GetLastMemberNumber(CancellationToken cancellationToken = default)
        {
            return _dbContext.Members.Where(it => it.Number != null)
                .OrderByDescending(it => it.Number)
                .Include(it => it.Payment)
                .FirstOrDefaultAsync(cancellationToken: cancellationToken);
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