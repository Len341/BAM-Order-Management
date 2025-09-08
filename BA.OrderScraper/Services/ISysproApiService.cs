using BA.OrderScraper.Models.DTO;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BA.OrderScraper.Services
{
    /// <summary>
    /// Interface for Syspro API integration to create sales orders
    /// </summary>
    public interface ISysproApiService
    {
        /// <summary>
        /// Creates a sales order in Syspro via API
        /// </summary>
        /// <param name="orderItem">The order item containing customer PO, items, etc.</param>
        /// <returns>The created order number from Syspro</returns>
        Task<string> CreateSalesOrderAsync(SysproOrderItem orderItem);

        /// <summary>
        /// Adds a line item to an existing sales order
        /// </summary>
        /// <param name="orderNumber">The sales order number</param>
        /// <param name="orderLine">The line item to add</param>
        /// <returns>Success status</returns>
        Task<bool> AddOrderLineAsync(string orderNumber, SysproOrderLine orderLine);

        /// <summary>
        /// Tests the connection to Syspro API
        /// </summary>
        /// <returns>True if connection is successful</returns>
        Task<bool> TestConnectionAsync();

        /// <summary>
        /// Validates that a stock code exists in Syspro inventory
        /// </summary>
        /// <param name="stockCode">The stock code to validate</param>
        /// <returns>True if stock code exists</returns>
        Task<bool> ValidateStockCodeAsync(string stockCode);
    }
}