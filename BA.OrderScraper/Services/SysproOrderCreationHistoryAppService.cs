using BA.OrderScraper.EFCore;
using BA.OrderScraper.Models;
using LinqKit;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BA.OrderScraper.Services
{
    public class SysproOrderCreationHistoryAppService
    {
        public async Task<SysproOrderCreationHistory> CreateOrUpdateSysproOrderCreationHistoryAsync(Models.SysproOrderCreationHistory sysproOrderCreationHistory)
        {
            using (var context = new BADbContext())
            {
                if (context.SysproOrderCreationHistory.Any(x => x.Id == sysproOrderCreationHistory.Id))
                {
                    var existingSysproOrderCreationHistory = context.SysproOrderCreationHistory.First(x => x.Id == sysproOrderCreationHistory.Id);
                    existingSysproOrderCreationHistory.CreationSuccess = sysproOrderCreationHistory.CreationSuccess;
                    existingSysproOrderCreationHistory.ErrorMessage = sysproOrderCreationHistory.ErrorMessage;
                    existingSysproOrderCreationHistory.InProgress = sysproOrderCreationHistory.InProgress;
                    existingSysproOrderCreationHistory.OrderNumber = sysproOrderCreationHistory.OrderNumber;
                    existingSysproOrderCreationHistory.UpdatedDate = DateTime.Now;
                    await context.SaveChangesAsync();
                    return existingSysproOrderCreationHistory;
                }
                context.SysproOrderCreationHistory.Add(sysproOrderCreationHistory);
                try
                {
                    await context.Database.OpenConnectionAsync();
                    await context.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT dbo.SysproOrderCreationHistory ON");
                    await context.SaveChangesAsync();
                    await context.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT dbo.SysproOrderCreationHistory ON");
                }
                finally
                {
                    await context.Database.CloseConnectionAsync();
                }
                return sysproOrderCreationHistory;
            }
        }

        public async Task<List<Models.SysproOrderCreationHistory>> GetSysproOrderCreationHistoryAsync(
            bool? inProgress = null,
            bool? isComplete = null,
            bool? error = null)
        {
            using (var context = new BADbContext())
            {
                var predicate = PredicateBuilder.New<Models.SysproOrderCreationHistory>(x => true);

                if(inProgress.HasValue) predicate = predicate.And(x => x.InProgress == inProgress.Value);
                if (isComplete.HasValue) predicate = predicate.And(x => x.CreationSuccess == isComplete.Value);
                if (error.HasValue) predicate = predicate.And(x => x.ErrorMessage != null && x.ErrorMessage.Trim() != string.Empty);

                return context.SysproOrderCreationHistory.Where(predicate).ToList();
            }
        }

        public async Task<Models.SysproOrderCreationHistory> GetSysproOrderCreationHistoryByManifestNumberAsync(int manifestNumber)
        {
            using (var context = new BADbContext())
            {
                return context.SysproOrderCreationHistory.FirstOrDefault(x => x.ManifestNumber == manifestNumber);
            }
        }

        public async Task DeleteSysproOrderCreationHistoryByManifestNumberAsync(int manifestNumber)
        {
            using (var context = new BADbContext())
            {
                var sysproOrderCreationHistory = context.SysproOrderCreationHistory.FirstOrDefault(x => x.ManifestNumber == manifestNumber);
                if (sysproOrderCreationHistory != null)
                {
                    context.SysproOrderCreationHistory.Remove(sysproOrderCreationHistory);
                    await context.SaveChangesAsync();
                }
            }
        }

        public async Task DeleteAllSysproOrderCreationHistoryAsync()
        {
            using (var context = new BADbContext())
            {
                context.SysproOrderCreationHistory.RemoveRange(context.SysproOrderCreationHistory);
                await context.SaveChangesAsync();
            }
        }

        public async Task UpdateSysproOrderCreationHistoryAsync(Models.SysproOrderCreationHistory sysproOrderCreationHistory)
        {
            using (var context = new BADbContext())
            {
                context.SysproOrderCreationHistory.Update(sysproOrderCreationHistory);
                await context.SaveChangesAsync();
            }
        }
    }
}
