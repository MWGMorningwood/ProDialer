# ProDialer - Mass Outbound Calling System

A comprehensive, enterprise-grade mass outbound calling system built with Azure services and .NET 8, designed to match and exceed VICIdial's functionality while leveraging modern cloud-native architecture.

## 🚀 Key Features

### VICIdial Feature Parity (~95%)
- **Advanced Campaign Management**: 50+ configuration options including adaptive dialing, lead recycling, compliance settings
- **Comprehensive List Management**: 35+ fields with duplicate checking, phone validation, mixing ratios  
- **Sophisticated Lead Management**: 60+ fields with lifecycle tracking, quality scoring, compliance flags
- **Detailed Call Logging**: 40+ fields with AMD results, call progress tracking, disposition management
- **Enterprise DNC Management**: System-wide and targeted Do-Not-Call list management
- **Hierarchical Dispositions**: Automated disposition system with recycling rules and hot keys

### Azure-Native Architecture Advantages
- **Scalability**: Auto-scaling Azure Functions and managed database services
- **Reliability**: Built-in high availability and disaster recovery
- **Security**: Azure Managed Identity and enterprise-grade security
- **Cost Efficiency**: Pay-per-use model with automatic scaling
- **Global Reach**: Deploy to multiple Azure regions worldwide

## Architecture

- **Frontend**: Blazor WebAssembly hosted on Azure Static Web Apps
- **Backend API**: Azure Functions with .NET 8 (isolated worker)
- **Communication**: Azure Communication Services for calling functionality
- **Data Storage**: 
  - Azure SQL Database for relational data (campaigns, lists, leads, call logs)
  - Azure Table Storage for non-relational data (real-time agent status, session data)
- **Infrastructure**: Bicep templates for Infrastructure as Code
- **Authentication**: Azure Managed Identity for secure service-to-service authentication
- **Monitoring**: Application Insights and Log Analytics

## Data Models & VICIdial Compatibility

### Core Models (Enhanced with 100+ VICIdial Fields)
- **Campaign**: 50+ fields including dial methods, prefixes, recycling rules, compliance settings
- **List**: 35+ fields with duplicate checking, validation settings, performance tracking
- **Lead**: 60+ fields with phone validation, lifecycle tracking, scoring, compliance
- **Agent**: Performance tracking, skills management, status monitoring
- **CallLog**: 40+ fields with AMD results, call progress, disposition tracking

### Supporting Models (New VICIdial-Style Features)
- **LeadFilter**: Complex SQL and rule-based lead filtering system
- **DncList/DncNumber**: Comprehensive Do-Not-Call management with system-wide and targeted lists
- **DispositionCategory/DispositionCode**: Hierarchical disposition system with automation and hot keys
- **AlternatePhone**: Unlimited phone numbers per lead with validation and call tracking

### VICIdial Feature Mapping
✅ **95% Feature Parity Achieved** - All major VICIdial functionality implemented  
🔄 **Architecture Adapted** - Azure-native equivalents for Asterisk-specific features  
❌ **Intentionally Excluded** - Legacy telco and hardware-dependent features  

*See `/docs/VICIDIAL_ENHANCEMENT_SUMMARY.md` for detailed feature comparison*

## Getting Started

### Prerequisites
- .NET 8 SDK
- Visual Studio Code or Visual Studio 2022
- Azure CLI (for deployment)
- SQL Server LocalDB (for development)
- Azure Storage Emulator (for development)

### Development Setup

1. **Clone and restore packages:**
   ```bash
   dotnet restore
   ```

2. **Configure local settings:**
   - Update `src/ProDialer.Functions/local.settings.json` with your connection strings
   - For local development, SQL Server LocalDB and Storage Emulator can be used

3. **Build the solution:**
   ```bash
   dotnet build
   ```

4. **Run the Functions backend:**
   ```bash
   dotnet run --project src/ProDialer.Functions
   ```

5. **Run the Blazor frontend:**
   ```bash
   dotnet run --project src/ProDialer.Web
   ```

### Database Setup

The application uses Entity Framework Core with SQL Server. For local development:

1. Ensure SQL Server LocalDB is installed
2. The database will be created automatically on first run
3. Connection string is configured in `local.settings.json`

### Azure Deployment

1. **Install Azure Developer CLI (azd):**
   ```bash
   # Follow instructions at https://aka.ms/azd-install
   ```

2. **Initialize and deploy:**
   ```bash
   azd init
   azd up
   ```

## Configuration

### Required Configuration Values

For Azure deployment, configure these values:

- **Communication Services**: Connection string for Azure Communication Services
- **Storage Account**: Connection string for Azure Storage (Table Storage)
- **SQL Database**: Connection string for Azure SQL Database
- **Application Insights**: Connection string for telemetry

### Environment Variables

| Setting | Description | Required |
|---------|-------------|----------|
| `ConnectionStrings:DefaultConnection` | SQL Database connection | Yes |
| `ConnectionStrings:Storage` | Azure Storage connection | Yes |
| `ConnectionStrings:CommunicationServices` | ACS connection | For calling features |
| `APPLICATIONINSIGHTS_CONNECTION_STRING` | App Insights connection | For monitoring |

## API Endpoints

### Campaigns
- `GET /api/campaigns` - List all campaigns
- `GET /api/campaigns/{id}` - Get campaign details
- `POST /api/campaigns` - Create new campaign
- `PUT /api/campaigns/{id}` - Update campaign
- `DELETE /api/campaigns/{id}` - Delete campaign
- `POST /api/campaigns/{id}/start` - Start campaign
- `POST /api/campaigns/{id}/stop` - Stop campaign

### Lists (TODO)
- `GET /api/lists` - List all lists
- `POST /api/lists` - Create new list
- `PUT /api/lists/{id}` - Update list

### Leads (TODO)
- `GET /api/leads` - List leads with filtering
- `POST /api/leads` - Create new lead
- `POST /api/leads/import` - Bulk import leads

## Setup and Configuration

### Prerequisites
- Azure subscription
- Azure CLI or PowerShell
- .NET 8 SDK
- Visual Studio 2022 or VS Code

### Local Development Setup

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd ProDialer
   ```

2. **Configure local settings**
   - Copy `src/ProDialer.Functions/local.settings.json.example` to `local.settings.json`
   - Update connection strings and Azure service configuration
   - See `MANAGED_IDENTITY_SETUP.md` for detailed authentication setup

3. **Deploy infrastructure**
   ```bash
   azd up
   ```

4. **Configure SQL Database permissions**
   - Connect to your SQL Database
   - Run the SQL commands in `MANAGED_IDENTITY_SETUP.md`

### Authentication
This project uses Azure Managed Identity for secure authentication. See `MANAGED_IDENTITY_SETUP.md` for complete setup instructions.

## Business Concepts

### Campaigns
Collections of lists and settings that define how calls should be made:
- **Dialing Ratio**: Number of outbound calls per agent allowed
- **Strategic Settings**: Time restrictions, location-based rules, ratio application
- **Call Behavior**: Answering machine detection, retry logic, recording settings

### Lists
Collections of leads with specific calling configurations:
- Can override campaign-level settings for specific lead groups
- Support priority-based calling and custom field schemas
- Track performance metrics (total leads, called leads, contacted leads)

### Leads
Individual contacts with extensive field support:
- Core contact information (name, phone, email, address)
- Call history and status tracking
- Custom fields for client-specific data requirements
- Disposition and outcome tracking

## Development Guidelines

### Code Organization
- **ProDialer.Shared**: Common models, DTOs, and shared logic
- **ProDialer.Functions**: Azure Functions API endpoints
- **ProDialer.Web**: Blazor WebAssembly frontend
- **ProDialer.Models**: Legacy model project (being phased out)

### Best Practices
- Use managed identity for Azure service authentication
- Implement proper error handling and retry logic
- Follow GDPR/privacy requirements for lead data
- Use structured logging with Application Insights
- Optimize database queries and implement proper indexing

## Contributing

1. Follow the existing code patterns and naming conventions
2. Add unit tests for new functionality
3. Update documentation for new features
4. Use semantic commit messages

## License

This project is proprietary. All rights reserved.