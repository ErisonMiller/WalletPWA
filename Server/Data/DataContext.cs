//using Microsoft.EntityFrameworkCore;
//using System.Data.Entity;
using Microsoft.EntityFrameworkCore;
using WalletPWA.Shared;

namespace WalletPWA.Server.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {

        }


        public DbSet<Order> orders { get; set; }
        public DbSet<Asset> assets { get; set; }
        public virtual DbSet<User> users { get; set; }

        public DbSet<Patrimony> patrimonies { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Order>()
                .ToTable("orders");
            modelBuilder.Entity<Asset>()
                .ToTable("assets");
            modelBuilder.Entity<User>()
                .ToTable("users").Property(p => p.UserId).HasDefaultValueSql("newsequentialid()");
            modelBuilder.Entity<Patrimony>()
                .ToTable("patrimonies").HasKey(p => new { p.UserId, p.Date }); ;
        }


    }
}
