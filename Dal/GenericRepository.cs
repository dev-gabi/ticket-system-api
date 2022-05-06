using Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Dal
{
    public class GenericRepository <TEntity> where TEntity : class, IId
    {
        internal TicketsContext context;
        internal DbSet<TEntity> dbSet;

        public GenericRepository(TicketsContext _context)
        {
            this.context = _context;
            this.dbSet = context.Set<TEntity>();
            context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;//with out this line httpPut will throw an exception
        }
        public virtual IEnumerable<TEntity> Get(
            Expression<Func<TEntity, bool>> filter = null,
            string includeProperties = "")
        {
            IQueryable<TEntity> query = dbSet;

            if (filter != null)
            {
                query = query.Where(filter);
            }

            foreach (var includeProperty in includeProperties.Split
                (new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                query = query.Include(includeProperty);
            }

            return query.ToList();

        }
    //    public virtual IQueryable<TEntity> GetIQueryable(
    //Expression<Func<TEntity, bool>> filter = null,
    //string includeProperties = "")
    //    {
    //        IQueryable<TEntity> query = dbSet;

    //        if (filter != null)
    //        {
    //            query = query.Where(filter);
    //        }

    //        foreach (var includeProperty in includeProperties.Split
    //            (new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
    //        {
    //            query = query.Include(includeProperty);
    //        }
    //        return query;
    //    }


        public virtual TEntity GetOne(Expression<Func<TEntity, bool>> filter = null)
        {
            TEntity entity = context.Set<TEntity>().FirstOrDefault(filter);
            return entity;
        }

        public virtual TEntity GetByID(object id)
        {

            return dbSet.Find(id);
        }

        public virtual void Add(TEntity entity)
        {
            dbSet.Add(entity);
        }

        //public virtual void AddRange(List<TEntity> entityList)//adding multiple items
        //{
        //    dbSet.AddRange(entityList);
        //}

        public virtual void Delete(object id)
        {
            TEntity entityToDelete = dbSet.Find(id);
            Delete(entityToDelete);
        }

        public virtual void Delete(TEntity entityToDelete)
        {
            if (context.Entry(entityToDelete).State == EntityState.Detached)
            {
                dbSet.Attach(entityToDelete);
            }
            dbSet.Remove(entityToDelete);

        }
        public virtual void Delete(Expression<Func<TEntity, bool>> filter = null)
        {
            IQueryable<TEntity> query = dbSet;
            if (filter != null)
            {
                query = query.Where(filter);
            }
            foreach (var item in query)
            {
                if (context.Entry(item).State == EntityState.Detached)
                {
                    dbSet.Attach(item);
                }
                dbSet.Remove(item);
            }
        }

        public virtual void Update(TEntity entityToUpdate)
        {
            dbSet.Attach(entityToUpdate);
            context.Entry(entityToUpdate).State = EntityState.Modified;
        }

        public void Save()
        {
            context.SaveChanges();
        }
    }
}
