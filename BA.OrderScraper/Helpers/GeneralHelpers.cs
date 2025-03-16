using OpenQA.Selenium.Edge;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using BA.OrderScraper.EFCore;
using Microsoft.EntityFrameworkCore;

namespace BA.OrderScraper.Helpers
{
    public static class GeneralHelpers
    {
        public static async Task UpdateDatabaseAsync()
        {
            using (var dbContext = new BADbContext())
            {
                //check if migrations are needed and apply them
                if ((await dbContext.Database.GetPendingMigrationsAsync()).Any())
                {
                    await dbContext.Database.MigrateAsync();
                }
            }
        }

        public static IWebDriver LaunchBrowser(string driverPath)
        {
            Console.WriteLine(driverPath);
            if (string.IsNullOrEmpty(driverPath))
            {
                throw new ArgumentNullException(nameof(driverPath));
            }
            var options = new EdgeOptions();
            options.AddArgument("--start-maximized");
            //options.AddArgument("--headless");
            //options.AddArgument("--disable-notifications");
            //options.AddArgument("--disable-gpu");
            options.AddArgument("inprivate");
            var driver = new EdgeDriver(
                EdgeDriverService
                .CreateDefaultService(driverPath, "msedgedriver.exe"),
                options,
                TimeSpan.FromMinutes(30));
            return driver;
        }

        public static void MoveFilesToArchive(IEnumerable<string> files, string archivePath)
        {
            if (!Directory.Exists(archivePath))
            {
                Directory.CreateDirectory(archivePath);
            }
            foreach (var file in files)
            {
                var archivedFileName = $"{Path.GetFileNameWithoutExtension(file)}_{DateTime.Now.ToString("yyyyMMddHHmmss")}{Path.GetExtension(file)}";
                File.Move(file, Path.Combine(archivePath, archivedFileName));
            }
        }
        public static void CleanArchiveHistory(string archivePath)
        {
            if (!Directory.Exists(archivePath))
            {
                return;
            }
            var archiveFiles = Directory.GetFiles(archivePath);
            foreach (var file in archiveFiles)
            {
                if (File.GetCreationTime(file) < DateTime.Now.AddDays(-(int.Parse(ConfigurationManager.AppSettings["ArchiveHistoryToKeepInDays"]))))
                {
                    File.Delete(file);
                }
            }
        }

        public static bool IsExcelFile(string filePath)
        {
            byte[] xlsSignature = { 0xD0, 0xCF, 0x11, 0xE0 };
            byte[] xlsxSignature = { 0x50, 0x4B, 0x03, 0x04 };

            using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                byte[] buffer = new byte[4];
                fs.Read(buffer, 0, 4);

                return buffer.Take(4).SequenceEqual(xlsSignature) || buffer.Take(4).SequenceEqual(xlsxSignature);
            }
        }
    }
}
