using System.Security.Cryptography.X509Certificates;
using Domain;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Persistence
{
        public class DataContext : IdentityDbContext<AppUser>
        {

            public DataContext(DbContextOptions option) : base(option)
            {

            }

            public DbSet<Values> Values { get; set; }
            public DbSet<Activity> Activities { get; set; }
            public DbSet<UserActivity> UserActivities { get; set; }
            
            protected override void OnModelCreating(ModelBuilder builder){
              base.OnModelCreating(builder);
              builder.Entity<Values>().HasData(
                  new Values { Id = 1, Name = "value 101" },
                  new Values { Id = 2, Name = "value 102" },
                  new Values { Id = 3, Name = "value 103" }
              );

            // many to many relationship
            builder.Entity<UserActivity>(x => x.HasKey(ua => new { ua.AppUserId, ua.ActivityId }));
            builder.Entity<UserActivity>().HasOne(u => u.AppUser).WithMany(a => a.UserActivities).HasForeignKey(u => u.AppUserId);

            builder.Entity<UserActivity>().HasOne(u => u.Activity).WithMany(a => a.UserActivities).HasForeignKey(u => u.ActivityId);
        }
        }
    }
