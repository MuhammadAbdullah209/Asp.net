using Auth_System.Models.Entity;
using Microsoft.EntityFrameworkCore;

namespace Auth_System.data
{
    public class AppDbContext:DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        
        }
       public DbSet<User>Users { get; set; }
    }
}
