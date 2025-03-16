using BA.OrderScraper.EFCore;
using BA.OrderScraper.Models.DTO;
using BA.OrderScraper.Models;
using LinqKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using OpenQA.Selenium.DevTools.V130.Debugger;

namespace BA.OrderScraper.Services
{
    public class ManifestAppService
    {
        public async Task<List<SysproOrderItem>> GetTopNManifestsToCreate(int n = 1)
        {
            using (var context = new BADbContext())
            {
                var sysproOrders = new List<SysproOrderItem>();
                //create a PredicateBuilder
                var predicate = PredicateBuilder.New<Manifest>();

                var sysproManifestsHistory = await context.SysproOrderCreationHistory.AsNoTracking()
                    .ToListAsync();

                var ordersGroupedByManifestNo = (await context.StagingManifest.AsNoTracking().ToListAsync())
                    .Where(z => !z.SupplierPadEasyReferenceNumberNotFound)//filter out items if they could not previously be found
                    .GroupBy(z => z.SupplierManifestNo);

                ordersGroupedByManifestNo = ordersGroupedByManifestNo
                    //.OrderBy(z => z.ToList().Count)
                    .OrderByDescending(z => z.FirstOrDefault()?.ImportTime);

                ordersGroupedByManifestNo.ForEach(ordersByManifestNo =>
                {
                    //check if the manifest has already been created
                    if (sysproManifestsHistory.Any(h => h.ManifestNumber == ordersByManifestNo.Key /*&& h.CreationSuccess*/)) return;

                    var itemsGroupedByPartNo = ordersByManifestNo
                        .GroupBy(z => z.SupplierPadEasyReferenceNumber);

                    var order = new SysproOrderItem
                    {
                        CustomerPurchaseOrder = ordersByManifestNo.Key,
                        Items = new List<SysproOrderLine>()
                    };

                    itemsGroupedByPartNo.ForEach(itemsByPartNo =>
                    {
                        order.Items.Add(new SysproOrderLine()
                        {
                            Quantity = itemsByPartNo.Count(),
                            QuickReference = itemsByPartNo.Key
                        });
                        order.ShipDate = itemsByPartNo.FirstOrDefault()!.ShipDate;
                        order.ShipVia = itemsByPartNo.FirstOrDefault()!.PickupRoute;
                    });

                    sysproOrders.Add(order);
                });


                return sysproOrders.Take(n).ToList();
            }
        }

        public async Task<SysproOrderItem> GetNextManifestsToCreate()
        {
            using (var context = new BADbContext())
            {
                //create a PredicateBuilder
                var predicate = PredicateBuilder.New<Manifest>();

                var sysproManifestsHistory = await context.SysproOrderCreationHistory.AsNoTracking()
                    .ToListAsync();

                var ordersGroupedByManifestNo = (await context.StagingManifest.AsNoTracking().ToListAsync())
                    .GroupBy(z => z.SupplierManifestNo);

                ordersGroupedByManifestNo = ordersGroupedByManifestNo
                    //.OrderBy(z => z.ToList().Count)
                    .OrderByDescending(z => z.FirstOrDefault()?.ImportTime);

                var manifestNumbers = ordersGroupedByManifestNo.Select(z => z.Key).ToList();
                int nextManifestToProcess = 0;
                if (sysproManifestsHistory.Any())
                {
                    for (int i = 0; i < manifestNumbers.Count; i++)
                    {
                        if (!sysproManifestsHistory.Any(h => h.ManifestNumber == manifestNumbers[i]))
                        {
                            nextManifestToProcess = manifestNumbers[i];
                            break;
                        }
                    }
                }

                var nextOrder = ordersGroupedByManifestNo.FirstOrDefault(z => z.Key == nextManifestToProcess);
                if (nextOrder != null)
                {
                    var itemsGroupedByPartNo = nextOrder
                        .GroupBy(z => z.SupplierPadEasyReferenceNumber);

                    var order = new SysproOrderItem
                    {
                        CustomerPurchaseOrder = nextOrder.Key,
                        Items = new List<SysproOrderLine>()
                    };

                    itemsGroupedByPartNo.ForEach(itemsByPartNo =>
                    {
                        order.Items.Add(new SysproOrderLine()
                        {
                            Quantity = itemsByPartNo.Count(),
                            QuickReference = itemsByPartNo.Key
                        });
                        order.ShipDate = itemsByPartNo.FirstOrDefault()!.ShipDate;
                        order.ShipVia = itemsByPartNo.FirstOrDefault()!.PickupRoute;
                    });

                    return order;
                }
                else
                {
                    return null;
                }
            }
        }

        public async Task<SysproOrderItem> GetSysproOrderByManifestNo(int manifestNo, bool inProgress = false)
        {
            using (var context = new BADbContext())
            {
                var sysproOrder = (await context.StagingManifest
                    .AsNoTracking()
                    .Where(z => z.SupplierManifestNo == manifestNo)
                    .ToListAsync())
                    .Where(z => !z.SupplierPadEasyReferenceNumberNotFound)//filter out items if they could not previously be found
                    .GroupBy(z => z.SupplierManifestNo)
                    .FirstOrDefault();

                if (sysproOrder != null)
                {
                    var itemsGroupedByPartNo = sysproOrder
                        .GroupBy(z => z.SupplierPadEasyReferenceNumber);

                    var order = new SysproOrderItem
                    {
                        CustomerPurchaseOrder = sysproOrder.Key,
                        InProgress = inProgress,
                        Items = new List<SysproOrderLine>()
                    };

                    itemsGroupedByPartNo.ForEach(itemsByPartNo =>
                    {
                        order.Items.Add(new SysproOrderLine()
                        {
                            Quantity = itemsByPartNo.Count(),
                            QuickReference = itemsByPartNo.Key
                        });
                        order.ShipDate = itemsByPartNo.FirstOrDefault()!.ShipDate;
                        order.ShipVia = itemsByPartNo.FirstOrDefault()!.PickupRoute;
                    });

                    return order;
                }
                else
                {
                    return null;
                }
            }
        }

        //create a method to see if there are any manifests to create
        public async Task<bool> HasPendingManifestsToCreate()
        {
            using (var context = new BADbContext())
            {
                try
                {
                    var sysproManifestsHistory = await context.SysproOrderCreationHistory.AsNoTracking()
                        .ToListAsync();

                    var ordersGroupedByManifestNo = (await context.StagingManifest.AsNoTracking()
                        .OrderByDescending(z => z.ImportTime)
                        .ToListAsync()
                        ).GroupBy(z => z.SupplierManifestNo);
                    return ordersGroupedByManifestNo.Any(ordersByManifestNo =>
                    {
                        //check if the manifest has already been created
                        return !sysproManifestsHistory.Any(h => h.ManifestNumber == ordersByManifestNo.Key /*&& h.CreationSuccess*/);
                    });
                }
                catch (Exception ex)
                {
                    throw;
                }
            }
        }

        public async Task<Manifest> UpdateManifestAsync(Manifest manifest)
        {
            using (var context = new BADbContext())
            {
                context.StagingManifest.Update(manifest);
                await context.SaveChangesAsync();
                return manifest;
            }
        }

        public async Task<Manifest?> GetManifestAsync(int manifestId)
        {
            using (var context = new BADbContext())
            {
                var manifest = await context.StagingManifest
                    .FirstOrDefaultAsync(z => z.SupplierManifestNo == manifestId);
                return manifest;
            }
        }
        public async Task<Manifest?> GetManifestAsync(int manifestId, string padEasyRef)
        {
            using (var context = new BADbContext())
            {
                var manifest = await context.StagingManifest
                    .FirstOrDefaultAsync(z => z.SupplierManifestNo == manifestId && z.SupplierPadEasyReferenceNumber == padEasyRef);
                return manifest;
            }
        }
        public async Task<List<Manifest>> GetManifestListAsync(int manifestId, string padEasyRef)
        {
            using (var context = new BADbContext())
            {
                var manifests = await context.StagingManifest
                    .Where(z => z.SupplierManifestNo == manifestId && z.SupplierPadEasyReferenceNumber == padEasyRef)
                    .ToListAsync();
                return manifests;
            }
        }
    }
}
