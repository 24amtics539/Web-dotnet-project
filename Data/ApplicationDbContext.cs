using CampusDeadline.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace CampusDeadline.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Subject> Subjects { get; set; } = null!;
        public DbSet<TaskItem> Tasks { get; set; } = null!;
        public DbSet<TaskStatusHistory> TaskStatusHistories { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User configuration
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(u => u.Email).IsUnique();
            });

            // Subject configuration
            modelBuilder.Entity<Subject>(entity =>
            {
                entity.HasOne(s => s.User)
                      .WithMany(u => u.Subjects)
                      .HasForeignKey(s => s.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(s => s.UserId);
            });

            // Task configuration
            modelBuilder.Entity<TaskItem>(entity =>
            {
                entity.HasOne(t => t.User)
                      .WithMany(u => u.Tasks)
                      .HasForeignKey(t => t.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(t => t.Subject)
                      .WithMany(s => s.Tasks)
                      .HasForeignKey(t => t.SubjectId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(t => t.UserId);
                entity.HasIndex(t => t.SubjectId);
                entity.HasIndex(t => t.DueDate);
                entity.HasIndex(t => t.Status);
                entity.HasIndex(t => new { t.UserId, t.Status });
            });

            // TaskStatusHistory configuration
            modelBuilder.Entity<TaskStatusHistory>(entity =>
            {
                entity.HasOne(h => h.TaskItem)
                      .WithMany(t => t.StatusHistories)
                      .HasForeignKey(h => h.TaskItemId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(h => h.TaskItemId);
            });
        }
    }
}
