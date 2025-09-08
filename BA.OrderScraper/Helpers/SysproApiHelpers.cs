using BA.OrderScraper.Models;
using BA.OrderScraper.Models.DTO;
using BA.OrderScraper.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BA.OrderScraper.Helpers
{
    /// <summary>
    /// Helper class for creating Syspro orders using API instead of Selenium automation
    /// This replaces the unreliable web browser automation with direct API calls
    /// </summary>
    public static class SysproApiHelpers
    {
        private static readonly ManifestAppService _manifestAppService = new ManifestAppService();
        private static readonly SysproOrderCreationHistoryAppService _sysproOrderCreationHistoryAppService = new SysproOrderCreationHistoryAppService();

        public static async Task CreateSysproOrdersAsync(ISysproApiService sysproApiService)
        {
            try
            {
                Console.WriteLine("Starting Syspro API order creation process...");

                // Test API connection first
                if (!await sysproApiService.TestConnectionAsync())
                {
                    throw new Exception("Unable to connect to Syspro API. Please check configuration and network connectivity.");
                }

                Console.WriteLine("Syspro API connection successful.");

                List<SysproOrderItem> orderItems = new List<SysproOrderItem>();

                // Check for in-progress orders first
                var inProgressOrders = (await _sysproOrderCreationHistoryAppService
                    .GetSysproOrderCreationHistoryAsync(inProgress: true))
                    .Take(3);

                if (inProgressOrders != null && inProgressOrders.Any())
                {
                    Console.WriteLine($"Found {inProgressOrders.Count()} in-progress orders. Resuming...");
                    foreach (var inProgressOrder in inProgressOrders)
                    {
                        var order = await _manifestAppService.GetSysproOrderByManifestNo(inProgressOrder.ManifestNumber, true);
                        if (order != null)
                        {
                            order.OrderNumber = inProgressOrder.OrderNumber ?? "";
                            orderItems.Add(order);
                        }
                    }
                }

                // Add new manifests to process
                orderItems.AddRange(await _manifestAppService.GetTopNManifestsToCreate(4));

                foreach (var sysproOrder in orderItems)
                {
                    if (sysproOrder.Items.Count == 0)
                    {
                        Console.WriteLine($"Skipping manifest {sysproOrder.CustomerPurchaseOrder} - no items to process.");
                        continue;
                    }

                    Console.WriteLine($"Processing manifest: {sysproOrder.CustomerPurchaseOrder} with {sysproOrder.Items.Count} items");

                    await ProcessSingleOrderAsync(sysproApiService, sysproOrder);
                }

                Console.WriteLine("All manifests processed successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in Syspro API order creation: {ex.Message}");
                throw;
            }
        }

        private static async Task ProcessSingleOrderAsync(ISysproApiService sysproApiService, SysproOrderItem sysproOrder)
        {
            SysproOrderCreationHistory? orderInProgress = null;
            string orderNumber = "";

            try
            {
                if (sysproOrder.InProgress && !string.IsNullOrEmpty(sysproOrder.OrderNumber))
                {
                    // Resume existing order
                    Console.WriteLine($"Resuming existing order: {sysproOrder.OrderNumber}");
                    orderNumber = sysproOrder.OrderNumber;
                    
                    orderInProgress = await _sysproOrderCreationHistoryAppService
                        .GetSysproOrderCreationHistoryByManifestNumberAsync(sysproOrder.CustomerPurchaseOrder);

                    if (orderInProgress != null)
                    {
                        // Add remaining items to existing order
                        var itemsToProcess = sysproOrder.Items.Skip(orderInProgress.OrderTotalItemsCompleted);
                        
                        foreach (var item in itemsToProcess)
                        {
                            if (await ValidateAndAddOrderLine(sysproApiService, orderNumber, item))
                            {
                                orderInProgress.OrderTotalItemsCompleted = orderInProgress.OrderTotalItemsCompleted + 1;
                                await _sysproOrderCreationHistoryAppService.CreateOrUpdateSysproOrderCreationHistoryAsync(orderInProgress);
                            }
                        }
                    }
                }
                else
                {
                    // Create new order
                    Console.WriteLine($"Creating new order for manifest: {sysproOrder.CustomerPurchaseOrder}");
                    
                    orderNumber = await sysproApiService.CreateSalesOrderAsync(sysproOrder);
                    
                    if (string.IsNullOrEmpty(orderNumber))
                    {
                        throw new Exception($"Failed to create sales order for manifest: {sysproOrder.CustomerPurchaseOrder}");
                    }

                    Console.WriteLine($"Created order: {orderNumber}");

                    // Create progress tracking record
                    orderInProgress = await _sysproOrderCreationHistoryAppService
                        .CreateOrUpdateSysproOrderCreationHistoryAsync(new SysproOrderCreationHistory()
                        {
                            ManifestNumber = sysproOrder.CustomerPurchaseOrder,
                            InProgress = true,
                            UpdatedDate = DateTime.Now,
                            OrderNumber = orderNumber,
                            OrderTotalItems = sysproOrder.Items.Count,
                            OrderTotalItemsCompleted = 0
                        });

                    // Process all items for new order
                    foreach (var item in sysproOrder.Items)
                    {
                        if (await ValidateAndAddOrderLine(sysproApiService, orderNumber, item))
                        {
                            orderInProgress.OrderTotalItemsCompleted = orderInProgress.OrderTotalItemsCompleted + 1;
                            await _sysproOrderCreationHistoryAppService.CreateOrUpdateSysproOrderCreationHistoryAsync(orderInProgress);
                        }
                    }
                }

                // Mark order as complete
                if (orderInProgress != null)
                {
                    orderInProgress.InProgress = false;
                    orderInProgress.CreationSuccess = true;
                    orderInProgress.UpdatedDate = DateTime.Now;
                    await _sysproOrderCreationHistoryAppService.CreateOrUpdateSysproOrderCreationHistoryAsync(orderInProgress);
                }

                Console.WriteLine($"Successfully completed order: {orderNumber}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing order for manifest {sysproOrder.CustomerPurchaseOrder}: {ex.Message}");

                // Mark order as failed if we have tracking record
                if (orderInProgress != null)
                {
                    orderInProgress.InProgress = false;
                    orderInProgress.CreationSuccess = false;
                    orderInProgress.UpdatedDate = DateTime.Now;
                    await _sysproOrderCreationHistoryAppService.CreateOrUpdateSysproOrderCreationHistoryAsync(orderInProgress);
                }

                throw;
            }
        }

        private static async Task<bool> ValidateAndAddOrderLine(ISysproApiService sysproApiService, string orderNumber, SysproOrderLine item)
        {
            try
            {
                Console.WriteLine($"Processing item: {item.QuickReference}, Qty: {item.Quantity}");

                // Validate stock code exists
                if (!await sysproApiService.ValidateStockCodeAsync(item.QuickReference))
                {
                    Console.WriteLine($"Warning: Stock code {item.QuickReference} not found in inventory. Skipping item.");
                    return false;
                }

                // Add the line to the order
                bool success = await sysproApiService.AddOrderLineAsync(orderNumber, item);
                
                if (success)
                {
                    Console.WriteLine($"Successfully added item: {item.QuickReference}");
                }
                else
                {
                    Console.WriteLine($"Failed to add item: {item.QuickReference}");
                }

                return success;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing item {item.QuickReference}: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Validates the Syspro API configuration
        /// </summary>
        public static async Task<bool> ValidateConfigurationAsync(ISysproApiService sysproApiService)
        {
            try
            {
                Console.WriteLine("Validating Syspro API configuration...");

                bool connectionTest = await sysproApiService.TestConnectionAsync();
                
                if (connectionTest)
                {
                    Console.WriteLine("✓ Syspro API configuration is valid");
                }
                else
                {
                    Console.WriteLine("✗ Syspro API configuration failed validation");
                }

                return connectionTest;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Configuration validation error: {ex.Message}");
                return false;
            }
        }
    }
}