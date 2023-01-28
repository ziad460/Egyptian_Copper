using CopperFactory.Interfaces;
using CopperFactory.Models;

namespace CopperFactory.Repository
{
    public class UnitOfWork : IUnityOfWork
    {
        private readonly ApplicationDbContext context;
        public IBaseRepository<Zone> Zone { get; private set;}
        public IBaseRepository<Product> Product { get; private set;}
        public IBaseRepository<Customer> Customer { get; private set;}
        public IBaseRepository<Order> Order { get; private set;}
        public IBaseRepository<OrderDetails> OrderDetails { get; private set;}
        public IBaseRepository<Factory> Factory { get; private set;}
        public IBaseRepository<Production> Production { get; private set;}
        public IBaseRepository<Forcasting> Forcasting { get; private set;}
        public IBaseRepository<Inventory_IN> Inventory_IN { get; private set;}
        public IBaseRepository<Inventory_Out> Inventory_Out { get; private set; }
        
        public UnitOfWork(ApplicationDbContext _context)
        {
            context = _context;
            Zone = new BaseRepository<Zone>(context);
            Product = new BaseRepository<Product>(context);
            Customer = new BaseRepository<Customer>(context);
            Order = new BaseRepository<Order>(context);
            Factory = new BaseRepository<Factory>(context);
            Production = new BaseRepository<Production>(context);
            Forcasting = new BaseRepository<Forcasting>(context);
            Inventory_IN = new BaseRepository<Inventory_IN>(context);
            Inventory_Out = new BaseRepository<Inventory_Out>(context);
            OrderDetails = new BaseRepository<OrderDetails>(context);
        }
        
        public int Complete()
        {
            return context.SaveChanges();
        }

        public async Task<int> CompleteAsync()
        {
            return await context.SaveChangesAsync();
        }

        public void Dispose()
        {
            context.Dispose();
        }
    }
}
