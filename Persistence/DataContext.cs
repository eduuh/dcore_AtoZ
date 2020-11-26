using Domain;
using Microsoft.EntityFrameworkCore;

namespace Persistence
{
        public class DataContext : DbContext
        {

            public DataContext(DbContextOptions option) : base(option)
            {

            }

            public DbSet<Values> Values { get; set; }
            
            protected override void OnModelCreating(ModelBuilder builder){
              builder.Entity<Values>().HasData(
                new Values { Id = 1, Name = "value 101" },
                new Values { Id = 2, Name = "value 102" },
                new Values { Id = 3, Name = "value 103" }
            );
        }
        }
    }
