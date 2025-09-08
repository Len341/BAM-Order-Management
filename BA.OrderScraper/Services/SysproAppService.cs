using BA.OrderScraper.EFCore;
using BA.OrderScraper.Models.Syspro;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
//using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BA.OrderScraper.Services
{
    public class SysproAppService
    {
        public async Task<List<InvMaster>> GetInvMasterListAsync()
        {
            try
            {
                using (var context = new SysproDbContext())
                {
                    var invMasterList = await context.InvMaster.AsNoTracking().ToListAsync();
                    return invMasterList;
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public async Task<InvMaster> GetInvMasterByStockCodeAsync(string stockCode)
        {
            try
            {
                using (var context = new SysproDbContext())
                {
                    var invMaster = await context.InvMaster.AsNoTracking()
                        .FirstOrDefaultAsync(z => z.StockCode == stockCode);
                    return invMaster;
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public async Task<List<SorMaster>> GetSorMasterListAsync()
        {
            try
            {
                using (var context = new SysproDbContext())
                {
                    var sorMasterList = await context.SorMaster.AsNoTracking().ToListAsync();
                    return sorMasterList;
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public async Task<bool> PurchaseOrderExists(string poNumber)
        {
            try
            {
                using (var context = new SysproDbContext())
                {
                    return await context.SorMaster.AnyAsync(z => z.CustomerPoNumber.Trim().ToLower() == poNumber.Trim().ToLower());
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public async Task<List<string>> ExistingPurchaseOrderNumbers()
        {
            try
            {
                using (var context = new SysproDbContext())
                {
                    return await context.SorMaster.Select(z => z.CustomerPoNumber.Trim().ToLower()).ToListAsync();
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
