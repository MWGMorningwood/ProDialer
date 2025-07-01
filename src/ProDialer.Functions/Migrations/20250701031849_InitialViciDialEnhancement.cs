using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProDialer.Functions.Migrations
{
    /// <inheritdoc />
    public partial class InitialViciDialEnhancement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Agents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "Offline"),
                    IsLoggedIn = table.Column<bool>(type: "bit", nullable: false),
                    IsOnCall = table.Column<bool>(type: "bit", nullable: false),
                    ActiveCalls = table.Column<int>(type: "int", nullable: false),
                    MaxConcurrentCalls = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    SkillLevel = table.Column<int>(type: "int", nullable: false, defaultValue: 5),
                    QualifiedCampaigns = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Languages = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    TimeZone = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CurrentSessionStartedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastLoggedOutAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TodayLoggedInMinutes = table.Column<int>(type: "int", nullable: false),
                    TodayCallsHandled = table.Column<int>(type: "int", nullable: false),
                    TodayTalkTimeMinutes = table.Column<int>(type: "int", nullable: false),
                    Role = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Supervisor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    Extension = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CommunicationEndpoint = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Tags = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CustomFields = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Agents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DispositionCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Code = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Color = table.Column<string>(type: "nvarchar(7)", maxLength: 7, nullable: false, defaultValue: "#808080"),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DispositionCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LeadFilters",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    FilterType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "RULE_BASED"),
                    SqlFilter = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    FilterRules = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    Priority = table.Column<int>(type: "int", nullable: false, defaultValue: 5),
                    UserGroup = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    MatchingLeadCount = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    LastAppliedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeadFilters", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Lists",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Priority = table.Column<int>(type: "int", nullable: false, defaultValue: 5),
                    CallStrategy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "Sequential"),
                    MaxCallAttempts = table.Column<int>(type: "int", nullable: true),
                    MinCallInterval = table.Column<int>(type: "int", nullable: true),
                    TimeZoneOverride = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CallStartTimeOverride = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: true),
                    CallEndTimeOverride = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: true),
                    AllowedDaysOverride = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    Source = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false, defaultValue: "Manual"),
                    SourceReference = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    TotalLeads = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    CalledLeads = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    ContactedLeads = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    CustomFieldsSchema = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ScriptId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    AgentScriptOverride = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CampaignCallerIdOverride = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    ListMixRatio = table.Column<decimal>(type: "decimal(5,2)", nullable: false, defaultValue: 1.0m),
                    DuplicateCheckMethod = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "PHONE"),
                    CustomFieldsCopy = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CustomFieldsModify = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    ResetLeadCalledCount = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    PhoneValidationSettings = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ImportExportLog = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PerformanceMetrics = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AnsweringMachineMessage = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    DropInGroup = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    WebFormAddress = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    WebFormAddress2 = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    WebFormAddress3 = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ResetTime = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    TimezoneMethod = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "COUNTRY_AND_AREA_CODE"),
                    LocalCallTime = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ExpirationDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OutboundCallerId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    TransferConf1 = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    TransferConf2 = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    TransferConf3 = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    TransferConf4 = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    TransferConf5 = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ResetsToday = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    LastResetAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Tags = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lists", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DispositionCodes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Code = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    IsContact = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    IsSale = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    ShouldRecycle = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    RecycleDelayHours = table.Column<int>(type: "int", nullable: false, defaultValue: 24),
                    RequiresCallback = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    RequiredFields = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AutoActions = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HotKey = table.Column<string>(type: "nvarchar(1)", maxLength: 1, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    UsageCount = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DispositionCodes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DispositionCodes_DispositionCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "DispositionCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Campaigns",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    DialMethod = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DialingRatio = table.Column<decimal>(type: "decimal(4,1)", nullable: false, defaultValue: 1.0m),
                    ApplyRatioToIdleAgentsOnly = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    AdaptiveMaxLevel = table.Column<decimal>(type: "decimal(4,1)", nullable: false, defaultValue: 2.0m),
                    DialTimeout = table.Column<int>(type: "int", nullable: false),
                    MaxConcurrentCalls = table.Column<int>(type: "int", nullable: false, defaultValue: 50),
                    DialPrefix = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    ManualDialPrefix = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    ThreeWayDialPrefix = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    AvailableOnlyRatioTally = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    AvailableOnlyTallyThreshold = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "100"),
                    NextAgentCall = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "longest_wait_time"),
                    ParkExtension = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    ParkFileName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    VoicemailExtension = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    AlternateNumberDialing = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    ScheduledCallbacks = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    AllowInbound = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    ForceResetHopper = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    HopperDuplicateCheck = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    GetCallLaunch = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "NONE"),
                    AnsweringMachineExtension = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    TimeZone = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false, defaultValue: "UTC"),
                    CallStartTime = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: false, defaultValue: "08:00"),
                    CallEndTime = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: false, defaultValue: "20:00"),
                    AllowedDaysOfWeek = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false, defaultValue: "Monday,Tuesday,Wednesday,Thursday,Friday"),
                    RespectLeadTimeZone = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    MinCallInterval = table.Column<int>(type: "int", nullable: false, defaultValue: 60),
                    MaxCallAttempts = table.Column<int>(type: "int", nullable: false, defaultValue: 3),
                    CallAttemptDelay = table.Column<int>(type: "int", nullable: false, defaultValue: 15),
                    EnableAnsweringMachineDetection = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    AnsweringMachineAction = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "Hangup"),
                    AnsweringMachineMessage = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    LeadOrder = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    RandomizeLeadOrder = table.Column<bool>(type: "bit", nullable: false),
                    LeadOrderSecondary = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    HopperLevel = table.Column<int>(type: "int", nullable: false),
                    OutboundCallerId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    TransferConf1 = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    TransferConf2 = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    TransferConf3 = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    TransferConf4 = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    TransferConf5 = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    XferConfADtmf = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    XferConfANumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    XferConfBDtmf = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    XferConfBNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ContainerEntry = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ManualDialFilter = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DispoCallUrl = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    WebForm1 = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    WebForm2 = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    WebForm3 = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    WrapupSeconds = table.Column<int>(type: "int", nullable: false),
                    DialStatuses = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    LeadFilterId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DropCallTimer = table.Column<int>(type: "int", nullable: false),
                    SafeHarborMessage = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    MaxDropPercentage = table.Column<decimal>(type: "decimal(5,2)", nullable: false, defaultValue: 3.0m),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    Priority = table.Column<int>(type: "int", nullable: false, defaultValue: 5),
                    LocationRestrictions = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    EnableCallRecording = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CustomFields = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RecyclingRules = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EnableAutoRecycling = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    MaxRecycleCount = table.Column<int>(type: "int", nullable: false, defaultValue: 3),
                    StatsLastUpdated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TotalDialedToday = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    TotalContactsToday = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    CurrentDropRate = table.Column<decimal>(type: "decimal(5,2)", nullable: false, defaultValue: 0m),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LeadFilterId1 = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Campaigns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Campaigns_LeadFilters_LeadFilterId1",
                        column: x => x.LeadFilterId1,
                        principalTable: "LeadFilters",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Leads",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ListId = table.Column<int>(type: "int", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    LastName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FullName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Company = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Title = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PrimaryPhone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    PhoneCode = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false, defaultValue: "1"),
                    PhoneNumberRaw = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    PhoneValidationStatus = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "UNVALIDATED"),
                    PhoneCarrier = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PhoneType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    SecondaryPhone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    MobilePhone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    WorkPhone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    HomePhone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    AlternatePhones = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    VendorLeadCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    SourceId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    MiddleInitial = table.Column<string>(type: "nvarchar(1)", maxLength: 1, nullable: true),
                    AddressLine3 = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Province = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SecurityPhrase = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Rank = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    CalledCount = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    QualityScore = table.Column<int>(type: "int", nullable: false, defaultValue: 50),
                    LifecycleStage = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "NEW"),
                    RecycleCount = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    LastRecycledAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ComplianceFlags = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CallbackAppointment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Owner = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    EntryListId = table.Column<int>(type: "int", nullable: true),
                    GmtOffset = table.Column<decimal>(type: "decimal(3,1)", nullable: false, defaultValue: 0m),
                    GmtOffsetNow = table.Column<decimal>(type: "decimal(3,1)", nullable: false, defaultValue: 0m),
                    PrimaryEmail = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    SecondaryEmail = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    AddressLine1 = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    AddressLine2 = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    City = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    State = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PostalCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Country = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    TimeZone = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(6)", maxLength: 6, nullable: false, defaultValue: "NEW"),
                    User = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Priority = table.Column<int>(type: "int", nullable: false, defaultValue: 5),
                    CallAttempts = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    LastLocalCallTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CalledSinceLastReset = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    LastCalledAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastContactedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NextCallAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ScheduledCallbackAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastHandlerAgent = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    LastCallOutcome = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Disposition = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Gender = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    DateOfBirth = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MaritalStatus = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Income = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Education = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Interests = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Source = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    SourceCampaign = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    LeadValue = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ExpectedRevenue = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Tags = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CustomFields = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsExcluded = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    ExclusionReason = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ExcludedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    HasOptedOut = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    OptedOutAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifyDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ConversionTracking = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    InteractionHistory = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ScoringFactors = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AttributionData = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Leads", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Leads_Lists_ListId",
                        column: x => x.ListId,
                        principalTable: "Lists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CampaignLists",
                columns: table => new
                {
                    CampaignId = table.Column<int>(type: "int", nullable: false),
                    ListId = table.Column<int>(type: "int", nullable: false),
                    Id = table.Column<int>(type: "int", nullable: false),
                    Priority = table.Column<int>(type: "int", nullable: false, defaultValue: 5),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    AllocationPercentage = table.Column<int>(type: "int", nullable: false),
                    MaxCallsPerHour = table.Column<int>(type: "int", nullable: true),
                    AddedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AddedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CampaignLists", x => new { x.CampaignId, x.ListId });
                    table.ForeignKey(
                        name: "FK_CampaignLists_Campaigns_CampaignId",
                        column: x => x.CampaignId,
                        principalTable: "Campaigns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CampaignLists_Lists_ListId",
                        column: x => x.ListId,
                        principalTable: "Lists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DncLists",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ListType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "INTERNAL"),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    Scope = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "SYSTEM_WIDE"),
                    CampaignId = table.Column<int>(type: "int", nullable: true),
                    ListId = table.Column<int>(type: "int", nullable: true),
                    TotalNumbers = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    LastUpdated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Source = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false, defaultValue: "MANUAL"),
                    ExpirationDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AutoScrubbing = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DncLists", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DncLists_Campaigns_CampaignId",
                        column: x => x.CampaignId,
                        principalTable: "Campaigns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_DncLists_Lists_ListId",
                        column: x => x.ListId,
                        principalTable: "Lists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "AlternatePhones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LeadId = table.Column<int>(type: "int", nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    PhoneCode = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false, defaultValue: "1"),
                    PhoneType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "OTHER"),
                    Priority = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "ACTIVE"),
                    CallAttempts = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    LastCalledAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastCallOutcome = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IsValidated = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    ValidationResult = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "UNKNOWN"),
                    Carrier = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    LineType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    TimeZone = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    BestCallTime = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlternatePhones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AlternatePhones_Leads_LeadId",
                        column: x => x.LeadId,
                        principalTable: "Leads",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DncNumbers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DncListId = table.Column<int>(type: "int", nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    PhoneCode = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false, defaultValue: "1"),
                    Reason = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false, defaultValue: "OPT_OUT"),
                    AddedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    AddedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DncNumbers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DncNumbers_DncLists_DncListId",
                        column: x => x.DncListId,
                        principalTable: "DncLists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CallLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CampaignId = table.Column<int>(type: "int", nullable: false),
                    LeadId = table.Column<int>(type: "int", nullable: false),
                    AgentId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CallId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CallDirection = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Outbound"),
                    CallStatus = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Initiated"),
                    CallProgress = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    AmdResult = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    CallOutcome = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    HangupCause = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    SipResponseCode = table.Column<int>(type: "int", nullable: true),
                    StartedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AnsweredAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DurationSeconds = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    RingDurationSeconds = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    TalkDurationSeconds = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    Cost = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    CostCurrency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: true),
                    Disposition = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    AnsweringMachineDetected = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    AnsweringMachineConfidence = table.Column<int>(type: "int", nullable: true),
                    WasTransferred = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    TransferredToAgent = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    WasRecorded = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    RecordingUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    QualityScore = table.Column<int>(type: "int", nullable: true),
                    TechnicalDetails = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Tags = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CustomFields = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TimeZone = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    LocalTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LeadAttemptNumber = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    DispositionCodeId = table.Column<int>(type: "int", nullable: true),
                    WrapupSeconds = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    ComplianceFlags = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    WasMonitored = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    ThreeWayParticipants = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TransferDetails = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AlternatePhoneId = table.Column<int>(type: "int", nullable: true),
                    CampaignId1 = table.Column<int>(type: "int", nullable: true),
                    DispositionCodeId1 = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CallLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CallLogs_AlternatePhones_AlternatePhoneId",
                        column: x => x.AlternatePhoneId,
                        principalTable: "AlternatePhones",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CallLogs_Campaigns_CampaignId",
                        column: x => x.CampaignId,
                        principalTable: "Campaigns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CallLogs_Campaigns_CampaignId1",
                        column: x => x.CampaignId1,
                        principalTable: "Campaigns",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CallLogs_DispositionCodes_DispositionCodeId",
                        column: x => x.DispositionCodeId,
                        principalTable: "DispositionCodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_CallLogs_DispositionCodes_DispositionCodeId1",
                        column: x => x.DispositionCodeId1,
                        principalTable: "DispositionCodes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CallLogs_Leads_LeadId",
                        column: x => x.LeadId,
                        principalTable: "Leads",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Agents_Email",
                table: "Agents",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_Agents_FirstName",
                table: "Agents",
                column: "FirstName");

            migrationBuilder.CreateIndex(
                name: "IX_Agents_IsActive",
                table: "Agents",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Agents_LastName",
                table: "Agents",
                column: "LastName");

            migrationBuilder.CreateIndex(
                name: "IX_Agents_Status",
                table: "Agents",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_AlternatePhones_CreatedAt",
                table: "AlternatePhones",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_AlternatePhones_LeadId",
                table: "AlternatePhones",
                column: "LeadId");

            migrationBuilder.CreateIndex(
                name: "IX_AlternatePhones_PhoneNumber",
                table: "AlternatePhones",
                column: "PhoneNumber");

            migrationBuilder.CreateIndex(
                name: "IX_AlternatePhones_PhoneType",
                table: "AlternatePhones",
                column: "PhoneType");

            migrationBuilder.CreateIndex(
                name: "IX_AlternatePhones_Priority",
                table: "AlternatePhones",
                column: "Priority");

            migrationBuilder.CreateIndex(
                name: "IX_AlternatePhones_Status",
                table: "AlternatePhones",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_AlternatePhones_ValidationResult",
                table: "AlternatePhones",
                column: "ValidationResult");

            migrationBuilder.CreateIndex(
                name: "IX_CallLogs_AgentId",
                table: "CallLogs",
                column: "AgentId");

            migrationBuilder.CreateIndex(
                name: "IX_CallLogs_AlternatePhoneId",
                table: "CallLogs",
                column: "AlternatePhoneId");

            migrationBuilder.CreateIndex(
                name: "IX_CallLogs_AmdResult",
                table: "CallLogs",
                column: "AmdResult");

            migrationBuilder.CreateIndex(
                name: "IX_CallLogs_CallDirection",
                table: "CallLogs",
                column: "CallDirection");

            migrationBuilder.CreateIndex(
                name: "IX_CallLogs_CallId",
                table: "CallLogs",
                column: "CallId");

            migrationBuilder.CreateIndex(
                name: "IX_CallLogs_CallOutcome",
                table: "CallLogs",
                column: "CallOutcome");

            migrationBuilder.CreateIndex(
                name: "IX_CallLogs_CallProgress",
                table: "CallLogs",
                column: "CallProgress");

            migrationBuilder.CreateIndex(
                name: "IX_CallLogs_CallStatus",
                table: "CallLogs",
                column: "CallStatus");

            migrationBuilder.CreateIndex(
                name: "IX_CallLogs_CampaignId",
                table: "CallLogs",
                column: "CampaignId");

            migrationBuilder.CreateIndex(
                name: "IX_CallLogs_CampaignId1",
                table: "CallLogs",
                column: "CampaignId1");

            migrationBuilder.CreateIndex(
                name: "IX_CallLogs_CreatedAt",
                table: "CallLogs",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_CallLogs_Disposition",
                table: "CallLogs",
                column: "Disposition");

            migrationBuilder.CreateIndex(
                name: "IX_CallLogs_DispositionCodeId",
                table: "CallLogs",
                column: "DispositionCodeId");

            migrationBuilder.CreateIndex(
                name: "IX_CallLogs_DispositionCodeId1",
                table: "CallLogs",
                column: "DispositionCodeId1");

            migrationBuilder.CreateIndex(
                name: "IX_CallLogs_LeadId",
                table: "CallLogs",
                column: "LeadId");

            migrationBuilder.CreateIndex(
                name: "IX_CallLogs_StartedAt",
                table: "CallLogs",
                column: "StartedAt");

            migrationBuilder.CreateIndex(
                name: "IX_CallLogs_WasRecorded",
                table: "CallLogs",
                column: "WasRecorded");

            migrationBuilder.CreateIndex(
                name: "IX_CallLogs_WasTransferred",
                table: "CallLogs",
                column: "WasTransferred");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignLists_CampaignId",
                table: "CampaignLists",
                column: "CampaignId");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignLists_IsActive",
                table: "CampaignLists",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignLists_ListId",
                table: "CampaignLists",
                column: "ListId");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignLists_Priority",
                table: "CampaignLists",
                column: "Priority");

            migrationBuilder.CreateIndex(
                name: "IX_Campaigns_CreatedAt",
                table: "Campaigns",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Campaigns_IsActive",
                table: "Campaigns",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Campaigns_LeadFilterId1",
                table: "Campaigns",
                column: "LeadFilterId1");

            migrationBuilder.CreateIndex(
                name: "IX_Campaigns_Name",
                table: "Campaigns",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Campaigns_NextAgentCall",
                table: "Campaigns",
                column: "NextAgentCall");

            migrationBuilder.CreateIndex(
                name: "IX_Campaigns_Priority",
                table: "Campaigns",
                column: "Priority");

            migrationBuilder.CreateIndex(
                name: "IX_DispositionCategories_Code",
                table: "DispositionCategories",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DispositionCategories_IsActive",
                table: "DispositionCategories",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_DispositionCategories_Name",
                table: "DispositionCategories",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_DispositionCodes_CategoryId",
                table: "DispositionCodes",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_DispositionCodes_Code",
                table: "DispositionCodes",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DispositionCodes_HotKey",
                table: "DispositionCodes",
                column: "HotKey");

            migrationBuilder.CreateIndex(
                name: "IX_DispositionCodes_IsActive",
                table: "DispositionCodes",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_DispositionCodes_Name",
                table: "DispositionCodes",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_DncLists_CampaignId",
                table: "DncLists",
                column: "CampaignId");

            migrationBuilder.CreateIndex(
                name: "IX_DncLists_CreatedAt",
                table: "DncLists",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_DncLists_IsActive",
                table: "DncLists",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_DncLists_ListId",
                table: "DncLists",
                column: "ListId");

            migrationBuilder.CreateIndex(
                name: "IX_DncLists_Name",
                table: "DncLists",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_DncLists_Scope",
                table: "DncLists",
                column: "Scope");

            migrationBuilder.CreateIndex(
                name: "IX_DncNumbers_AddedAt",
                table: "DncNumbers",
                column: "AddedAt");

            migrationBuilder.CreateIndex(
                name: "IX_DncNumbers_DncListId",
                table: "DncNumbers",
                column: "DncListId");

            migrationBuilder.CreateIndex(
                name: "IX_DncNumbers_PhoneNumber",
                table: "DncNumbers",
                column: "PhoneNumber");

            migrationBuilder.CreateIndex(
                name: "IX_LeadFilters_CreatedAt",
                table: "LeadFilters",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_LeadFilters_FilterType",
                table: "LeadFilters",
                column: "FilterType");

            migrationBuilder.CreateIndex(
                name: "IX_LeadFilters_IsActive",
                table: "LeadFilters",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_LeadFilters_Name",
                table: "LeadFilters",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_LeadFilters_Priority",
                table: "LeadFilters",
                column: "Priority");

            migrationBuilder.CreateIndex(
                name: "IX_Leads_CreatedAt",
                table: "Leads",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Leads_Disposition",
                table: "Leads",
                column: "Disposition");

            migrationBuilder.CreateIndex(
                name: "IX_Leads_HasOptedOut",
                table: "Leads",
                column: "HasOptedOut");

            migrationBuilder.CreateIndex(
                name: "IX_Leads_IsExcluded",
                table: "Leads",
                column: "IsExcluded");

            migrationBuilder.CreateIndex(
                name: "IX_Leads_LastCalledAt",
                table: "Leads",
                column: "LastCalledAt");

            migrationBuilder.CreateIndex(
                name: "IX_Leads_LifecycleStage",
                table: "Leads",
                column: "LifecycleStage");

            migrationBuilder.CreateIndex(
                name: "IX_Leads_ListId",
                table: "Leads",
                column: "ListId");

            migrationBuilder.CreateIndex(
                name: "IX_Leads_NextCallAt",
                table: "Leads",
                column: "NextCallAt");

            migrationBuilder.CreateIndex(
                name: "IX_Leads_PhoneValidationStatus",
                table: "Leads",
                column: "PhoneValidationStatus");

            migrationBuilder.CreateIndex(
                name: "IX_Leads_PrimaryEmail",
                table: "Leads",
                column: "PrimaryEmail");

            migrationBuilder.CreateIndex(
                name: "IX_Leads_PrimaryPhone",
                table: "Leads",
                column: "PrimaryPhone");

            migrationBuilder.CreateIndex(
                name: "IX_Leads_Priority",
                table: "Leads",
                column: "Priority");

            migrationBuilder.CreateIndex(
                name: "IX_Leads_ScheduledCallbackAt",
                table: "Leads",
                column: "ScheduledCallbackAt");

            migrationBuilder.CreateIndex(
                name: "IX_Leads_Status",
                table: "Leads",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Leads_Tags",
                table: "Leads",
                column: "Tags");

            migrationBuilder.CreateIndex(
                name: "IX_Leads_User",
                table: "Leads",
                column: "User");

            migrationBuilder.CreateIndex(
                name: "IX_Lists_CreatedAt",
                table: "Lists",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Lists_DuplicateCheckMethod",
                table: "Lists",
                column: "DuplicateCheckMethod");

            migrationBuilder.CreateIndex(
                name: "IX_Lists_IsActive",
                table: "Lists",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Lists_Name",
                table: "Lists",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Lists_Priority",
                table: "Lists",
                column: "Priority");

            migrationBuilder.CreateIndex(
                name: "IX_Lists_Source",
                table: "Lists",
                column: "Source");

            migrationBuilder.CreateIndex(
                name: "IX_Lists_Tags",
                table: "Lists",
                column: "Tags");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Agents");

            migrationBuilder.DropTable(
                name: "CallLogs");

            migrationBuilder.DropTable(
                name: "CampaignLists");

            migrationBuilder.DropTable(
                name: "DncNumbers");

            migrationBuilder.DropTable(
                name: "AlternatePhones");

            migrationBuilder.DropTable(
                name: "DispositionCodes");

            migrationBuilder.DropTable(
                name: "DncLists");

            migrationBuilder.DropTable(
                name: "Leads");

            migrationBuilder.DropTable(
                name: "DispositionCategories");

            migrationBuilder.DropTable(
                name: "Campaigns");

            migrationBuilder.DropTable(
                name: "Lists");

            migrationBuilder.DropTable(
                name: "LeadFilters");
        }
    }
}
