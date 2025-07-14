using Microsoft.EntityFrameworkCore;
using Tiba.Shared.Model;

namespace BackendService.Dal;

public class TodoDbContext : DbContext
{
    public TodoDbContext(DbContextOptions<TodoDbContext> options) : base(options)
    {
    }

    public DbSet<Todo> Todos { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Todo>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.Title).IsRequired().HasMaxLength(500);
            entity.Property(e => e.IsCompleted).IsRequired();
            entity.Property(e => e.UserId).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
            
            // Index for efficient user-based queries
            entity.HasIndex(e => e.UserId);
            
            // Table name mapping
            entity.ToTable("todos");
        });
    }
}