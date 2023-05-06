using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.Specification;
using Ardalis.Specification.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using OpenRace.Entities;

namespace OpenRace.Data.Ef
{
    // See: https://github.com/ardalis/Specification/blob/main/Specification.EntityFrameworkCore/src/Ardalis.Specification.EntityFrameworkCore/RepositoryBaseOfT.cs
    public class EfRepository<TEntity> where TEntity : class, IEntity
    {
        protected readonly AppDbContext DbContext;
        private readonly ISpecificationEvaluator _specificationEvaluator = SpecificationEvaluator.Default;

        public EfRepository(AppDbContext dbContext)
        {
            DbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public virtual Task<TEntity> GetById(Guid id, CancellationToken cancellationToken = default)
        {
            return DbContext.Set<TEntity>().FirstAsync(it => it.Id == id, cancellationToken);
        }

        public virtual IAsyncEnumerable<TEntity> AllAsync()
        {
            return DbContext.Set<TEntity>().AsAsyncEnumerable();
        }

        public virtual async Task<int> CountAsync(ISpecification<TEntity> spec, CancellationToken cancellationToken = default)
        {
            var specificationResult = ApplySpecification(spec);
            return await specificationResult.CountAsync(cancellationToken);
        }

        public virtual async Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            await DbContext.Set<TEntity>().AddAsync(entity, cancellationToken);
            await DbContext.SaveChangesAsync(cancellationToken);

            return entity;
        }

        //https://entityframework-plus.net/batch-update
        public virtual async Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            DbContext.Entry(entity).State = EntityState.Modified;
            await DbContext.SaveChangesAsync(cancellationToken);
        }
        
        public async Task AddAll(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
        {
            await DbContext.AddRangeAsync(entities, cancellationToken);
            await DbContext.SaveChangesAsync(cancellationToken);
        }
        
        public virtual async Task AddOrUpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            DbContext.Entry(entity).State = entity.Id == Guid.Empty ? EntityState.Added : EntityState.Modified;
            await DbContext.SaveChangesAsync(cancellationToken);
        }

        public virtual async Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            DbContext.Set<TEntity>().Remove(entity);
            await DbContext.SaveChangesAsync(cancellationToken);
        }

        public virtual async Task<TEntity> FirstAsync(ISpecification<TEntity> spec, CancellationToken cancellationToken = default)
        {
            var specificationResult = ApplySpecification(spec);
            return await specificationResult.FirstAsync(cancellationToken);
        }

        public virtual Task<TEntity?> FirstOrDefaultAsync(
            ISpecification<TEntity> spec, CancellationToken cancellationToken = default)
        {
            var specificationResult = ApplySpecification(spec);
            return specificationResult.FirstOrDefaultAsync(cancellationToken);
        }

        protected virtual IQueryable<TEntity> ApplySpecification(ISpecification<TEntity> specification, bool evaluateCriteriaOnly = false)
        {
            return _specificationEvaluator.GetQuery(DbContext.Set<TEntity>().AsQueryable(), specification, evaluateCriteriaOnly);
        }
        
        /// <summary>
        /// Filters all entities of <typeparamref name="TEntity" />, that matches the encapsulated query logic of the
        /// <paramref name="specification"/>, from the database.
        /// <para>
        /// Projects each entity into a new form, being <typeparamref name="TResult" />.
        /// </para>
        /// </summary>
        /// <typeparam name="TResult">The type of the value returned by the projection.</typeparam>
        /// <param name="specification">The encapsulated query logic.</param>
        /// <returns>The filtered projected entities as an <see cref="IQueryable{T}"/>.</returns>
        protected virtual IQueryable<TResult> ApplySpecification<TResult>(ISpecification<TEntity, TResult> specification)
        {
            if (specification is null) throw new ArgumentNullException(nameof(specification));
            if (specification.Selector is null) throw new SelectorNotFoundException();

            return _specificationEvaluator.GetQuery(DbContext.Set<TEntity>().AsQueryable(), specification);
        }
    }
}