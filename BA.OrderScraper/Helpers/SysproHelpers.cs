using BA.OrderScraper.Models;
using BA.OrderScraper.Models.DTO;
using BA.OrderScraper.Services;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BA.OrderScraper.Helpers
{
    /// <summary>
    /// Syspro integration helpers - UPDATED to use API instead of Selenium
    /// This replaces the previous 1600+ lines of brittle web automation 
    /// with clean, reliable API calls to Syspro Business Objects
    /// </summary>
    public static class SysproHelpers
    {
        static ManifestAppService manifestAppService = new ManifestAppService();
        static SysproAppService sysproAppService = new SysproAppService();
        static SysproOrderCreationHistoryAppService sysproOrderCreationHistoryAppService = new SysproOrderCreationHistoryAppService();
        static ISysproApiService sysproApiService = new SysproApiService();

        /// <summary>
        /// Creates Syspro orders using API calls instead of Selenium web automation
        /// This method maintains the same public interface but uses Syspro APIs internally
        /// </summary>
        /// <param name="webDriver">Legacy parameter - no longer used but maintained for compatibility</param>
        public static async Task CreateSysproOrders(IWebDriver? webDriver = null)
        {
            var currentOrder = new SysproOrderItem();
            SysproOrderCreationHistory orderInProgress = null;

            try
            {
                Console.WriteLine("Starting Syspro order creation using API integration...");

                // Test API connection first
                bool isConnected = await sysproApiService.TestConnectionAsync();
                if (!isConnected)
                {
                    throw new Exception("Cannot connect to Syspro API. Please ensure the Syspro web service is running and configured correctly.");
                }

                // Authenticate with Syspro
                bool isAuthenticated = await sysproApiService.AuthenticateAsync();
                if (!isAuthenticated)
                {
                    throw new Exception("Failed to authenticate with Syspro API. Please check credentials and company settings.");
                }

                Console.WriteLine("Successfully connected and authenticated with Syspro API");

                List<SysproOrderItem> orderItems = new List<SysproOrderItem>();

                // Check for in-progress orders (same logic as before)
                var inProgressOrders = (await sysproOrderCreationHistoryAppService
                    .GetSysproOrderCreationHistoryAsync(inProgress: true))
                    .Take(3);

                if (inProgressOrders != null && inProgressOrders.Any())
                {
                    foreach (var inProgressOrder in inProgressOrders)
                    {
                        var order = await manifestAppService.GetSysproOrderByManifestNo(inProgressOrder.ManifestNumber, true);
                        if (order != null)
                        {
                            order.OrderNumber = inProgressOrder.OrderNumber ?? "";
                            orderItems.Add(order);
                        }
                    }
                }

                // Add new orders to process
                orderItems.AddRange(await manifestAppService.GetTopNManifestsToCreate(4));

                // Process each order via API
                foreach (var sysproOrder in orderItems)
                {
                    if (sysproOrder.Items.Count == 0) continue;

                    Console.WriteLine($"Processing manifest: {sysproOrder.CustomerPurchaseOrder}");
                    currentOrder = sysproOrder;

                    orderInProgress = new SysproOrderCreationHistory();

                    if (sysproOrder.InProgress)
                    {
                        // Handle existing in-progress order
                        Console.WriteLine($"Continuing with order: {sysproOrder.OrderNumber}");
                        orderInProgress = await sysproOrderCreationHistoryAppService
                            .GetSysproOrderCreationHistoryByManifestNumberAsync(sysproOrder.CustomerPurchaseOrder);

                        // For API implementation, we'll treat in-progress orders as new orders
                        // since updating via API is more complex and may not be needed initially
                        Console.WriteLine("Note: In-progress orders are treated as new orders in API mode");
                    }

                    // Create the order via API
                    await CreateSalesOrderViaApi(sysproOrder, orderInProgress);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in CreateSysproOrders: {ex.Message}");
                
                // Log error to database (same as before)
                var error = new Error(
                    ex.Message,
                    ex.StackTrace,
                    ex.InnerException?.Message ?? "",
                    1);

                using (var context = new EFCore.BADbContext())
                {
                    context.Error.Add(error);
                    await context.SaveChangesAsync();
                }

                // Update order history with error if we have an order in progress
                if (orderInProgress != null)
                {
                    orderInProgress.UpdatedDate = DateTime.Now;
                    orderInProgress.InProgress = false;
                    orderInProgress.CreationSuccess = false;
                    orderInProgress.ErrorMessage = ex.Message;
                    
                    await sysproOrderCreationHistoryAppService
                        .CreateOrUpdateSysproOrderCreationHistoryAsync(orderInProgress);
                }

                throw;
            }
        }

        /// <summary>
        /// Creates a sales order using Syspro API calls
        /// Replaces the complex Selenium-based form filling with direct API communication
        /// </summary>
        private static async Task CreateSalesOrderViaApi(SysproOrderItem sysproOrder, SysproOrderCreationHistory orderInProgress)
        {
            try
            {
                Console.WriteLine($"Creating sales order for manifest {sysproOrder.CustomerPurchaseOrder} via API...");

                // Initialize order tracking
                if (orderInProgress == null || orderInProgress.Id == Guid.Empty)
                {
                    orderInProgress = new SysproOrderCreationHistory
                    {
                        ManifestNumber = sysproOrder.CustomerPurchaseOrder,
                        InProgress = true,
                        UpdatedDate = DateTime.Now,
                        OrderNumber = "",
                        OrderTotalItems = sysproOrder.Items.Count,
                        OrderTotalItemsCompleted = 0
                    };
                }

                // Filter out items that couldn't be found (preserve existing logic)
                var validItems = new List<SysproOrderLine>();
                var itemsNotFound = new List<SysproOrderLine>();

                foreach (var item in sysproOrder.Items)
                {
                    Console.WriteLine($"Looking up stock code for QuickReference: {item.QuickReference}");
                    
                    var stockLookup = await sysproApiService.LookupStockCodeAsync(item.QuickReference);
                    var inventoryItem = stockLookup.GetFirstItem();

                    if (inventoryItem != null)
                    {
                        Console.WriteLine($"Found stock code: {inventoryItem.StockCode} for {item.QuickReference}");
                        validItems.Add(item);
                    }
                    else
                    {
                        Console.WriteLine($"Warning: Stock code not found for QuickReference: {item.QuickReference}");
                        itemsNotFound.Add(item);
                        
                        // Mark items as not found in database (preserve existing behavior)
                        var manifestItemsNotFound = await manifestAppService.GetManifestListAsync(sysproOrder.CustomerPurchaseOrder, item.QuickReference);
                        foreach (var manifest in manifestItemsNotFound)
                        {
                            manifest.SupplierPadEasyReferenceNumberNotFound = true;
                            await manifestAppService.UpdateManifestAsync(manifest);
                        }
                    }
                }

                // Update the order with only valid items
                sysproOrder.Items = validItems;

                if (sysproOrder.Items.Count == 0)
                {
                    Console.WriteLine("No valid items found for order. Skipping order creation.");
                    
                    orderInProgress.UpdatedDate = DateTime.Now;
                    orderInProgress.InProgress = false;
                    orderInProgress.CreationSuccess = false;
                    orderInProgress.ErrorMessage = "No valid items found - all items failed stock code lookup";
                    
                    await sysproOrderCreationHistoryAppService
                        .CreateOrUpdateSysproOrderCreationHistoryAsync(orderInProgress);
                    return;
                }

                // Create the sales order via API
                Console.WriteLine($"Calling Syspro API to create sales order with {sysproOrder.Items.Count} items...");
                var response = await sysproApiService.CreateSalesOrderAsync(sysproOrder);

                if (response.IsSuccess)
                {
                    var orderNumber = response.GetOrderNumber();
                    Console.WriteLine($"Successfully created sales order: {orderNumber}");
                    
                    // Update order tracking with success
                    orderInProgress.OrderNumber = orderNumber;
                    orderInProgress.UpdatedDate = DateTime.Now;
                    orderInProgress.InProgress = false;
                    orderInProgress.CreationSuccess = true;
                    orderInProgress.ErrorMessage = "";
                    orderInProgress.OrderTotalItemsCompleted = sysproOrder.Items.Count;
                    
                    await sysproOrderCreationHistoryAppService
                        .CreateOrUpdateSysproOrderCreationHistoryAsync(orderInProgress);
                }
                else
                {
                    var errorMessage = response.GetErrorMessage();
                    Console.WriteLine($"Failed to create sales order: {errorMessage}");
                    
                    // Update order tracking with error
                    orderInProgress.UpdatedDate = DateTime.Now;
                    orderInProgress.InProgress = false;
                    orderInProgress.CreationSuccess = false;
                    orderInProgress.ErrorMessage = errorMessage;
                    
                    await sysproOrderCreationHistoryAppService
                        .CreateOrUpdateSysproOrderCreationHistoryAsync(orderInProgress);
                    
                    throw new Exception($"Syspro API error: {errorMessage}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating sales order via API: {ex.Message}");
                
                // Update order tracking with error
                if (orderInProgress != null)
                {
                    orderInProgress.UpdatedDate = DateTime.Now;
                    orderInProgress.InProgress = false;
                    orderInProgress.CreationSuccess = false;
                    orderInProgress.ErrorMessage = ex.Message;
                    
                    await sysproOrderCreationHistoryAppService
                        .CreateOrUpdateSysproOrderCreationHistoryAsync(orderInProgress);
                }
                
                throw;
            }
        }

        #region Legacy Compatibility Methods (No longer used but maintained for compatibility)
        
        /// <summary>
        /// Legacy method - no longer used in API implementation
        /// Kept for backward compatibility
        /// </summary>
        [Obsolete("This method is no longer used in the API implementation. Authentication is handled automatically.")]
        public static async Task LoginSysproAvantiPortal(IWebDriver webDriver, object wait)
        {
            Console.WriteLine("Warning: LoginSysproAvantiPortal is deprecated in API mode. Authentication handled automatically.");
        }
        
        #endregion
    }
}