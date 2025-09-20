using CarDealerShip.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace CarDealerShip.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Car> Cars => Set<Car>();
        public DbSet<Purchase> Purchases => Set<Purchase>();
        public DbSet<AppUser> Users => Set<AppUser>();

        protected override void OnModelCreating(ModelBuilder b)
        {
            base.OnModelCreating(b);

            // ---------- Entity configuration ----------
            b.Entity<Car>(e =>
            {
                e.Property(x => x.Make).HasMaxLength(50).IsRequired();
                e.Property(x => x.Model).HasMaxLength(50).IsRequired();
                e.Property(x => x.Price).HasPrecision(18, 2);
            });

            b.Entity<AppUser>(e =>
            {
                e.Property(x => x.Email).HasMaxLength(256).IsRequired();
                e.Property(x => x.Role).HasMaxLength(32).IsRequired();
                e.HasIndex(x => x.Email).IsUnique();   // avoid duplicate users
            });

            b.Entity<Purchase>(e =>
            {
                // Default timestamp in DB (SQL Server). For SQLite use "CURRENT_TIMESTAMP".
                e.Property(x => x.PurchaseDate).HasDefaultValueSql("GETUTCDATE()");
            });

            // ---------- Stable seed IDs (match your migration) ----------
            var car1 = new Guid("30DAC83D-022F-42A1-ADEC-04DF1DB61848");
            var car2 = new Guid("5C2D89FD-0E76-4E30-91EE-6C41988C0C9D");
            var car3 = new Guid("7CAE4529-4BED-47FA-80F3-B49E8970991E");
            var car4 = new Guid("A23BD5B3-2981-4CE5-A97D-616C91DC0C02");
            var car5 = new Guid("BBA7CCB4-871F-4B79-8F87-CFD1B6E4FC4A");
            var car6 = new Guid("BD27A697-F06E-4CDA-A0EC-8BC675CBF07C");
            var car7 = new Guid("D151CFDD-51B7-4715-A45A-4070837E6142");
            var car8 = new Guid("D884B695-B973-405B-9299-B0B3450405AE");
            var car9 = new Guid("D914343B-5C88-417E-8112-0DC9E2BEEA31");
            var car10 = new Guid("F52880FB-A538-4A2B-816F-EEBD59B0AC12");

            var adminId = "4574785d-904d-48af-9740-5f23585faaf5";

            // ---------- Seed data (static, no dynamic calls) ----------
            b.Entity<Car>().HasData(
                new Car { Id = car8, Make = "Honda", Model = "Accord", Year = 2022, Price = 25000m, IsAvailable = true },
                new Car { Id = car9, Make = "Toyota", Model = "Corolla", Year = 2020, Price = 15000m, IsAvailable = true },
                new Car { Id = car6, Make = "Toyota", Model = "Camry", Year = 2021, Price = 20000m, IsAvailable = true },
                new Car { Id = car7, Make = "Ford", Model = "Focus", Year = 2018, Price = 12000m, IsAvailable = true },
                new Car { Id = car3, Make = "Ford", Model = "Mustang", Year = 2021, Price = 30000m, IsAvailable = true },
                new Car { Id = car2, Make = "Chevrolet", Model = "Malibu", Year = 2019, Price = 16000m, IsAvailable = true },
                new Car { Id = car1, Make = "BMW", Model = "3 Series", Year = 2022, Price = 35000m, IsAvailable = true },
                new Car { Id = car10, Make = "Mercedes", Model = "C-Class", Year = 2022, Price = 40000m, IsAvailable = true },
                new Car { Id = car5, Make = "Honda", Model = "Civic", Year = 2019, Price = 14000m, IsAvailable = true },
                new Car { Id = car4, Make = "Nissan", Model = "Altima", Year = 2020, Price = 18000m, IsAvailable = true }
            );

            b.Entity<AppUser>().HasData(new AppUser
            {
                Id = adminId,
                Email = "admin@local",
                PasswordHash = "Admin#12345",  // demo only; switch to Identity later
                Role = "Admin"
            });
        }
    }
}
