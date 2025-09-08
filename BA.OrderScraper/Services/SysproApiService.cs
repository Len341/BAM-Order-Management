using BA.OrderScraper.Models.DTO;
using BA.OrderScraper.Models.Syspro.Api;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace BA.OrderScraper.Services
{
    /// <summary>
    /// Implementation of Syspro API service
    /// Replaces Selenium WebDriver automation with direct API calls
    /// </summary>
    public class SysproApiService : ISysproApiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;
        private readonly string _username;
        private readonly string _password;
        private readonly string _company;
        private readonly string _companyPassword;
        private readonly SysproAppService _sysproAppService;
        private string _sessionToken;

        public SysproApiService()
        {
            _httpClient = new HttpClient();
            _baseUrl = ConfigurationManager.AppSettings["SysproApiBaseUrl"] ?? "http://localhost:8080";
            _username = ConfigurationManager.AppSettings["SysproApiUsername"] ?? ConfigurationManager.AppSettings["SysproAvantiPortalUsername"];
            _password = ConfigurationManager.AppSettings["SysproApiPassword"] ?? ConfigurationManager.AppSettings["SysproAvantiPortalPassword"];
            _company = ConfigurationManager.AppSettings["SysproApiCompany"] ?? ConfigurationManager.AppSettings["SysproAvantiPortalCompany"];
            _companyPassword = ConfigurationManager.AppSettings["SysproApiCompanyPassword"] ?? ConfigurationManager.AppSettings["SysproAvantiPortalCompanyPassword"];
            _sysproAppService = new SysproAppService();

            // Set default headers for API calls
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "BA-OrderScraper-API/1.0");
            _httpClient.Timeout = TimeSpan.FromMinutes(5);
        }

        public async Task<bool> TestConnectionAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/health");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Connection test failed: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> AuthenticateAsync()
        {
            try
            {
                var loginRequest = new
                {
                    UserId = _username,
                    Password = _password,
                    Company = _company,
                    CompanyPassword = _companyPassword
                };

                var json = Newtonsoft.Json.JsonConvert.SerializeObject(loginRequest);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"{_baseUrl}/logon", content);
                
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var loginResponse = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(responseContent);
                    _sessionToken = loginResponse?.Guid;
                    
                    // Add session token to default headers
                    if (!string.IsNullOrEmpty(_sessionToken))
                    {
                        _httpClient.DefaultRequestHeaders.Remove("Guid");
                        _httpClient.DefaultRequestHeaders.Add("Guid", _sessionToken);
                        return true;
                    }
                }

                Console.WriteLine($"Authentication failed: {response.StatusCode} - {response.ReasonPhrase}");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Authentication error: {ex.Message}");
                return false;
            }
        }

        public async Task<SalesOrderResponse> CreateSalesOrderAsync(SysproOrderItem orderItem)
        {
            try
            {
                // Ensure we're authenticated
                if (string.IsNullOrEmpty(_sessionToken))
                {
                    if (!await AuthenticateAsync())
                    {
                        throw new InvalidOperationException("Failed to authenticate with Syspro API");
                    }
                }

                // Build the sales order request
                var salesOrderRequest = await BuildSalesOrderRequest(orderItem);
                
                // Serialize to XML
                var requestXml = SerializeToXml(salesOrderRequest);
                
                // Create business object request
                var businessObjectRequest = new
                {
                    BusinessObject = "SORTOI",
                    InputXml = requestXml
                };

                var json = Newtonsoft.Json.JsonConvert.SerializeObject(businessObjectRequest);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"{_baseUrl}/businessobject", content);
                
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var businessObjectResponse = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(responseContent);
                    
                    // Extract the XML response
                    string outputXml = businessObjectResponse?.OutputXml;
                    if (!string.IsNullOrEmpty(outputXml))
                    {
                        return DeserializeFromXml<SalesOrderResponse>(outputXml);
                    }
                }

                throw new Exception($"API call failed: {response.StatusCode} - {response.ReasonPhrase}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating sales order: {ex.Message}");
                
                // Return error response
                return new SalesOrderResponse
                {
                    ErrorNumbers = new ErrorNumbers
                    {
                        ErrorNumber = new List<ErrorNumber>
                        {
                            new ErrorNumber
                            {
                                Number = "API001",
                                Description = $"API Error: {ex.Message}"
                            }
                        }
                    }
                };
            }
        }

        public async Task<StockCodeLookupResponse> LookupStockCodeAsync(string alternateKey)
        {
            try
            {
                // Ensure we're authenticated
                if (string.IsNullOrEmpty(_sessionToken))
                {
                    if (!await AuthenticateAsync())
                    {
                        throw new InvalidOperationException("Failed to authenticate with Syspro API");
                    }
                }

                var lookupRequest = new StockCodeLookupRequest
                {
                    Key = new StockCodeKey
                    {
                        AlternateKey1 = alternateKey
                    }
                };

                var requestXml = SerializeToXml(lookupRequest);

                var businessObjectRequest = new
                {
                    BusinessObject = "INVQRY",
                    InputXml = requestXml
                };

                var json = Newtonsoft.Json.JsonConvert.SerializeObject(businessObjectRequest);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"{_baseUrl}/businessobject", content);
                
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var businessObjectResponse = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(responseContent);
                    
                    string outputXml = businessObjectResponse?.OutputXml;
                    if (!string.IsNullOrEmpty(outputXml))
                    {
                        return DeserializeFromXml<StockCodeLookupResponse>(outputXml);
                    }
                }

                return new StockCodeLookupResponse(); // Empty response if not found
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error looking up stock code: {ex.Message}");
                return new StockCodeLookupResponse(); // Empty response on error
            }
        }

        public async Task<SalesOrderResponse> UpdateSalesOrderAsync(string orderNumber, SysproOrderItem orderItem)
        {
            // For now, updating orders is not implemented
            // This would typically involve a different business object like SORTOI in update mode
            throw new NotImplementedException("Order updates via API are not yet implemented");
        }

        private async Task<SalesOrderRequest> BuildSalesOrderRequest(SysproOrderItem orderItem)
        {
            var request = new SalesOrderRequest
            {
                Item = new SalesOrderHeader
                {
                    Customer = "TOY020", // Default customer from existing implementation
                    CustomerPoNumber = orderItem.CustomerPurchaseOrder.ToString(),
                    ReqShipDate = orderItem.ShipDate.ToString("yyyy-MM-dd"),
                    ShippingInstrs = orderItem.ShipVia ?? ""
                }
            };

            // Build order lines
            foreach (var line in orderItem.Items)
            {
                // Look up stock code by QuickReference
                var stockLookup = await LookupStockCodeAsync(line.QuickReference);
                var inventoryItem = stockLookup.GetFirstItem();

                if (inventoryItem != null)
                {
                    // Get the correct warehouse from database
                    var invMaster = await _sysproAppService.GetInvMasterByStockCodeAsync(inventoryItem.StockCode);
                    
                    request.Item.SalesOrderLines.Add(new SalesOrderLine
                    {
                        StockCode = inventoryItem.StockCode,
                        Warehouse = invMaster?.WarehouseToUse ?? inventoryItem.Warehouse,
                        OrderQty = line.Quantity,
                        OrderUom = "EA" // Default unit of measure
                    });
                }
                else
                {
                    Console.WriteLine($"Warning: Stock code not found for QuickReference: {line.QuickReference}");
                    // Could optionally add to a "not found" list or log for later processing
                }
            }

            return request;
        }

        private string SerializeToXml<T>(T obj)
        {
            var serializer = new XmlSerializer(typeof(T));
            using (var stringWriter = new StringWriter())
            using (var xmlWriter = XmlWriter.Create(stringWriter, new XmlWriterSettings { Indent = true }))
            {
                serializer.Serialize(xmlWriter, obj);
                return stringWriter.ToString();
            }
        }

        private T DeserializeFromXml<T>(string xml)
        {
            var serializer = new XmlSerializer(typeof(T));
            using (var stringReader = new StringReader(xml))
            {
                return (T)serializer.Deserialize(stringReader);
            }
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }
}