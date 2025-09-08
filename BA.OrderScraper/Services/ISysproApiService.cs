using BA.OrderScraper.Models.DTO;
using BA.OrderScraper.Models.Syspro.Api;
using System.Threading.Tasks;

namespace BA.OrderScraper.Services
{
    /// <summary>
    /// Interface for Syspro API operations
    /// Replaces Selenium-based web automation with direct API calls
    /// </summary>
    public interface ISysproApiService
    {
        /// <summary>
        /// Creates a sales order in Syspro using the API
        /// </summary>
        /// <param name="orderItem">The order details to create</param>
        /// <returns>Response containing order number or error details</returns>
        Task<SalesOrderResponse> CreateSalesOrderAsync(SysproOrderItem orderItem);

        /// <summary>
        /// Looks up stock code by alternate key (QuickReference)
        /// </summary>
        /// <param name="alternateKey">The QuickReference to search for</param>
        /// <returns>Stock code lookup results</returns>
        Task<StockCodeLookupResponse> LookupStockCodeAsync(string alternateKey);

        /// <summary>
        /// Updates an existing sales order
        /// </summary>
        /// <param name="orderNumber">Existing order number</param>
        /// <param name="orderItem">Updated order details</param>
        /// <returns>Response containing success/failure status</returns>
        Task<SalesOrderResponse> UpdateSalesOrderAsync(string orderNumber, SysproOrderItem orderItem);

        /// <summary>
        /// Tests the API connection to Syspro
        /// </summary>
        /// <returns>True if connection is successful</returns>
        Task<bool> TestConnectionAsync();

        /// <summary>
        /// Authenticates with Syspro and establishes a session
        /// </summary>
        /// <returns>True if authentication successful</returns>
        Task<bool> AuthenticateAsync();
    }
}