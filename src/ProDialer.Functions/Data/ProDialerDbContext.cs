using Microsoft.EntityFrameworkCore;
using ProDialer.Shared.Models;

namespace ProDialer.Functions.Data;

/// <summary>
/// Database context for ProDialer application using Entity Framework Core
/// Manages campaigns, lists, leads, agents, and call logs
/// </summary>
public class ProDialerDbContext : DbContext
{
    public ProDialerDbContext(DbContextOptions<ProDialerDbContext> options) : base(options)
    {
    }

    // Main entity sets
    public DbSet<Campaign> Campaigns { get; set; } = null!;
    public DbSet<List> Lists { get; set; } = null!;
    public DbSet<Lead> Leads { get; set; } = null!;
    public DbSet<Agent> Agents { get; set; } = null!;
    public DbSet<CallLog> CallLogs { get; set; } = null!;
    public DbSet<CampaignList> CampaignLists { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Campaign entity
        modelBuilder.Entity<Campaign>(entity =>
        {
            entity.ToTable("Campaigns");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.DialingRatio).HasDefaultValue(1);
            entity.Property(e => e.ApplyRatioToIdleAgentsOnly).HasDefaultValue(true);
            entity.Property(e => e.MaxConcurrentCalls).HasDefaultValue(50);
            entity.Property(e => e.TimeZone).HasMaxLength(100).HasDefaultValue("UTC");
            entity.Property(e => e.CallStartTime).HasMaxLength(5).HasDefaultValue("08:00");
            entity.Property(e => e.CallEndTime).HasMaxLength(5).HasDefaultValue("20:00");
            entity.Property(e => e.AllowedDaysOfWeek).HasMaxLength(200).HasDefaultValue("Monday,Tuesday,Wednesday,Thursday,Friday");
            entity.Property(e => e.RespectLeadTimeZone).HasDefaultValue(true);
            entity.Property(e => e.MinCallInterval).HasDefaultValue(60);
            entity.Property(e => e.MaxCallAttempts).HasDefaultValue(3);
            entity.Property(e => e.CallAttemptDelay).HasDefaultValue(15);
            entity.Property(e => e.EnableAnsweringMachineDetection).HasDefaultValue(true);
            entity.Property(e => e.AnsweringMachineAction).HasMaxLength(50).HasDefaultValue("Hangup");
            entity.Property(e => e.AnsweringMachineMessage).HasMaxLength(2000);
            entity.Property(e => e.IsActive).HasDefaultValue(false);
            entity.Property(e => e.Priority).HasDefaultValue(5);
            entity.Property(e => e.LocationRestrictions).HasMaxLength(500);
            entity.Property(e => e.EnableCallRecording).HasDefaultValue(false);
            entity.Property(e => e.CreatedBy).HasMaxLength(100);
            entity.Property(e => e.UpdatedBy).HasMaxLength(100);
            
            // Indexes
            entity.HasIndex(e => e.Name);
            entity.HasIndex(e => e.IsActive);
            entity.HasIndex(e => e.Priority);
            entity.HasIndex(e => e.CreatedAt);
        });

        // Configure List entity
        modelBuilder.Entity<List>(entity =>
        {
            entity.ToTable("Lists");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.Priority).HasDefaultValue(5);
            entity.Property(e => e.CallInOrder).HasDefaultValue(true);
            entity.Property(e => e.TimeZoneOverride).HasMaxLength(100);
            entity.Property(e => e.CallStartTimeOverride).HasMaxLength(5);
            entity.Property(e => e.CallEndTimeOverride).HasMaxLength(5);
            entity.Property(e => e.AllowedDaysOverride).HasMaxLength(200);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Source).HasMaxLength(50).HasDefaultValue("Manual");
            entity.Property(e => e.SourceReference).HasMaxLength(200);
            entity.Property(e => e.Tags).HasMaxLength(500);
            entity.Property(e => e.CreatedBy).HasMaxLength(100);
            entity.Property(e => e.UpdatedBy).HasMaxLength(100);
            
            // Indexes
            entity.HasIndex(e => e.Name);
            entity.HasIndex(e => e.IsActive);
            entity.HasIndex(e => e.Priority);
            entity.HasIndex(e => e.CreatedAt);
        });

        // Configure Lead entity
        modelBuilder.Entity<Lead>(entity =>
        {
            entity.ToTable("Leads");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FirstName).HasMaxLength(100);
            entity.Property(e => e.LastName).HasMaxLength(100);
            entity.Property(e => e.FullName).HasMaxLength(200);
            entity.Property(e => e.Company).HasMaxLength(200);
            entity.Property(e => e.Title).HasMaxLength(100);
            entity.Property(e => e.PrimaryPhone).IsRequired().HasMaxLength(20);
            entity.Property(e => e.SecondaryPhone).HasMaxLength(20);
            entity.Property(e => e.MobilePhone).HasMaxLength(20);
            entity.Property(e => e.WorkPhone).HasMaxLength(20);
            entity.Property(e => e.HomePhone).HasMaxLength(20);
            entity.Property(e => e.PrimaryEmail).HasMaxLength(200);
            entity.Property(e => e.SecondaryEmail).HasMaxLength(200);
            entity.Property(e => e.AddressLine1).HasMaxLength(200);
            entity.Property(e => e.AddressLine2).HasMaxLength(200);
            entity.Property(e => e.City).HasMaxLength(100);
            entity.Property(e => e.State).HasMaxLength(100);
            entity.Property(e => e.PostalCode).HasMaxLength(20);
            entity.Property(e => e.Country).HasMaxLength(100);
            entity.Property(e => e.TimeZone).HasMaxLength(100);
            entity.Property(e => e.Status).HasMaxLength(50).HasDefaultValue("New");
            entity.Property(e => e.Priority).HasDefaultValue(5);
            entity.Property(e => e.CallAttempts).HasDefaultValue(0);
            entity.Property(e => e.Disposition).HasMaxLength(50);
            entity.Property(e => e.Notes).HasMaxLength(2000);
            entity.Property(e => e.CreatedBy).HasMaxLength(100);
            entity.Property(e => e.UpdatedBy).HasMaxLength(100);
            
            // Foreign key relationship
            entity.HasOne<List>()
                  .WithMany()
                  .HasForeignKey(e => e.ListId)
                  .OnDelete(DeleteBehavior.Restrict);
            
            // Indexes
            entity.HasIndex(e => e.PrimaryPhone);
            entity.HasIndex(e => e.PrimaryEmail);
            entity.HasIndex(e => e.ListId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.Disposition);
            entity.HasIndex(e => e.Priority);
            entity.HasIndex(e => e.CreatedAt);
        });

        // Configure Agent entity
        modelBuilder.Entity<Agent>(entity =>
        {
            entity.ToTable("Agents");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UserId).IsRequired().HasMaxLength(100);
            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(200);
            entity.Property(e => e.PhoneNumber).HasMaxLength(20);
            entity.Property(e => e.Status).HasMaxLength(20).HasDefaultValue("LoggedOut");
            entity.Property(e => e.IsLoggedIn).HasDefaultValue(false);
            entity.Property(e => e.IsOnCall).HasDefaultValue(false);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.MaxConcurrentCalls).HasDefaultValue(1);
            entity.Property(e => e.SkillLevel).HasDefaultValue(1);
            entity.Property(e => e.Tags).HasMaxLength(500);
            entity.Property(e => e.CreatedBy).HasMaxLength(100);
            entity.Property(e => e.UpdatedBy).HasMaxLength(100);
            
            // Indexes
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.Email);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.IsActive);
            entity.HasIndex(e => e.IsLoggedIn);
        });

        // Configure CallLog entity
        modelBuilder.Entity<CallLog>(entity =>
        {
            entity.ToTable("CallLogs");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.CallId).HasMaxLength(200);
            entity.Property(e => e.PhoneNumber).IsRequired().HasMaxLength(20);
            entity.Property(e => e.CallDirection).HasMaxLength(20).HasDefaultValue("Outbound");
            entity.Property(e => e.CallStatus).HasMaxLength(20).HasDefaultValue("Initiated");
            entity.Property(e => e.CallOutcome).HasMaxLength(50);
            entity.Property(e => e.DurationSeconds).HasDefaultValue(0);
            entity.Property(e => e.RingDurationSeconds).HasDefaultValue(0);
            entity.Property(e => e.TalkDurationSeconds).HasDefaultValue(0);
            entity.Property(e => e.CostCurrency).HasMaxLength(3);
            entity.Property(e => e.Disposition).HasMaxLength(50);
            entity.Property(e => e.Notes).HasMaxLength(2000);
            entity.Property(e => e.AnsweringMachineDetected).HasDefaultValue(false);
            entity.Property(e => e.WasTransferred).HasDefaultValue(false);
            entity.Property(e => e.TransferredToAgent).HasMaxLength(100);
            entity.Property(e => e.WasRecorded).HasDefaultValue(false);
            entity.Property(e => e.RecordingUrl).HasMaxLength(500);
            entity.Property(e => e.TimeZone).HasMaxLength(100);
            
            // Foreign key relationships
            entity.HasOne<Campaign>()
                  .WithMany()
                  .HasForeignKey(e => e.CampaignId)
                  .OnDelete(DeleteBehavior.Restrict);
            
            entity.HasOne<Lead>()
                  .WithMany()
                  .HasForeignKey(e => e.LeadId)
                  .OnDelete(DeleteBehavior.Restrict);
            
            // Indexes
            entity.HasIndex(e => e.CallId);
            entity.HasIndex(e => e.CampaignId);
            entity.HasIndex(e => e.LeadId);
            entity.HasIndex(e => e.AgentId);
            entity.HasIndex(e => e.CallDirection);
            entity.HasIndex(e => e.CallStatus);
            entity.HasIndex(e => e.CallOutcome);
            entity.HasIndex(e => e.StartedAt);
            entity.HasIndex(e => e.CreatedAt);
        });

        // Configure CampaignList junction table
        modelBuilder.Entity<CampaignList>(entity =>
        {
            entity.ToTable("CampaignLists");
            entity.HasKey(e => new { e.CampaignId, e.ListId });
            
            // Foreign key relationships
            entity.HasOne(e => e.Campaign)
                  .WithMany(c => c.CampaignLists)
                  .HasForeignKey(e => e.CampaignId)
                  .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasOne(e => e.List)
                  .WithMany(l => l.CampaignLists)
                  .HasForeignKey(e => e.ListId)
                  .OnDelete(DeleteBehavior.Cascade);
            
            entity.Property(e => e.Priority).HasDefaultValue(5);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.AllocationPercentage).HasDefaultValue(100);
            entity.Property(e => e.AddedBy).HasMaxLength(100);
            
            // Indexes
            entity.HasIndex(e => e.CampaignId);
            entity.HasIndex(e => e.ListId);
            entity.HasIndex(e => e.Priority);
            entity.HasIndex(e => e.IsActive);
        });
    }
}
