
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
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

int retryCount = int.Parse(ConfigurationManager.AppSettings["retryCount"] ?? "3");
int retries = 0;
ManifestAppService manifestAppService = new ManifestAppService();

await RunMain(args);

async Task RunMain(string[] args)
{
    IWebDriver? webDriver = null;
    var jobType = args.Length > 0 ? args[0]?.ToLower() ?? "" : "";
    var importType = args.Length > 1 ? args[1]?.ToLower() ?? "" : "";
    var useApi = args.Length > 2 && args[2]?.ToLower() == "api"; // New parameter to force API usage
    bool hadException = false;
    
    try
    {
        await GeneralHelpers.UpdateDatabaseAsync();

        switch (jobType)
        {
            case Consts.JobType.ToyotaOrdersImport:
                // Toyota import still uses web scraping
                webDriver = GeneralHelpers.LaunchBrowser(ConfigurationManager.AppSettings["EdgeDriverPath"] ?? "");
                await ToyotaPortalHelpers.ImportToyotaOrders(importType, webDriver);
                break;
            case Consts.JobType.SysproOrderCreate:
                if (useApi)
                {
                    // Use new API-based approach
                    await RunSysproApiOrderCreation(manifestAppService);
                }
                else
                {
                    // Fallback to existing Selenium approach
                    webDriver = GeneralHelpers.LaunchBrowser(ConfigurationManager.AppSettings["EdgeDriverPath"] ?? "");
                    await RunSysproSalesOrderCreation(manifestAppService, webDriver);
                }
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
            if (webDriver != null)
            {
                QuitAndCloseAllWebdriverInstances(webDriver);
            }
        }
    }

    async Task RunSysproApiOrderCreation(ManifestAppService manifestAppService)
    {
        try
        {
            Console.WriteLine("Starting Syspro API-based order creation...");
            
            // Setup dependency injection for API services
            var services = new ServiceCollection();
            services.AddHttpClient<ISysproApiService, SysproApiService>();
            services.AddLogging(builder => builder.AddConsole());
            var serviceProvider = services.BuildServiceProvider();
            
            var sysproApiService = serviceProvider.GetRequiredService<ISysproApiService>();

            // Validate configuration first
            if (!await SysproApiHelpers.ValidateConfigurationAsync(sysproApiService))
            {
                Console.WriteLine("Syspro API configuration validation failed. Please check your settings.");
                throw new Exception("Syspro API configuration is invalid");
            }

            // Process orders using API
            while (await manifestAppService.HasPendingManifestsToCreate())
            {
                await SysproApiHelpers.CreateSysproOrdersAsync(sysproApiService);
            }
            
            Console.WriteLine("All manifests processed successfully via API. Exiting application.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in API order creation: {ex.Message}");
            throw;
        }
    }

    async Task RunSysproSalesOrderCreation(ManifestAppService manifestAppService, IWebDriver webDriver)
    {
        Console.WriteLine("Starting Syspro Selenium-based order creation (legacy mode)...");
        while (await manifestAppService.HasPendingManifestsToCreate())
        {
            await SysproHelpers.CreateSysproOrders(webDriver);
            QuitAndCloseAllWebdriverInstances(webDriver);
            webDriver = GeneralHelpers.LaunchBrowser(ConfigurationManager.AppSettings["EdgeDriverPath"] ?? "");
        }
        Console.WriteLine("All manifests processed. Exiting application.");
    }

    void RestartScraper(string[] args, IWebDriver? webDriver, int exitCode = 0)
    {
        retries++;
        if (webDriver != null)
        {
            QuitAndCloseAllWebdriverInstances(webDriver);
        }
        //await RunMain(args);
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