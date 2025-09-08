# BAM Syspro Order Automation

A reliable order management system that automates the creation of sales orders in Syspro through multiple integration methods.

## Features

- **API-First Approach**: Reliable Syspro API integration for order creation
- **Fallback Support**: Legacy Selenium web automation as backup
- **Multi-API Support**: Supports various Syspro integration methods:
  - SYSPRO e.net Solutions (Web Services)
  - Business Objects (XML-based API)
  - REST APIs (newer Syspro versions)
- **Toyota Portal Integration**: Automated order import from Toyota supplier portal
- **Progress Tracking**: Comprehensive order creation history and error handling

## Command Line Usage

This project accepts the following command-line arguments:

1. **jobType**: The type of job to run (import orders or create orders)
2. **importType**: The type of import to run (if jobType is import)
3. **apiMode**: Optional parameter to force API usage (set to "api")

### Example Usage

```bash
# Import Toyota orders (manifest or skid)
BA.OrderScraper.exe "toyotaordersimport" "manifest"
BA.OrderScraper.exe "toyotaordersimport" "skid"

# Create Syspro orders using API (recommended)
BA.OrderScraper.exe "sysproordercreate" "" "api"

# Create Syspro orders using legacy Selenium (fallback)
BA.OrderScraper.exe "sysproordercreate"
```

## Configuration

### Syspro API Settings

Add these settings to your `app.config`:

```xml
<!-- Primary Syspro API Configuration -->
<add key="SysproApiBaseUrl" value="http://192.168.1.8:8080/SysproWCFService" />
<add key="SysproApiUsername" value="ADMIN" />
<add key="SysproApiPassword" value="2319" />
<add key="SysproCompanyDatabase" value="1" />
<add key="SysproApiTimeout" value="30" />

<!-- Alternative API endpoints for different integration methods -->
<add key="SysproBusinessObjectsUrl" value="http://192.168.1.8:8080/SysproBusinessObjects" />
<add key="SysproRestApiUrl" value="http://192.168.1.8:8080/SysproRestAPI" />
<add key="SysproWebServiceUrl" value="http://192.168.1.8:8080/SysproWebService.asmx" />
```

### Legacy Selenium Settings

For fallback web automation:

```xml
<add key="SysproAvantiPortalUrl" value="http://192.168.1.8/SYSPROAvanti/"/>
<add key="SysproAvantiPortalUsername" value="ADMIN"/>
<add key="SysproAvantiPortalPassword" value="2319"/>
<add key="SysproAvantiPortalCompany" value="1 - BRACE ABLE"/>
```

## API Integration Details

### Supported Syspro APIs

1. **Business Objects (XML-based)**
   - Most widely supported across Syspro versions
   - XML request/response format
   - Endpoint: `/BusinessObjects/SalesOrder`

2. **Web Services (SOAP/WCF)**
   - Traditional SOAP-based integration
   - Structured service contracts
   - Endpoint: `/SysproWCFService`

3. **REST API (Modern)**
   - JSON-based communication
   - RESTful endpoints
   - Endpoint: `/api/salesorders`

### Order Creation Process

1. **Validation**: Test API connection and validate configuration
2. **Order Creation**: Create sales order header with customer information
3. **Line Items**: Add individual product lines to the order
4. **Progress Tracking**: Monitor and log creation progress
5. **Error Handling**: Comprehensive error logging and retry logic

## Advantages of API Integration

- **Reliability**: No browser dependencies or UI element changes
- **Performance**: Faster execution compared to web automation
- **Maintainability**: Easier to maintain and debug
- **Scalability**: Better handling of multiple concurrent orders
- **Error Handling**: More precise error detection and handling

## Migration from Selenium

The application automatically detects API availability and falls back to Selenium if needed. To force API usage:

```bash
BA.OrderScraper.exe "sysproordercreate" "" "api"
```

## Dependencies

- .NET 8.0
- Entity Framework Core
- Microsoft.Extensions.Http
- System.ServiceModel.Http
- Microsoft.Extensions.Logging.Console

## Database

The application uses Entity Framework to manage order history and tracking in the BAOrders database.