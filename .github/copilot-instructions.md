# ProDialer - Mass Outbound Calling System

<!-- Use this file to provide workspace-specific custom instructions to Copilot. For more details, visit https://code.visualstudio.com/docs/copilot/copilot-customization#_use-a-githubcopilotinstructionsmd-file -->

## Project Overview

This is a comprehensive mass outbound calling system built with Azure services and .NET. The system is designed for high-volume outbound calling campaigns with sophisticated lead management and agent productivity features.

## Architecture

- **Frontend**: Blazor WebAssembly hosted on Azure Static Web Apps
- **Backend API**: Azure Functions with .NET 8 (isolated worker)
- **Communication**: Azure Communication Services for calling functionality
- **Data Storage**: 
  - Azure SQL Database for relational data (campaigns, lists, leads, call logs)
  - Azure Table Storage for non-relational data (real-time agent status, session data)
- **Infrastructure**: Bicep templates for Infrastructure as Code
- **Monitoring**: Application Insights and Log Analytics

## Key Business Concepts

### Campaigns
Collections of lists and settings that define how calls should be made:
- **Dialing Ratio**: Number of outbound calls per agent allowed
- **Strategic Settings**: Time restrictions, location-based rules, ratio application (all agents vs idle only)
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
- **ProDialer.Models**: Legacy model project (being phased out in favor of Shared)

### Azure Integration
- Use managed identity for authentication between services
- Implement proper error handling and retry logic for Azure service calls
- Follow Azure Communication Services best practices for call management
- Use Application Insights for telemetry and monitoring

### Data Management
- Entity Framework Core for SQL database operations
- Azure.Data.Tables for Table Storage operations
- Implement proper connection string management and configuration
- Use appropriate caching strategies for frequently accessed data

### Security Considerations
- Never expose sensitive data (connection strings, keys) in client-side code
- Implement proper authorization for API endpoints
- Use HTTPS everywhere and validate all inputs
- Follow GDPR/privacy requirements for lead data handling

### Performance
- Implement efficient paging for large lead lists
- Use background services for call processing and status updates
- Consider read replicas for reporting and analytics
- Optimize database queries and use proper indexing

### Testing
- Write unit tests for business logic
- Integration tests for Azure service interactions
- Load testing for high-volume calling scenarios
- Monitor performance metrics and set up alerts

## Common Patterns

### API Responses
Use consistent response patterns with proper HTTP status codes and error handling.

### Configuration Management
Use Azure App Configuration or environment variables for settings that may change between environments.

### Logging
Use structured logging with Application Insights for better searchability and monitoring.

### Error Handling
Implement comprehensive error handling with user-friendly messages and proper error codes.

When working on this project, always consider the high-volume, multi-tenant nature of the calling system and ensure that solutions scale appropriately.
