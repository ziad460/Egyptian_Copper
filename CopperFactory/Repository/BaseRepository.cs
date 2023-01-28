using CopperFactory.Interfaces;
using CopperFactory.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace CopperFactory.Repository
{
    public class BaseRepository<T> : IBaseRepository<T> where T : class
    {
        private readonly ApplicationDbContext context;

        public BaseRepository(ApplicationDbContext _context)
        {
            context = _context;
        }
        public void AddOne(T entity)
        {
            context.Set<T>().Add(entity);
        }
        public T Find(Expression<Func<T, bool>> criteria, string[] includes = null)
        {
            IQueryable<T> query = context.Set<T>();
            if (includes != null)
            {
                foreach (var item in includes)
                {
                    if (query.Include(item) != null)
                        query = query.Include(item);
                }
            }
            return query.SingleOrDefault(criteria); 
        }
        public async Task<T> FindAsync(Expression<Func<T, bool>> criteria, string[] includes = null)
        {
            IQueryable<T> query = context.Set<T>();
            if (includes != null)
            {
                foreach (var item in includes)
                {
                    if (query.Include(item) != null)
                        query = query.Include(item);
                }
            }
            return await query.SingleOrDefaultAsync(criteria);
        }
        public List<T> FindAll(Expression<Func<T, bool>> criteria, string[] includes = null)
        {
            IQueryable<T> query = context.Set<T>();
            if (includes != null)
            {
                foreach (var item in includes)
                {
                    if (query.Include(item) != null)
                        query = query.Include(item);
                }
            }
            return query.Where(criteria).ToList();
        }
        public async Task<List<T>> FindAllAsync(Expression<Func<T, bool>> criteria, string[] includes = null)
        {
            IQueryable<T> query = context.Set<T>();
            if (includes != null)
            {
                foreach (var item in includes)
                {
                    if (query.Include(item) != null)
                        query = query.Include(item);
                }
            }
            return await query.Where(criteria).ToListAsync();
        }

        public List<T> GetAll(string[] includes = null)
        {
            IQueryable<T> query = context.Set<T>();
            if (includes != null)
            {
                foreach (var item in includes)
                {
                    if (query.Include(item) != null)
                        query = query.Include(item);
                }
            }
            return query.ToList();
        }

        public async Task<List<T>> GetAllAsync(string[] include = null)
        {
            IQueryable<T> query = context.Set<T>();
            if (include != null)
            {
                foreach (var item in include)
                {
                    if (query.Include(item) != null)
                        query = query.Include(item);
                }
            }
            var x = await query.ToListAsync();
            return x;
        }

        public void UpdateOne(T entity)
        {
            context.Entry(entity).State = EntityState.Modified;
        }

        public IQueryable<T> QueryableFind(Expression<Func<T, bool>> criteria)
        {
            return context.Set<T>().Where(criteria);
        }

    }
}
