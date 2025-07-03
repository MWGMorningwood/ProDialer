# ProDialer Dialing Dashboard Test Plan

## Overview
This document outlines the testing strategy for the ProDialer Dialing Dashboard, a comprehensive real-time campaign monitoring and control interface that provides VICIdial-style functionality for managing outbound calling operations.

## Test Environment Setup

### Prerequisites
1. **Azure Services Running**:
   - Azure SQL Database with complete schema deployed
   - Azure Functions backend API running
   - Azure Communication Services configured
   - Azure Static Web Apps hosting the Blazor frontend

2. **Test Data Requirements**:
   - At least 2-3 campaigns (active and inactive)
   - Several leads across different lists
   - Agent records with various statuses
   - Sample call logs for testing active calls view

3. **Browser Requirements**:
   - Modern web browser (Chrome, Firefox, Edge)
   - JavaScript enabled
   - Network access to Azure services

## Test Scenarios

### 1. Dashboard Load and Real-time Statistics
**Objective**: Verify dashboard loads correctly and displays accurate real-time statistics

**Test Steps**:
1. Navigate to `/dialing` route in the web application
2. Verify the dashboard loads without errors
3. Check that all statistics cards display data:
   - Active Campaigns count
   - Available Agents count
   - Calls Today count
   - Calls In Progress count
   - Answer Rate percentage
   - Ready Leads count
4. Wait 5+ seconds and verify statistics auto-refresh

**Expected Results**:
- Dashboard loads cleanly with professional UI
- All statistics cards show numeric values (may be 0 if no data)
- Auto-refresh updates statistics every 5 seconds
- No console errors or network failures

### 2. Campaign Control Tab
**Objective**: Test campaign management functionality

**Test Steps**:
1. Click "Campaign Control" tab
2. Verify campaigns list displays with correct information:
   - Campaign name and description
   - Active/Inactive status badges
   - Dial method
   - Action buttons appropriate to status
3. Test campaign control actions:
   - Start an inactive campaign
   - Pause an active campaign
   - Stop an active campaign
   - Process a campaign
4. Test global campaign controls:
   - "Start All Campaigns" button
   - "Pause All" button
   - "Stop All" button

**Expected Results**:
- Campaigns display in clean table format
- Status badges show correct colors (green for active, gray for inactive)
- Individual campaign controls work correctly
- Global controls affect all campaigns appropriately
- UI shows loading state during operations
- Success/error messages appear as needed

### 3. Agent Status Tab
**Objective**: Verify agent monitoring functionality

**Test Steps**:
1. Click "Agent Status" tab
2. Verify agents list displays with:
   - Agent full name and ID
   - Current status (Available, On Call, Break, Offline)
   - Current call indicator if on call
   - Campaign assignment
   - Action buttons based on status
3. Test agent actions:
   - Assign available agent to campaign
   - Hangup call for agent on call

**Expected Results**:
- Agents display with accurate status information
- Status badges use appropriate colors
- Actions are available based on agent status
- Operations complete successfully

### 4. Active Calls Tab
**Objective**: Test active call monitoring and control

**Test Steps**:
1. Click "Active Calls" tab
2. If no active calls, note empty state message
3. If active calls exist, verify display shows:
   - Call ID
   - Agent information
   - Phone number being called
   - Call status
   - Call duration
   - Campaign information
4. Test call control actions:
   - Hangup active call
   - Transfer call (if available)

**Expected Results**:
- Active calls display in real-time
- Call duration updates continuously
- Call controls work correctly
- Empty state shows helpful message when no active calls

### 5. Manual Dial Tab
**Objective**: Test manual dialing functionality

**Test Steps**:
1. Click "Manual Dial" tab
2. Test manual call form:
   - Enter phone number
   - Select available agent
   - Select campaign (optional)
   - Submit call request
3. Test lead search and call:
   - Enter search term in lead search
   - Click search button
   - Verify leads appear in results
   - Click "Call" button on a lead

**Expected Results**:
- Manual call form accepts valid input
- Agent dropdown shows only available agents
- Campaign dropdown shows only active campaigns
- Lead search returns relevant results
- Call initiation works from both forms
- Form validation prevents invalid submissions

### 6. Error Handling and Edge Cases
**Objective**: Test system behavior under error conditions

**Test Steps**:
1. Test with no internet connection
2. Test with backend API unavailable
3. Test with invalid data submissions
4. Test concurrent operations
5. Test browser refresh during operations

**Expected Results**:
- Graceful error handling with user-friendly messages
- No application crashes or unhandled exceptions
- Operations can be retried after connectivity restore
- State is preserved appropriately across refreshes

### 7. Performance and Responsiveness
**Objective**: Verify dashboard performance and UI responsiveness

**Test Steps**:
1. Test dashboard on different screen sizes
2. Monitor network requests and response times
3. Test with multiple browser tabs open
4. Test auto-refresh performance over extended periods

**Expected Results**:
- Dashboard is responsive on mobile and desktop
- API calls complete within reasonable time (< 2 seconds)
- Auto-refresh doesn't cause performance degradation
- UI remains responsive during background operations

## Integration Test Scenarios

### 1. End-to-End Call Flow
**Objective**: Test complete call lifecycle through dashboard

**Test Steps**:
1. Start a campaign via dashboard
2. Monitor active calls tab for new calls
3. Watch call progress through different states
4. Verify call completion and disposition
5. Check that statistics update correctly

### 2. Multi-Agent Scenario
**Objective**: Test dashboard with multiple agents

**Test Steps**:
1. Have multiple agents with different statuses
2. Assign agents to different campaigns
3. Monitor agent status changes
4. Test concurrent call handling

### 3. Campaign Lifecycle Management
**Objective**: Test complete campaign management through dashboard

**Test Steps**:
1. Create campaign through regular UI
2. Start campaign via dashboard
3. Monitor campaign progress
4. Pause/resume as needed
5. Stop campaign and verify cleanup

## Validation Checklist

### UI/UX Validation
- [ ] Dashboard loads quickly (< 3 seconds)
- [ ] All tabs are accessible and functional
- [ ] Statistics cards display meaningful data
- [ ] Tables are sortable and filterable where applicable
- [ ] Action buttons are clearly labeled and appropriately enabled/disabled
- [ ] Loading states provide visual feedback
- [ ] Error messages are helpful and actionable
- [ ] Mobile responsiveness works correctly

### Functional Validation
- [ ] Real-time updates work consistently
- [ ] Campaign controls affect database state correctly
- [ ] Agent status reflects actual system state
- [ ] Active calls display matches communication service state
- [ ] Manual dialing initiates calls successfully
- [ ] Lead search returns accurate results
- [ ] All API endpoints respond correctly
- [ ] Permission/authorization works appropriately

### Data Validation
- [ ] Statistics calculations are accurate
- [ ] Call durations are calculated correctly
- [ ] Agent assignments are reflected properly
- [ ] Campaign statuses are synchronized
- [ ] Lead data is displayed accurately
- [ ] Call logs are updated in real-time

## Test Data Setup

### Sample Campaigns
```sql
-- Create test campaigns with different statuses
INSERT INTO Campaigns (Name, Description, DialMethod, IsActive, DialRatio, CallStartTime, CallEndTime)
VALUES 
('Test Campaign 1', 'Active predictive campaign', 'PREDICTIVE', 1, 2.5, '09:00:00', '17:00:00'),
('Test Campaign 2', 'Inactive preview campaign', 'PREVIEW', 0, 1.0, '10:00:00', '16:00:00'),
('Test Campaign 3', 'Manual dial campaign', 'MANUAL', 1, 1.0, '08:00:00', '18:00:00');
```

### Sample Agents
```sql
-- Create test agents with different statuses
INSERT INTO Agents (UserId, FirstName, LastName, FullName, Email, Status, IsLoggedIn, IsActive)
VALUES 
('agent1', 'John', 'Doe', 'John Doe', 'john.doe@company.com', 'AVAILABLE', 1, 1),
('agent2', 'Jane', 'Smith', 'Jane Smith', 'jane.smith@company.com', 'ON_CALL', 1, 1),
('agent3', 'Bob', 'Johnson', 'Bob Johnson', 'bob.johnson@company.com', 'BREAK', 1, 1);
```

### Sample Leads
```sql
-- Create test leads for searching and calling
INSERT INTO Leads (ListId, FirstName, LastName, FullName, PrimaryPhone, CallCount, LastCallDate, Status)
VALUES 
(1, 'Alice', 'Brown', 'Alice Brown', '555-123-4567', 0, NULL, 'New'),
(1, 'Charlie', 'Wilson', 'Charlie Wilson', '555-234-5678', 1, GETDATE()-1, 'Callback'),
(1, 'Diana', 'Davis', 'Diana Davis', '555-345-6789', 0, NULL, 'New');
```

## Success Criteria

The dialing dashboard test is considered successful when:

1. **All major functionality works**: Campaign control, agent monitoring, call management, manual dialing
2. **Real-time updates function**: 5-second auto-refresh works consistently
3. **Error handling is robust**: Graceful handling of network issues and invalid input
4. **Performance is acceptable**: Dashboard loads quickly and remains responsive
5. **UI is professional**: Clean, modern interface suitable for production use
6. **Integration is seamless**: Dashboard reflects actual system state accurately

## Risk Mitigation

### Potential Issues and Mitigations
1. **Azure Communication Services Integration**: If ACS is not fully configured, mock the communication service for UI testing
2. **Database Connectivity**: Ensure connection strings are correct and database is accessible
3. **Real-time Updates**: If auto-refresh causes performance issues, consider increasing interval or implementing more efficient polling
4. **Browser Compatibility**: Test on multiple browsers and have fallbacks for older browsers

## Next Steps After Testing

1. **Performance Optimization**: Based on test results, optimize API calls and UI rendering
2. **Enhanced Features**: Add features like call recording controls, advanced filtering, etc.
3. **Production Deployment**: Deploy to Azure Static Web Apps for live testing
4. **User Training**: Create documentation and training materials for end users
5. **Monitoring Setup**: Implement application insights and monitoring for production use

This comprehensive test plan ensures the ProDialer Dialing Dashboard meets all requirements for a production-ready mass outbound calling system with VICIdial feature parity.
