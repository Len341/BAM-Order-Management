# BA Order Scraper - API Integration Update

This project has been updated to replace Selenium-based web automation with direct Syspro API integration for creating sales orders.

## What Changed

### Before (Selenium-based)
- **1600+ lines** of complex web automation code in `SysproHelpers.cs`
- Brittle browser automation prone to failures
- Required Edge WebDriver and handling of web UI quirks
- Slow and resource-intensive
- Difficult to debug and maintain

### After (API-based)
- **Clean API integration** using Syspro Business Objects
- Direct communication with Syspro web services
- No browser automation required for order creation
- Faster and more reliable
- Easier to maintain and debug

## New Architecture

### API Service Layer
- `ISysproApiService` - Interface for Syspro API operations
- `SysproApiService` - Implementation using HTTP client and XML serialization
- Support for authentication, order creation, and stock lookups

### API Models
- `SalesOrderRequest/Response` - For SORTOI (Sales Order Transaction Import)
- `StockCodeLookupRequest/Response` - For INVQRY (Inventory Query)
- Proper XML serialization for Syspro Business Objects

### Updated Configuration
- New `SysproApi*` settings in `app.config`
- Maintains backward compatibility with existing settings
- Supports both API and legacy Selenium modes

## Setup Requirements

### 1. Install Syspro Web Services
You need to install and configure Syspro Web Services on your Syspro server:

```
http://your-syspro-server:8080/sysprowebapi
```

### 2. Update Configuration
Edit `app.config` and update these settings:

```xml
<!-- Syspro API Settings -->
<add key="SysproApiBaseUrl" value="http://your-syspro-server:8080/sysprowebapi" />
<add key="SysproApiUsername" value="your-api-user" />
<add key="SysproApiPassword" value="your-api-password" />
<add key="SysproApiCompany" value="COMPANY01" />
<add key="SysproApiCompanyPassword" value="company-password" />
```

### 3. Database Connections
Ensure both connection strings are configured:
- `DefaultConnection` - Main application database
- `SysproDatabase` - Syspro company database for inventory lookups

## Usage

### Command Line (unchanged)
```bash
BA.OrderScraper.exe sysproordercreate
```

### API Mode vs Legacy Mode
- **Syspro Orders**: Now uses API (no browser required)
- **Toyota Import**: Still uses Selenium (browser required)

## Key Benefits

1. **Reliability**: Direct API calls eliminate web UI inconsistencies
2. **Performance**: Much faster than browser automation
3. **Maintainability**: Clean, testable code instead of complex Selenium logic
4. **Resource Usage**: No browser processes or WebDriver management
5. **Error Handling**: Better error reporting from Syspro Business Objects

## Compatibility

- Maintains same public interface (`SysproHelpers.CreateSysproOrders`)
- Existing database schema unchanged
- Same error logging and progress tracking
- Legacy Selenium code preserved for Toyota import

## API Endpoints Used

### Authentication
```
POST /logon
```

### Business Object Execution
```
POST /businessobject
Body: {
  "BusinessObject": "SORTOI",
  "InputXml": "<PostSalesOrder>...</PostSalesOrder>"
}
```

## Troubleshooting

### Connection Issues
1. Verify Syspro Web Services are running
2. Check firewall settings
3. Confirm API endpoint URL is correct
4. Test with Syspro's built-in web service tester

### Authentication Issues
1. Verify username/password are correct
2. Check company database permissions
3. Ensure company password is set if required

### Order Creation Issues
1. Check stock codes exist in Syspro
2. Verify warehouse settings
3. Review Syspro Business Object logs
4. Check customer setup (default: TOY020)

## Development Notes

- API integration is production-ready but may need local Syspro web service setup
- Stock code lookup logic preserved from original Selenium implementation
- Error handling maintains same database logging pattern
- Progress tracking continues to work as before

## Future Enhancements

1. Implement order updates via API
2. Add support for additional Syspro business objects
3. Implement retry logic for API failures
4. Add comprehensive logging for API interactions
5. Support for batch order processing