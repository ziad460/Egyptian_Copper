using System.Linq.Expressions;

namespace CopperFactory.Interfaces
{
    public interface IBaseRepository<T> where T : class
    {
        List<T> GetAll(string[] includes = null);
        Task<List<T>> GetAllAsync(string[] include = null);
        void AddOne(T entity);
        void UpdateOne(T entity);
        T Find(Expression<Func<T, bool>> criteria , string[] includes = null);
        List<T> FindAll(Expression<Func<T, bool>> criteria, string[] includes = null);
        Task<T> FindAsync(Expression<Func<T, bool>> criteria, string[] includes = null);
        Task<List<T>> FindAllAsync(Expression<Func<T, bool>> criteria, string[] includes = null);
        IQueryable<T> QueryableFind(Expression<Func<T, bool>> criteria);
    }
}
