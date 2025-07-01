using Azure.Data.Tables;
using Azure.Identity;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ProDialer.Functions.Data;
using ProDialer.Functions.Services;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

// Configure Entity Framework with SQL Database
builder.Services.AddDbContext<ProDialerDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    if (string.IsNullOrEmpty(connectionString))
    {
        throw new InvalidOperationException("Database connection string 'DefaultConnection' is not configured.");
    }
    
    options.UseSqlServer(connectionString, sqlOptions =>
    {
        sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorNumbersToAdd: null);
    });
});

// Configure Azure Table Storage with Managed Identity
builder.Services.AddSingleton<TableServiceClient>(serviceProvider =>
{
    var configuration = serviceProvider.GetRequiredService<IConfiguration>();
    var storageAccountName = configuration["Azure:StorageAccountName"];
    
    if (string.IsNullOrEmpty(storageAccountName))
    {
        // Fallback to connection string for local development
        var connectionString = configuration.GetConnectionString("Storage");
        if (!string.IsNullOrEmpty(connectionString))
        {
            return new TableServiceClient(connectionString);
        }
        
        throw new InvalidOperationException("Either Azure:StorageAccountName or Storage connection string must be configured.");
    }
    
    // Use managed identity for Azure deployment
    var serviceUri = new Uri($"https://{storageAccountName}.table.core.windows.net");
    return new TableServiceClient(serviceUri, new DefaultAzureCredential());
});

// Register custom services
builder.Services.AddScoped<CommunicationService>();
builder.Services.AddScoped<TableStorageService>();

// Configure Communication Service options
builder.Services.Configure<CommunicationServiceOptions>(
    builder.Configuration.GetSection(CommunicationServiceOptions.SectionName));

builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();

var app = builder.Build();

// Initialize database and tables on startup
using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    
    try
    {
        // Initialize database
        var dbContext = scope.ServiceProvider.GetRequiredService<ProDialerDbContext>();
        await dbContext.Database.EnsureCreatedAsync();
        logger.LogInformation("Database initialized successfully");

        // Initialize table storage
        var tableService = scope.ServiceProvider.GetRequiredService<TableStorageService>();
        await tableService.InitializeTablesAsync();
        logger.LogInformation("Table storage initialized successfully");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Failed to initialize database or table storage");
        throw;
    }
}

app.Run();
