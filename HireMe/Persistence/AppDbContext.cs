using System.Reflection;
using HireMe.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HireMe.Persistence
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        
        public DbSet<Governorate> Governorates {get;set;} 

        public DbSet<Job> Jobs {get;set;}

        public DbSet<Question> Questions {get;set;}
        
        protected override void OnModelCreating(ModelBuilder builder)
        {

           builder.Entity<Job>().HasQueryFilter(j => !j.IsDeleted);
           builder.Entity<Question>().HasQueryFilter(q => !q.IsDeleted);
           builder.Entity<Answer>().HasQueryFilter(a => !a.IsDeleted);
           builder.Entity<Application>().HasQueryFilter(a => !a.IsDeleted);

            builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
            base.OnModelCreating(builder);
        }
    }
}
