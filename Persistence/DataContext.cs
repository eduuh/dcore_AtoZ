using System;
using System.ComponentModel;
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
        public DbSet<UserFollowing> Followings { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<UserActivity>(b =>
            {

                b.HasKey(ua => new { ua.AppUserId, ua.ActivityId });

                b.HasOne(a => a.AppUser).WithMany(act => act.UserActivities).HasForeignKey(f => f.AppUserId);

                b.HasOne(act => act.Activity).WithMany(a => a.AppUsers).HasForeignKey(f => f.ActivityId);
            }

            );

            builder.Entity<UserFollowing>(b =>
            {
                b.HasKey(k => new { k.ObserverId, k.TargetId });
                b.HasOne(o => o.Observer).WithMany(f => f.Followings)
                .HasForeignKey(o => o.ObserverId).OnDelete(DeleteBehavior.Restrict);


                b.HasOne(o => o.Target).WithMany(f => f.Followers)
                .HasForeignKey(o => o.TargetId).OnDelete(DeleteBehavior.Restrict);

            });

        }
    }
}
