using Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Dal
{
    public class TicketsContext : IdentityDbContext
    {
        public TicketsContext(DbContextOptions<TicketsContext> options)
            : base(options)
        {
        }
        public DbSet<AuthLog> AuthLog { get; set; }
        public DbSet<ErrorLog> ErrorLog { get; set; }
        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<Reply> Replies { get; set; }
        public DbSet<ReplyImage> ReplyImages { get; set; }

        public DbSet<Employee> Employees { get; set; }
        public DbSet<Customer> Customers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}
