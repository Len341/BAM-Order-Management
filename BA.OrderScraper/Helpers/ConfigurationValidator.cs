using System;
using System.Configuration;
using System.Net.NetworkInformation;
using System.Threading.Tasks;

namespace BA.OrderScraper.Helpers
{
    /// <summary>
    /// Helper class to validate Syspro API configuration and connectivity
    /// </summary>
    public static class ConfigurationValidator
    {
        public static async Task<bool> ValidateAllConfigurationAsync()
        {
            Console.WriteLine("=== Configuration Validation ===");
            
            bool allValid = true;
            
            // 1. Check required configuration keys
            allValid &= ValidateConfigurationKeys();
            
            // 2. Check network connectivity
            allValid &= await ValidateNetworkConnectivityAsync();
            
            // 3. Check database connectivity
            allValid &= await ValidateDatabaseConnectivityAsync();
            
            Console.WriteLine($"\nOverall Configuration Status: {(allValid ? "✓ VALID" : "✗ INVALID")}");
            
            return allValid;
        }
        
        private static bool ValidateConfigurationKeys()
        {
            Console.WriteLine("\n1. Validating Configuration Keys...");
            
            bool valid = true;
            
            // Required Syspro API settings
            var requiredKeys = new[]
            {
                "SysproApiBaseUrl",
                "SysproApiUsername", 
                "SysproApiPassword",
                "SysproCompanyDatabase"
            };
            
            foreach (var key in requiredKeys)
            {
                var value = ConfigurationManager.AppSettings[key];
                if (string.IsNullOrEmpty(value))
                {
                    Console.WriteLine($"   ✗ {key}: MISSING");
                    valid = false;
                }
                else
                {
                    // Don't log passwords in full
                    var displayValue = key.Contains("Password") ? "***" : value;
                    Console.WriteLine($"   ✓ {key}: {displayValue}");
                }
            }
            
            // Optional/Legacy settings
            var legacyKeys = new[]
            {
                "SysproAvantiPortalUrl",
                "SysproAvantiPortalUsername",
                "EdgeDriverPath"
            };
            
            Console.WriteLine("\n   Legacy/Fallback Settings:");
            foreach (var key in legacyKeys)
            {
                var value = ConfigurationManager.AppSettings[key];
                var status = string.IsNullOrEmpty(value) ? "NOT SET" : "SET";
                Console.WriteLine($"   - {key}: {status}");
            }
            
            return valid;
        }
        
        private static async Task<bool> ValidateNetworkConnectivityAsync()
        {
            Console.WriteLine("\n2. Validating Network Connectivity...");
            
            var baseUrl = ConfigurationManager.AppSettings["SysproApiBaseUrl"];
            if (string.IsNullOrEmpty(baseUrl))
            {
                Console.WriteLine("   ✗ Cannot test connectivity - SysproApiBaseUrl not configured");
                return false;
            }
            
            try
            {
                // Extract hostname from URL
                var uri = new Uri(baseUrl);
                var hostname = uri.Host;
                var port = uri.Port;
                
                Console.WriteLine($"   Testing connection to: {hostname}:{port}");
                
                // Simple ping test
                var ping = new Ping();
                var reply = await ping.SendPingAsync(hostname, 5000);
                
                if (reply.Status == IPStatus.Success)
                {
                    Console.WriteLine($"   ✓ Ping successful ({reply.RoundtripTime}ms)");
                    return true;
                }
                else
                {
                    Console.WriteLine($"   ✗ Ping failed: {reply.Status}");
                    Console.WriteLine("   Note: Ping failure doesn't necessarily mean API won't work");
                    Console.WriteLine("   Server might be configured to not respond to ping");
                    return true; // Don't fail validation just on ping
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"   ⚠ Network test failed: {ex.Message}");
                Console.WriteLine("   This might indicate connectivity issues");
                return false;
            }
        }
        
        private static async Task<bool> ValidateDatabaseConnectivityAsync()
        {
            Console.WriteLine("\n3. Validating Database Connectivity...");
            
            try
            {
                // Test database connectivity using existing service
                var sysproAppService = new BA.OrderScraper.Services.SysproAppService();
                
                // Try to get a simple record to test connectivity
                var invMasterList = await sysproAppService.GetInvMasterListAsync();
                
                if (invMasterList != null)
                {
                    Console.WriteLine($"   ✓ Syspro database connection successful");
                    Console.WriteLine($"   ✓ Found {invMasterList.Count} inventory records");
                    return true;
                }
                else
                {
                    Console.WriteLine("   ✗ Database connection returned null");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"   ✗ Database connection failed: {ex.Message}");
                Console.WriteLine("   Check database server and connection string");
                return false;
            }
        }
        
        /// <summary>
        /// Generate a sample configuration for documentation
        /// </summary>
        public static void DisplaySampleConfiguration()
        {
            Console.WriteLine("=== Sample Configuration ===");
            Console.WriteLine("Add these settings to your app.config:");
            Console.WriteLine();
            Console.WriteLine("<!-- Syspro API Configuration -->");
            Console.WriteLine("<add key=\"SysproApiBaseUrl\" value=\"http://your-syspro-server:8080/SysproWCFService\" />");
            Console.WriteLine("<add key=\"SysproApiUsername\" value=\"your-username\" />");
            Console.WriteLine("<add key=\"SysproApiPassword\" value=\"your-password\" />");
            Console.WriteLine("<add key=\"SysproCompanyDatabase\" value=\"1\" />");
            Console.WriteLine("<add key=\"SysproApiTimeout\" value=\"30\" />");
            Console.WriteLine();
            Console.WriteLine("<!-- Alternative API Endpoints -->");
            Console.WriteLine("<add key=\"SysproBusinessObjectsUrl\" value=\"http://your-syspro-server:8080/SysproBusinessObjects\" />");
            Console.WriteLine("<add key=\"SysproRestApiUrl\" value=\"http://your-syspro-server:8080/SysproRestAPI\" />");
            Console.WriteLine("<add key=\"SysproWebServiceUrl\" value=\"http://your-syspro-server:8080/SysproWebService.asmx\" />");
        }
        
        /// <summary>
        /// Quick configuration check without network tests
        /// </summary>
        public static bool QuickConfigurationCheck()
        {
            var baseUrl = ConfigurationManager.AppSettings["SysproApiBaseUrl"];
            var username = ConfigurationManager.AppSettings["SysproApiUsername"];
            var password = ConfigurationManager.AppSettings["SysproApiPassword"];
            
            return !string.IsNullOrEmpty(baseUrl) && 
                   !string.IsNullOrEmpty(username) && 
                   !string.IsNullOrEmpty(password);
        }
    }
}