using Microsoft.EntityFrameworkCore;
using ForumApp.Models;

namespace ForumApp.Data;

public class ForumContext : DbContext
{
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Topic> Topics { get; set; } = null!;
    public DbSet<Message> Messages { get; set; } = null!;
    public DbSet<AuditLog> AuditLogs { get; set; } = null!;

    public ForumContext(DbContextOptions<ForumContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.Id);
            entity.HasIndex(u => u.Username).IsUnique();
            entity.HasIndex(u => u.Email).IsUnique();
            entity.Property(u => u.Role).HasConversion<int>();
        });

        // Topic configuration
        modelBuilder.Entity<Topic>(entity =>
        {
            entity.HasKey(t => t.Id);
            entity.HasOne(t => t.Author)
                .WithMany(u => u.Topics)
                .HasForeignKey(t => t.AuthorId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Message configuration
        modelBuilder.Entity<Message>(entity =>
        {
            entity.HasKey(m => m.Id);
            entity.HasOne(m => m.Topic)
                .WithMany(t => t.Messages)
                .HasForeignKey(m => m.TopicId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(m => m.Author)
                .WithMany(u => u.Messages)
                .HasForeignKey(m => m.AuthorId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // AuditLog configuration
        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(a => a.Id);
            entity.HasOne(a => a.User)
                .WithMany(u => u.AuditLogs)
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
