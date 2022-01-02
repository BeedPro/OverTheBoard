﻿using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace OverTheBoard.Data.Repositories
{
    public class SecurityRepository<TEntity> : ISecurityRepository<TEntity> where TEntity : class
    {
        private readonly DbSet<TEntity> dbSet;
        
        public SecurityRepository(SecurityDbContext dbContext)
        {
            this.dbSet = dbContext.Set<TEntity>();
        }

        public IQueryable<TEntity> Query()
        {
            return dbSet.AsQueryable();
        }

        public void Add(TEntity entity)
        {
            dbSet.Add(entity);
        }
        public void RemoveRange(ICollection<TEntity> list)
        {
            dbSet.RemoveRange(list);
        }

        public IEnumerable<TEntity> GetAll()
        {
            return GetQuery().ToList();
        }

        public void Remove(TEntity entity)
        {
            if (entity == null) return;

            dbSet.Remove(entity);
        }

        public IQueryable<TEntity> GetQuery()
        {
            return dbSet;
        }

    }
}