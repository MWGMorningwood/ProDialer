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
    
    // New supporting models for VICIdial compatibility
    public DbSet<LeadFilter> LeadFilters { get; set; } = null!;
    public DbSet<DncList> DncLists { get; set; } = null!;
    public DbSet<DncNumber> DncNumbers { get; set; } = null!;
    public DbSet<DispositionCategory> DispositionCategories { get; set; } = null!;
    public DbSet<DispositionCode> DispositionCodes { get; set; } = null!;
    public DbSet<AlternatePhone> AlternatePhones { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Campaign entity with VICIdial enhancements
        modelBuilder.Entity<Campaign>(entity =>
        {
            entity.ToTable("Campaigns");
            entity.HasKey(e => e.Id);
            
            // Core properties
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.DialingRatio).HasColumnType("decimal(4,1)").HasDefaultValue(1.0m);
            entity.Property(e => e.AdaptiveMaxLevel).HasColumnType("decimal(4,1)").HasDefaultValue(2.0m);
            entity.Property(e => e.MaxDropPercentage).HasColumnType("decimal(5,2)").HasDefaultValue(3.0m);
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
            
            // VICIdial enhancement properties
            entity.Property(e => e.DialPrefix).HasMaxLength(20);
            entity.Property(e => e.ManualDialPrefix).HasMaxLength(20);
            entity.Property(e => e.ThreeWayDialPrefix).HasMaxLength(20);
            entity.Property(e => e.NextAgentCall).HasMaxLength(50).HasDefaultValue("longest_wait_time");
            entity.Property(e => e.AvailableOnlyRatioTally).HasDefaultValue(false);
            entity.Property(e => e.AvailableOnlyTallyThreshold).HasDefaultValue(100);
            entity.Property(e => e.AlternateNumberDialing).HasDefaultValue(false);
            entity.Property(e => e.ScheduledCallbacks).HasDefaultValue(true);
            entity.Property(e => e.AllowInbound).HasDefaultValue(false);
            entity.Property(e => e.ParkExtension).HasMaxLength(20);
            entity.Property(e => e.ParkFileName).HasMaxLength(100);
            entity.Property(e => e.VoicemailExtension).HasMaxLength(20);
            entity.Property(e => e.AnsweringMachineExtension).HasMaxLength(20);
            entity.Property(e => e.XferConfADtmf).HasMaxLength(50);
            entity.Property(e => e.XferConfANumber).HasMaxLength(50);
            entity.Property(e => e.XferConfBDtmf).HasMaxLength(50);
            entity.Property(e => e.XferConfBNumber).HasMaxLength(50);
            entity.Property(e => e.ContainerEntry).HasMaxLength(100);
            entity.Property(e => e.ManualDialFilter).HasMaxLength(50);
            entity.Property(e => e.ForceResetHopper).HasDefaultValue(false);
            entity.Property(e => e.HopperDuplicateCheck).HasDefaultValue(true);
            entity.Property(e => e.GetCallLaunch).HasMaxLength(50).HasDefaultValue("NONE");
            entity.Property(e => e.RecyclingRules).HasColumnType("nvarchar(max)");
            entity.Property(e => e.EnableAutoRecycling).HasDefaultValue(false);
            entity.Property(e => e.MaxRecycleCount).HasDefaultValue(3);
            entity.Property(e => e.TotalDialedToday).HasDefaultValue(0);
            entity.Property(e => e.TotalContactsToday).HasDefaultValue(0);
            entity.Property(e => e.CurrentDropRate).HasColumnType("decimal(5,2)").HasDefaultValue(0);
            entity.Property(e => e.CustomFields).HasColumnType("nvarchar(max)");
            
            // Indexes
            entity.HasIndex(e => e.Name);
            entity.HasIndex(e => e.IsActive);
            entity.HasIndex(e => e.Priority);
            entity.HasIndex(e => e.CreatedAt);
            entity.HasIndex(e => e.NextAgentCall);
        });

        // Configure List entity with VICIdial enhancements
        modelBuilder.Entity<List>(entity =>
        {
            entity.ToTable("Lists");
            entity.HasKey(e => e.Id);
            
            // Core properties
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Priority).HasDefaultValue(5);
            entity.Property(e => e.CallStrategy).HasMaxLength(50).HasDefaultValue("Sequential");
            entity.Property(e => e.CreatedBy).HasMaxLength(100);
            entity.Property(e => e.UpdatedBy).HasMaxLength(100);
            
            // VICIdial enhancement properties
            entity.Property(e => e.AgentScriptOverride).HasMaxLength(50);
            entity.Property(e => e.CampaignCallerIdOverride).HasMaxLength(20);
            entity.Property(e => e.ListMixRatio).HasColumnType("decimal(5,2)").HasDefaultValue(1.0m);
            entity.Property(e => e.DuplicateCheckMethod).HasMaxLength(50).HasDefaultValue("PHONE");
            entity.Property(e => e.CustomFieldsCopy).HasDefaultValue(false);
            entity.Property(e => e.CustomFieldsModify).HasDefaultValue(true);
            entity.Property(e => e.PhoneValidationSettings).HasColumnType("nvarchar(max)");
            entity.Property(e => e.ResetLeadCalledCount).HasDefaultValue(true);
            entity.Property(e => e.ImportExportLog).HasMaxLength(500);
            entity.Property(e => e.PerformanceMetrics).HasColumnType("nvarchar(max)");
            entity.Property(e => e.TotalLeads).HasDefaultValue(0);
            entity.Property(e => e.CalledLeads).HasDefaultValue(0);
            entity.Property(e => e.ContactedLeads).HasDefaultValue(0);
            entity.Property(e => e.TimeZoneOverride).HasMaxLength(100);
            entity.Property(e => e.CallStartTimeOverride).HasMaxLength(5);
            entity.Property(e => e.CallEndTimeOverride).HasMaxLength(5);
            entity.Property(e => e.AllowedDaysOverride).HasMaxLength(200);
            entity.Property(e => e.Source).HasMaxLength(100).HasDefaultValue("Manual");
            entity.Property(e => e.SourceReference).HasMaxLength(500);
            entity.Property(e => e.CustomFieldsSchema).HasColumnType("nvarchar(max)");
            entity.Property(e => e.ScriptId).HasMaxLength(50);
            entity.Property(e => e.AnsweringMachineMessage).HasMaxLength(2000);
            entity.Property(e => e.DropInGroup).HasMaxLength(50);
            entity.Property(e => e.WebFormAddress).HasMaxLength(2000);
            entity.Property(e => e.WebFormAddress2).HasMaxLength(2000);
            entity.Property(e => e.WebFormAddress3).HasMaxLength(2000);
            entity.Property(e => e.ResetTime).HasMaxLength(100);
            entity.Property(e => e.TimezoneMethod).HasMaxLength(50).HasDefaultValue("COUNTRY_AND_AREA_CODE");
            entity.Property(e => e.LocalCallTime).HasMaxLength(50);
            entity.Property(e => e.OutboundCallerId).HasMaxLength(20);
            entity.Property(e => e.TransferConf1).HasMaxLength(50);
            entity.Property(e => e.TransferConf2).HasMaxLength(50);
            entity.Property(e => e.TransferConf3).HasMaxLength(50);
            entity.Property(e => e.TransferConf4).HasMaxLength(50);
            entity.Property(e => e.TransferConf5).HasMaxLength(50);
            entity.Property(e => e.ResetsToday).HasDefaultValue(0);
            entity.Property(e => e.Tags).HasMaxLength(500);
            
            // Indexes
            entity.HasIndex(e => e.Name);
            entity.HasIndex(e => e.IsActive);
            entity.HasIndex(e => e.Priority);
            entity.HasIndex(e => e.CreatedAt);
            entity.HasIndex(e => e.DuplicateCheckMethod);
            entity.HasIndex(e => e.Source);
            entity.HasIndex(e => e.Tags);
        });

        // Configure Lead entity with VICIdial enhancements
        modelBuilder.Entity<Lead>(entity =>
        {
            entity.ToTable("Leads");
            entity.HasKey(e => e.Id);
            
            // Core contact information
            entity.Property(e => e.FirstName).HasMaxLength(100);
            entity.Property(e => e.LastName).HasMaxLength(100);
            entity.Property(e => e.FullName).HasMaxLength(200);
            entity.Property(e => e.Company).HasMaxLength(200);
            entity.Property(e => e.Title).HasMaxLength(100);
            entity.Property(e => e.PrimaryPhone).IsRequired().HasMaxLength(20);
            entity.Property(e => e.PhoneCode).HasMaxLength(4).HasDefaultValue("1");
            entity.Property(e => e.PhoneNumberRaw).HasMaxLength(20);
            entity.Property(e => e.PhoneValidationStatus).HasMaxLength(20).HasDefaultValue("UNVALIDATED");
            entity.Property(e => e.PhoneCarrier).HasMaxLength(100);
            entity.Property(e => e.PhoneType).HasMaxLength(20);
            entity.Property(e => e.SecondaryPhone).HasMaxLength(20);
            entity.Property(e => e.MobilePhone).HasMaxLength(20);
            entity.Property(e => e.WorkPhone).HasMaxLength(20);
            entity.Property(e => e.HomePhone).HasMaxLength(20);
            entity.Property(e => e.AlternatePhones).HasMaxLength(1000);
            entity.Property(e => e.VendorLeadCode).HasMaxLength(20);
            entity.Property(e => e.SourceId).HasMaxLength(50);
            entity.Property(e => e.MiddleInitial).HasMaxLength(1);
            entity.Property(e => e.AddressLine3).HasMaxLength(200);
            entity.Property(e => e.Province).HasMaxLength(100);
            entity.Property(e => e.SecurityPhrase).HasMaxLength(100);
            entity.Property(e => e.Rank).HasDefaultValue(0);
            entity.Property(e => e.CalledCount).HasDefaultValue(0);
            entity.Property(e => e.QualityScore).HasDefaultValue(50);
            entity.Property(e => e.LifecycleStage).HasMaxLength(20).HasDefaultValue("NEW");
            entity.Property(e => e.RecycleCount).HasDefaultValue(0);
            entity.Property(e => e.ComplianceFlags).HasColumnType("nvarchar(max)");
            entity.Property(e => e.CallbackAppointment).HasColumnType("nvarchar(max)");
            entity.Property(e => e.Owner).HasMaxLength(20);
            entity.Property(e => e.GmtOffset).HasColumnType("decimal(3,1)").HasDefaultValue(0);
            entity.Property(e => e.GmtOffsetNow).HasColumnType("decimal(3,1)").HasDefaultValue(0);
            entity.Property(e => e.PrimaryEmail).HasMaxLength(200);
            entity.Property(e => e.SecondaryEmail).HasMaxLength(200);
            entity.Property(e => e.AddressLine1).HasMaxLength(200);
            entity.Property(e => e.AddressLine2).HasMaxLength(200);
            entity.Property(e => e.City).HasMaxLength(100);
            entity.Property(e => e.State).HasMaxLength(100);
            entity.Property(e => e.PostalCode).HasMaxLength(20);
            entity.Property(e => e.Country).HasMaxLength(100);
            entity.Property(e => e.TimeZone).HasMaxLength(100);
            entity.Property(e => e.Status).HasMaxLength(6).HasDefaultValue("NEW");
            entity.Property(e => e.User).HasMaxLength(20);
            entity.Property(e => e.Priority).HasDefaultValue(5);
            entity.Property(e => e.CallAttempts).HasDefaultValue(0);
            entity.Property(e => e.CalledSinceLastReset).HasDefaultValue(false);
            entity.Property(e => e.LastHandlerAgent).HasMaxLength(100);
            entity.Property(e => e.Notes).HasMaxLength(2000);
            entity.Property(e => e.LastCallOutcome).HasMaxLength(50);
            entity.Property(e => e.Disposition).HasMaxLength(50);
            entity.Property(e => e.Gender).HasMaxLength(10);
            entity.Property(e => e.MaritalStatus).HasMaxLength(20);
            entity.Property(e => e.Income).HasMaxLength(100);
            entity.Property(e => e.Education).HasMaxLength(100);
            entity.Property(e => e.Interests).HasMaxLength(200);
            entity.Property(e => e.Source).HasMaxLength(200);
            entity.Property(e => e.SourceCampaign).HasMaxLength(200);
            entity.Property(e => e.LeadValue).HasColumnType("decimal(18,2)");
            entity.Property(e => e.ExpectedRevenue).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Tags).HasMaxLength(500);
            entity.Property(e => e.CustomFields).HasColumnType("nvarchar(max)");
            entity.Property(e => e.IsExcluded).HasDefaultValue(false);
            entity.Property(e => e.ExclusionReason).HasMaxLength(200);
            entity.Property(e => e.HasOptedOut).HasDefaultValue(false);
            entity.Property(e => e.ModifyDate);
            entity.Property(e => e.ConversionTracking).HasColumnType("nvarchar(max)");
            entity.Property(e => e.InteractionHistory).HasColumnType("nvarchar(max)");
            entity.Property(e => e.ScoringFactors).HasColumnType("nvarchar(max)");
            entity.Property(e => e.AttributionData).HasColumnType("nvarchar(max)");
            entity.Property(e => e.CreatedBy).HasMaxLength(100);
            entity.Property(e => e.UpdatedBy).HasMaxLength(100);
            
            // Foreign key relationship
            entity.HasOne(e => e.List)
                  .WithMany(l => l.Leads)
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
            entity.HasIndex(e => e.PhoneValidationStatus);
            entity.HasIndex(e => e.LifecycleStage);
            entity.HasIndex(e => e.User);
            entity.HasIndex(e => e.LastCalledAt);
            entity.HasIndex(e => e.NextCallAt);
            entity.HasIndex(e => e.ScheduledCallbackAt);
            entity.HasIndex(e => e.IsExcluded);
            entity.HasIndex(e => e.HasOptedOut);
            entity.HasIndex(e => e.Tags);
        });

        // Configure Agent entity
        modelBuilder.Entity<Agent>(entity =>
        {
            entity.ToTable("Agents");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Email).HasMaxLength(200);
            entity.Property(e => e.PhoneNumber).HasMaxLength(20);
            entity.Property(e => e.Status).HasMaxLength(50).HasDefaultValue("Offline");
            entity.Property(e => e.SkillLevel).HasDefaultValue(5);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.MaxConcurrentCalls).HasDefaultValue(1);
            
            // Indexes
            entity.HasIndex(e => e.FirstName);
            entity.HasIndex(e => e.LastName);
            entity.HasIndex(e => e.Email);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.IsActive);
        });

        // Configure CallLog entity with VICIdial enhancements
        modelBuilder.Entity<CallLog>(entity =>
        {
            entity.ToTable("CallLogs");
            entity.HasKey(e => e.Id);
            
            // Core call information
            entity.Property(e => e.CallId).HasMaxLength(200);
            entity.Property(e => e.PhoneNumber).IsRequired().HasMaxLength(20);
            entity.Property(e => e.CallDirection).HasMaxLength(20).HasDefaultValue("Outbound");
            entity.Property(e => e.CallStatus).HasMaxLength(20).HasDefaultValue("Initiated");
            entity.Property(e => e.CallProgress).HasMaxLength(20);
            entity.Property(e => e.AmdResult).HasMaxLength(20);
            entity.Property(e => e.CallOutcome).HasMaxLength(50);
            entity.Property(e => e.HangupCause).HasMaxLength(50);
            entity.Property(e => e.Disposition).HasMaxLength(50);
            entity.Property(e => e.Notes).HasMaxLength(2000);
            entity.Property(e => e.RecordingUrl).HasMaxLength(500);
            entity.Property(e => e.AgentId).HasMaxLength(100);
            entity.Property(e => e.DurationSeconds).HasDefaultValue(0);
            entity.Property(e => e.RingDurationSeconds).HasDefaultValue(0);
            entity.Property(e => e.TalkDurationSeconds).HasDefaultValue(0);
            entity.Property(e => e.Cost).HasColumnType("decimal(18,4)");
            entity.Property(e => e.CostCurrency).HasMaxLength(3);
            entity.Property(e => e.AnsweringMachineDetected).HasDefaultValue(false);
            entity.Property(e => e.WasTransferred).HasDefaultValue(false);
            entity.Property(e => e.TransferredToAgent).HasMaxLength(100);
            entity.Property(e => e.WasRecorded).HasDefaultValue(false);
            entity.Property(e => e.TechnicalDetails).HasMaxLength(1000);
            entity.Property(e => e.ErrorMessage).HasMaxLength(500);
            entity.Property(e => e.Tags).HasMaxLength(500);
            entity.Property(e => e.CustomFields).HasColumnType("nvarchar(max)");
            entity.Property(e => e.TimeZone).HasMaxLength(100);
            entity.Property(e => e.LeadAttemptNumber).HasDefaultValue(1);
            entity.Property(e => e.WrapupSeconds).HasDefaultValue(0);
            entity.Property(e => e.ComplianceFlags).HasColumnType("nvarchar(max)");
            entity.Property(e => e.WasMonitored).HasDefaultValue(false);
            entity.Property(e => e.ThreeWayParticipants).HasColumnType("nvarchar(max)");
            entity.Property(e => e.TransferDetails).HasColumnType("nvarchar(max)");
            
            // Foreign key relationships
            entity.HasOne(e => e.Campaign)
                  .WithMany()
                  .HasForeignKey(e => e.CampaignId)
                  .OnDelete(DeleteBehavior.Restrict);
            
            entity.HasOne(e => e.Lead)
                  .WithMany(l => l.CallLogs)
                  .HasForeignKey(e => e.LeadId)
                  .OnDelete(DeleteBehavior.Restrict);
            
            entity.HasOne(e => e.DispositionCode)
                  .WithMany()
                  .HasForeignKey(e => e.DispositionCodeId)
                  .OnDelete(DeleteBehavior.SetNull);
            
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
            entity.HasIndex(e => e.Disposition);
            entity.HasIndex(e => e.DispositionCodeId);
            entity.HasIndex(e => e.CallProgress);
            entity.HasIndex(e => e.AmdResult);
            entity.HasIndex(e => e.WasRecorded);
            entity.HasIndex(e => e.WasTransferred);
        });

        // Configure LeadFilter entity
        modelBuilder.Entity<LeadFilter>(entity =>
        {
            entity.ToTable("LeadFilters");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.FilterType).HasMaxLength(50).HasDefaultValue("RULE_BASED");
            entity.Property(e => e.SqlFilter).HasMaxLength(2000);
            entity.Property(e => e.FilterRules).HasColumnType("nvarchar(max)");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Priority).HasDefaultValue(5);
            entity.Property(e => e.UserGroup).HasMaxLength(100);
            entity.Property(e => e.MatchingLeadCount).HasDefaultValue(0);
            entity.Property(e => e.CreatedBy).HasMaxLength(100);
            entity.Property(e => e.UpdatedBy).HasMaxLength(100);
            
            entity.HasIndex(e => e.Name);
            entity.HasIndex(e => e.FilterType);
            entity.HasIndex(e => e.Priority);
            entity.HasIndex(e => e.IsActive);
            entity.HasIndex(e => e.CreatedAt);
        });

        // Configure DncList entity
        modelBuilder.Entity<DncList>(entity =>
        {
            entity.ToTable("DncLists");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.ListType).HasMaxLength(50).HasDefaultValue("INTERNAL");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Scope).HasMaxLength(50).HasDefaultValue("SYSTEM_WIDE");
            entity.Property(e => e.TotalNumbers).HasDefaultValue(0);
            entity.Property(e => e.Source).HasMaxLength(100).HasDefaultValue("MANUAL");
            entity.Property(e => e.AutoScrubbing).HasDefaultValue(true);
            entity.Property(e => e.CreatedBy).HasMaxLength(100);
            entity.Property(e => e.UpdatedBy).HasMaxLength(100);
            
            entity.HasOne(e => e.Campaign)
                  .WithMany()
                  .HasForeignKey(e => e.CampaignId)
                  .OnDelete(DeleteBehavior.SetNull);
            
            entity.HasOne(e => e.List)
                  .WithMany()
                  .HasForeignKey(e => e.ListId)
                  .OnDelete(DeleteBehavior.SetNull);
            
            entity.HasIndex(e => e.Name);
            entity.HasIndex(e => e.Scope);
            entity.HasIndex(e => e.IsActive);
            entity.HasIndex(e => e.CreatedAt);
        });

        // Configure DncNumber entity
        modelBuilder.Entity<DncNumber>(entity =>
        {
            entity.ToTable("DncNumbers");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.PhoneNumber).IsRequired().HasMaxLength(20);
            entity.Property(e => e.PhoneCode).HasMaxLength(4).HasDefaultValue("1");
            entity.Property(e => e.Reason).HasMaxLength(100).HasDefaultValue("OPT_OUT");
            entity.Property(e => e.AddedAt);
            entity.Property(e => e.Notes).HasMaxLength(500);
            entity.Property(e => e.AddedBy).HasMaxLength(100);
            
            entity.HasOne(e => e.DncList)
                  .WithMany(d => d.DncNumbers)
                  .HasForeignKey(e => e.DncListId)
                  .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasIndex(e => e.PhoneNumber);
            entity.HasIndex(e => e.DncListId);
            entity.HasIndex(e => e.AddedAt);
        });

        // Configure DispositionCategory entity
        modelBuilder.Entity<DispositionCategory>(entity =>
        {
            entity.ToTable("DispositionCategories");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Code).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Color).HasMaxLength(7).HasDefaultValue("#808080");
            entity.Property(e => e.DisplayOrder).HasDefaultValue(0);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            
            entity.HasIndex(e => e.Code).IsUnique();
            entity.HasIndex(e => e.Name);
            entity.HasIndex(e => e.IsActive);
        });

        // Configure DispositionCode entity
        modelBuilder.Entity<DispositionCode>(entity =>
        {
            entity.ToTable("DispositionCodes");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Code).IsRequired().HasMaxLength(20);
            entity.Property(e => e.IsContact).HasDefaultValue(false);
            entity.Property(e => e.IsSale).HasDefaultValue(false);
            entity.Property(e => e.ShouldRecycle).HasDefaultValue(false);
            entity.Property(e => e.RecycleDelayHours).HasDefaultValue(24);
            entity.Property(e => e.RequiresCallback).HasDefaultValue(false);
            entity.Property(e => e.RequiredFields).HasColumnType("nvarchar(max)");
            entity.Property(e => e.AutoActions).HasColumnType("nvarchar(max)");
            entity.Property(e => e.HotKey).HasMaxLength(1);
            entity.Property(e => e.DisplayOrder).HasDefaultValue(0);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.UsageCount).HasDefaultValue(0);
            
            entity.HasOne(e => e.Category)
                  .WithMany(c => c.DispositionCodes)
                  .HasForeignKey(e => e.CategoryId)
                  .OnDelete(DeleteBehavior.Restrict);
            
            entity.HasIndex(e => e.Code).IsUnique();
            entity.HasIndex(e => e.Name);
            entity.HasIndex(e => e.CategoryId);
            entity.HasIndex(e => e.IsActive);
            entity.HasIndex(e => e.HotKey);
        });

        // Configure AlternatePhone entity
        modelBuilder.Entity<AlternatePhone>(entity =>
        {
            entity.ToTable("AlternatePhones");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.PhoneNumber).IsRequired().HasMaxLength(20);
            entity.Property(e => e.PhoneCode).HasMaxLength(4).HasDefaultValue("1");
            entity.Property(e => e.PhoneType).HasMaxLength(20).HasDefaultValue("OTHER");
            entity.Property(e => e.Priority).HasDefaultValue(1);
            entity.Property(e => e.Status).HasMaxLength(20).HasDefaultValue("ACTIVE");
            entity.Property(e => e.CallAttempts).HasDefaultValue(0);
            entity.Property(e => e.LastCallOutcome).HasMaxLength(50);
            entity.Property(e => e.IsValidated).HasDefaultValue(false);
            entity.Property(e => e.ValidationResult).HasMaxLength(20).HasDefaultValue("UNKNOWN");
            entity.Property(e => e.Carrier).HasMaxLength(100);
            entity.Property(e => e.LineType).HasMaxLength(20);
            entity.Property(e => e.TimeZone).HasMaxLength(100);
            entity.Property(e => e.BestCallTime).HasColumnType("nvarchar(max)");
            entity.Property(e => e.Notes).HasMaxLength(500);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.CreatedBy).HasMaxLength(100);
            entity.Property(e => e.UpdatedBy).HasMaxLength(100);
            
            entity.HasOne(e => e.Lead)
                  .WithMany()
                  .HasForeignKey(e => e.LeadId)
                  .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasIndex(e => e.LeadId);
            entity.HasIndex(e => e.PhoneNumber);
            entity.HasIndex(e => e.PhoneType);
            entity.HasIndex(e => e.Priority);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.ValidationResult);
            entity.HasIndex(e => e.CreatedAt);
        });

        // Configure Agent entity
        modelBuilder.Entity<Agent>(entity =>
        {
            entity.ToTable("Agents");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Email).HasMaxLength(200);
            entity.Property(e => e.PhoneNumber).HasMaxLength(20);
            entity.Property(e => e.Status).HasMaxLength(50).HasDefaultValue("Offline");
            entity.Property(e => e.SkillLevel).HasDefaultValue(5);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.MaxConcurrentCalls).HasDefaultValue(1);
            
            entity.HasIndex(e => e.FirstName);
            entity.HasIndex(e => e.LastName);
            entity.HasIndex(e => e.Email);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.IsActive);
        });

        // Configure CampaignList junction table
        modelBuilder.Entity<CampaignList>(entity =>
        {
            entity.ToTable("CampaignLists");
            entity.HasKey(e => new { e.CampaignId, e.ListId });
            
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
            entity.Property(e => e.AddedBy).HasMaxLength(100);
            
            entity.HasIndex(e => e.CampaignId);
            entity.HasIndex(e => e.ListId);
            entity.HasIndex(e => e.Priority);
            entity.HasIndex(e => e.IsActive);
        });
    }
}
