
using BA.OrderScraper.EFCore;
using BA.OrderScraper.Models;
using OfficeOpenXml;
using OpenQA.Selenium;
using OpenQA.Selenium.Edge;
using System.Globalization;
using System.Configuration;
using System.Diagnostics;
using BA.OrderScraper.Helpers;
using Microsoft.EntityFrameworkCore;
using static BA.OrderScraper.Shared.Consts;
using BA.OrderScraper.Shared;
using BA.OrderScraper.Services;

int retryCount = int.Parse(ConfigurationManager.AppSettings["retryCount"]);
int retries = 0;
ManifestAppService manifestAppService = new ManifestAppService();

await RunMain(args);

async Task RunMain(string[] args)
{
    IWebDriver? webDriver = GeneralHelpers.LaunchBrowser(ConfigurationManager.AppSettings["EdgeDriverPath"] ?? "");
    var jobType = args.Length > 0 ? args[0]?.ToLower() ?? "" : "";
    var importType = args.Length > 1 ? args[1]?.ToLower() ?? "" : "";
    try
    {
        await GeneralHelpers.UpdateDatabaseAsync();

        switch (jobType)
        {
            case Consts.JobType.ToyotaOrdersImport:
                await ToyotaPortalHelpers.ImportToyotaOrders(importType, webDriver);
                break;
            case Consts.JobType.SysproOrderCreate:
                if (await manifestAppService.HasPendingManifestsToCreate())
                {
                    await SysproHelpers.CreateSysproOrders(webDriver);
                    QuitAndCloseAllWebdriverInstances(webDriver);
                }
                break;
            default:
                throw new Exception("Invalid job type");
        }
    }
    catch (Exception ex)
    {
        var error = new Error(
            ex.Message,
            ex.StackTrace,
            ex.InnerException?.Message ?? "",
            retries + 1);

        using (var context = new BADbContext())
        {
            context.Error.Add(error);
            await context.SaveChangesAsync();
        }
        if (retries < retryCount)
        {
            retries++;
            QuitAndCloseAllWebdriverInstances(webDriver);
            await RunMain(args);
        }
    }
    finally
    {
        Thread.Sleep(10000);
        QuitAndCloseAllWebdriverInstances(webDriver);
    }
}

static void QuitAndCloseAllWebdriverInstances(IWebDriver? webDriver)
{
    webDriver?.Dispose();
    //webDriver?.Close();
    webDriver?.Quit();
    //var processesToKill = Process.GetProcessesByName("msedgedriver").ToList();
    ////var baOrderScraper = Process.GetProcessesByName("BA.OrderScraper").ToList();
    ////processesToKill.AddRange(baOrderScraper);
    //if (processesToKill != null && processesToKill.Count > 0)
    //{
    //    foreach (Process worker in processesToKill)
    //    {
    //        worker.Kill();
    //        //worker.WaitForExit();
    //        worker.Dispose();
    //    }
    //}
    //Process.Start("taskkill", $"/F /IM {ConfigurationManager.AppSettings["PublishPath"].TrimEnd('\\')}\\BA.OrderScraper.exe /T");
}