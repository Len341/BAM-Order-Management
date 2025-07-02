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
    }
}
