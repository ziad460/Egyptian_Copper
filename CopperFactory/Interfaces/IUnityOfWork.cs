
using CopperFactory.Models;

namespace CopperFactory.Interfaces
{
    public interface IUnityOfWork : IDisposable
    {
        IBaseRepository<Zone> Zone { get; }
        IBaseRepository<Product> Product { get; }
        IBaseRepository<Customer> Customer { get; }
        IBaseRepository<Order> Order { get; }
        IBaseRepository<OrderDetails> OrderDetails { get; }
        IBaseRepository<Factory> Factory { get; }
        IBaseRepository<Production> Production { get; }
        IBaseRepository<Forcasting> Forcasting { get; }
        IBaseRepository<Inventory_IN> Inventory_IN { get; }
        IBaseRepository<Inventory_Out> Inventory_Out { get; }
        int Complete();
        Task<int> CompleteAsync();
    }
}
