using Microsoft.EntityFrameworkCore;
using ReminderService.Worker.Entities;

namespace ReminderService.Worker.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
        ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
    }

    public DbSet<Recipient> Recipients => Set<Recipient>();
    public DbSet<Reminder> Reminders => Set<Reminder>();
    public DbSet<ReminderRecipient> ReminderRecipients => Set<ReminderRecipient>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Recipient>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(200);
            entity.Property(e => e.IsActive).IsRequired();
        });

        modelBuilder.Entity<Reminder>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(300);
            entity.Property(e => e.Description).HasMaxLength(2000);
            entity.Property(e => e.DueDate).IsRequired();
            entity.Property(e => e.IntervalDays).IsRequired();
            entity.Property(e => e.Priority).IsRequired();
        });

        modelBuilder.Entity<ReminderRecipient>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Status).IsRequired();

            entity.HasOne(e => e.Reminder)
                .WithMany(r => r.ReminderRecipients)
                .HasForeignKey(e => e.ReminderId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Recipient)
                .WithMany(r => r.ReminderRecipients)
                .HasForeignKey(e => e.RecipientId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new { e.ReminderId, e.RecipientId }).IsUnique();
        });
    }
}
