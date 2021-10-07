using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.Specification;
using Microsoft.EntityFrameworkCore;
using OpenRace.Entities;

namespace OpenRace.Data.Ef
{
    public class MembersRepository : EfRepository<Member>
    {
        public MembersRepository(RaceDbContext dbContext) : base(dbContext)
        {
        }

        // public override Task AddOrUpdateAsync(Member entity, CancellationToken cancellationToken = default)
        // {
        //     // return _dbContext.Members.Upsert(entity)
        //     //     .On(c => new { c.FullName, c.Email })
        //     //     .RunAsync(cancellationToken);
        // }

        public Task<Member?> GetLastPaidMemberOrNull()
        {
            return _dbContext.Members
                .Where(it => it.Number != null)
                .OrderByDescending(it => it.Number)
                .Include(it => it.Payment)
                .FirstOrDefaultAsync()!;
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
                .FirstOrDefaultAsync(it => it.Id == id, cancellationToken)!;
        }
    }
}