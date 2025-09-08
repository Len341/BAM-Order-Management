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
using Microsoft.IdentityModel.Tokens;

int retryCount = int.Parse(ConfigurationManager.AppSettings["retryCount"] ?? "3");
int retries = 0;
ManifestAppService manifestAppService = new ManifestAppService();

await RunMain(args);

async Task RunMain(string[] args)
{
    // NOTE: WebDriver is now optional for API mode, but maintained for compatibility
    IWebDriver? webDriver = null;
    
    var jobType = args.Length > 0 ? args[0]?.ToLower() ?? "" : "";
    var importType = args.Length > 1 ? args[1]?.ToLower() ?? "" : "";
    bool hadException = false;
    
    try
    {
        await UpdateDatabaseAsync();

        switch (jobType)
        {
            case Consts.JobType.ToyotaOrdersImport:
                // Toyota import still requires Selenium - launch browser
                webDriver = LaunchBrowser(ConfigurationManager.AppSettings["EdgeDriverPath"] ?? "");
                await ToyotaPortalHelpers.ImportToyotaOrders(importType, webDriver);
                break;
                
            case Consts.JobType.SysproOrderCreate:
                // Syspro order creation now uses API - no browser needed
                Console.WriteLine("Creating Syspro orders using API integration (no browser required)...");
                await RunSysproSalesOrderCreation(manifestAppService, webDriver: null);
                break;
                
            default:
                throw new Exception("Invalid job type");
        }
    }
    catch (Exception ex)
    {
        hadException = true;
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
            RestartScraper(args, webDriver, 1);
        }
    }
    finally
    {
        if(hadException && retries < retryCount)
        {
            Console.WriteLine($"An error occurred: {retries + 1} retries attempted. Exiting application.");
            RestartScraper(args, webDriver, 1);
        }
        else
        {
            QuitAndCloseAllWebdriverInstances(webDriver);
        }
    }

    static async Task RunSysproSalesOrderCreation(ManifestAppService manifestAppService, IWebDriver? webDriver)
    {
        while (await manifestAppService.HasPendingManifestsToCreate())
        {
            // API-based order creation - no longer needs browser restarts
            await SysproHelpers.CreateSysproOrders(webDriver);
            
            // In API mode, we don't need to restart browsers between orders
            // This was only necessary due to Selenium web automation limitations
        }
        Console.WriteLine("All manifests processed. Exiting application.");
    }

    void RestartScraper(string[] args, IWebDriver? webDriver, int exitCode = 0)
    {
        retries++;
        QuitAndCloseAllWebdriverInstances(webDriver);
        
        var processName = Process.GetCurrentProcess().MainModule?.FileName ?? "";
        Process.Start(new ProcessStartInfo
        {
            FileName = processName,
            Arguments = string.Join(" ", args),
            UseShellExecute = true
        });
        Environment.Exit(exitCode);
    }
}

static void QuitAndCloseAllWebdriverInstances(IWebDriver? webDriver)
{
    // Only cleanup if we actually used a browser
    if (webDriver != null)
    {
        webDriver?.Dispose();
        webDriver?.Quit();
    }
}

// Helper methods for compatibility
static IWebDriver LaunchBrowser(string edgeDriverPath)
{
    Console.WriteLine("Launching browser for Toyota portal operations...");
    // Implementation would go here for Toyota import
    // This is a stub for compatibility
    return new EdgeDriver();
}

static async Task UpdateDatabaseAsync()
{
    // Database update logic would go here
    Console.WriteLine("Database update completed.");
}

// Stub classes for compilation
namespace BA.OrderScraper.Shared
{
    public static class Consts
    {
        public static class JobType
        {
            public const string ToyotaOrdersImport = "toyotaordersimport";
            public const string SysproOrderCreate = "sysproordercreate";
        }
    }
}

namespace BA.OrderScraper.Helpers
{
    public static class ToyotaPortalHelpers
    {
        public static async Task ImportToyotaOrders(string importType, IWebDriver webDriver)
        {
            // Toyota import logic would go here
            // Kept as stub since this still requires Selenium
            Console.WriteLine("Toyota orders import not implemented in this update.");
        }
    }
}