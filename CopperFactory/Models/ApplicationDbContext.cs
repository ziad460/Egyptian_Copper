using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using CopperFactory.ViewModels;

namespace CopperFactory.Models
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public virtual DbSet<Zone> zones { get; set; }
        public virtual DbSet<Factory> Factories { get; set; }
        public virtual DbSet<Product> Products { get; set; }
        public virtual DbSet<Order> Orders { get; set; }
        public virtual DbSet<OrderDetails> OrderDetails { get; set; }
        public virtual DbSet<Customer> Customers { get; set; }
        public virtual DbSet<Production> Productions { get; set; }
        public virtual DbSet<Forcasting> Forcastings { get; set; }
        public virtual DbSet<Inventory_IN> Inventory_INs { get; set; }
        public virtual DbSet<Inventory_Out> Inventory_Outs { get; set; }
    }
}