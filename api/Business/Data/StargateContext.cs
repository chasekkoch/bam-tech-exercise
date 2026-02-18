using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using System.Data;

namespace StargateAPI.Business.Data
{
    public class StargateContext(DbContextOptions<StargateContext> options) : IdentityDbContext<ApplicationUser>(options)
    {
        public IDbConnection Connection => Database.GetDbConnection();
        public required DbSet<Person> People { get; set; }
        public required DbSet<AstronautDetail> AstronautDetails { get; set; }
        public required DbSet<AstronautDuty> AstronautDuties { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            
            // Suppress warning about pending model changes from Identity seed data
            optionsBuilder.ConfigureWarnings(warnings =>
                warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(StargateContext).Assembly);

            SeedData(modelBuilder);
        }

        private static void SeedData(ModelBuilder modelBuilder)
        {
            var seedDate = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            // Seed roles
            var adminRoleId = "1";
            var userRoleId = "2";

            modelBuilder.Entity<IdentityRole>().HasData(
                new IdentityRole
                {
                    Id = adminRoleId,
                    Name = "Admin",
                    NormalizedName = "ADMIN",
                    ConcurrencyStamp = adminRoleId
                },
                new IdentityRole
                {
                    Id = userRoleId,
                    Name = "User",
                    NormalizedName = "USER",
                    ConcurrencyStamp = userRoleId
                }
            );

            // Seed default admin user
            var adminUserId = "admin-user-id";
            var hasher = new PasswordHasher<ApplicationUser>();
            
            var adminUser = new ApplicationUser
            {
                Id = adminUserId,
                UserName = "admin@stargate.com",
                NormalizedUserName = "ADMIN@STARGATE.COM",
                Email = "admin@stargate.com",
                NormalizedEmail = "ADMIN@STARGATE.COM",
                EmailConfirmed = true,
                FirstName = "System",
                LastName = "Administrator",
                SecurityStamp = "C5E8A4F2-9B3D-4A1E-8F6C-2D7B9E4A1C3F",
                CreatedAt = seedDate
            };
            adminUser.PasswordHash = hasher.HashPassword(adminUser, "Admin123!");

            modelBuilder.Entity<ApplicationUser>().HasData(adminUser);

            // Assign admin role to admin user
            modelBuilder.Entity<IdentityUserRole<string>>().HasData(
                new IdentityUserRole<string>
                {
                    RoleId = adminRoleId,
                    UserId = adminUserId
                }
            );

            //add seed data
            modelBuilder.Entity<Person>()
                .HasData(
                    new Person
                    {
                        Id = 1,
                        Name = "John Doe"
                    },
                    new Person
                    {
                        Id = 2,
                        Name = "Jane Doe"
                    }
                );

            modelBuilder.Entity<AstronautDetail>()
                .HasData(
                    new AstronautDetail
                    {
                        Id = 1,
                        PersonId = 1,
                        CurrentRank = "1LT",
                        CurrentDutyTitle = "Commander",
                        CareerStartDate = seedDate
                    }
                );

            modelBuilder.Entity<AstronautDuty>()
                .HasData(
                    new AstronautDuty
                    {
                        Id = 1,
                        PersonId = 1,
                        DutyStartDate = seedDate,
                        DutyTitle = "Commander",
                        Rank = "1LT"
                    }
                );
        }
    }
}
