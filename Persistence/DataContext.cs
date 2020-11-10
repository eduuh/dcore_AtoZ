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

    }
}
