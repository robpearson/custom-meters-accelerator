using Microsoft.EntityFrameworkCore;
using ManagedApplicationScheduler.DataAccess.Entities;
namespace ManagedApplicationScheduler.DataAccess.Context
{
    public class ApplicationsDBContext : DbContext
    {
        public DbSet<ApplicationConfiguration> ApplicationConfigurations { get; set; }
        public DbSet<ApplicationLog> ApplicationLogs { get; set; }
        public DbSet<UsageResult> UsageResults { get; set; }
        public DbSet<Subscription> Subscriptions { get; set; }
        public DbSet<Plan> Plans { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<ScheduledTasks> ScheduledTasks { get; set; } // Added this line

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {

            }
        }
        public ApplicationsDBContext()
        {
        }

        public ApplicationsDBContext(DbContextOptions<ApplicationsDBContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ApplicationConfiguration>(entity =>
            {
                entity.HasKey(e => e.id);
                entity.Property(e => e.Name).IsRequired();
                entity.Property(e => e.Value).IsRequired();
                entity.Property(e => e.Description);
            });

            modelBuilder.Entity<ApplicationLog>(entity =>
            {
                entity.HasKey(e => e.id);
                entity.Property(e => e.ActionTime);
                entity.Property(e => e.LogDetail);

            });

            modelBuilder.Entity<UsageResult>(entity =>
            {
                entity.HasKey(e => e.id);
                entity.Property(e => e.Status);
                entity.Property(e => e.UsagePostedDate);
                entity.Property(e => e.UsageEventId);
                entity.Property(e => e.MessageTime);
                entity.Property(e => e.ResourceId);
                entity.Property(e => e.Quantity);
                entity.Property(e => e.Dimension);
                entity.Property(e => e.PlanId);
                entity.Property(e => e.ScheduledTaskName);
                entity.Property(e => e.ResourceUri);
                entity.Property(e => e.Message);
            });

            modelBuilder.Entity<Subscription>(entity =>
            {
                entity.HasKey(e => e.id);
                entity.Property(e => e.PlanId);
                entity.Property(e => e.Publisher);
                entity.Property(e => e.Product);
                entity.Property(e => e.Version);
                entity.Property(e => e.ProvisionState);
                entity.Property(e => e.ProvisionTime);
                entity.Property(e => e.ResourceUsageId);
                entity.Property(e => e.SubscriptionStatus);
                entity.Property(e => e.Dimension);
                entity.Property(e => e.SubscriptionKey);
                entity.Property(e => e.ResourceUri);
            });

            modelBuilder.Entity<Plan>(entity =>
            {
                entity.HasKey(e => e.id);
                entity.Property(e => e.PlanId);
                entity.Property(e => e.PlanName);
                entity.Property(e => e.OfferId);
                entity.Property(e => e.OfferName);
                entity.Property(e => e.Dimension);

            });

            modelBuilder.Entity<Payment>(entity =>
            {
                entity.HasKey(e => e.id);
                entity.Property(e => e.PaymentName);
                entity.Property(e => e.Quantity);
                entity.Property(e => e.Dimension);
                entity.Property(e => e.PlanId);
                entity.Property(e => e.StartDate);
                entity.Property(e => e.PaymentType);
                entity.Property(e => e.OfferId);
            });

            modelBuilder.Entity<ScheduledTasks>(entity => // Added this block
            {
                entity.HasKey(e => e.id);
                entity.Property(e => e.ScheduledTaskName);
                entity.Property(e => e.ResourceUri);
                entity.Property(e => e.Quantity);
                entity.Property(e => e.Dimension);
                entity.Property(e => e.NextRunTime);
                entity.Property(e => e.PlanId);
                entity.Property(e => e.Frequency);
                entity.Property(e => e.StartDate);
                entity.Property(e => e.Status);
            });
        }
    }
}