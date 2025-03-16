using BA.OrderScraper.EFCore;
using BA.OrderScraper.Models;
using BA.OrderScraper.Shared;
using OfficeOpenXml;
using OpenQA.Selenium;
using System.Configuration;
using System.Data.Entity;
using System.Globalization;

namespace BA.OrderScraper.Helpers
{
    public static class ToyotaPortalHelpers
    {
        public static async Task ImportToyotaOrders(string importType, IWebDriver? webDriver)
        {
            webDriver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(60);
            webDriver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(120);

            LoginToyotaEPortal(webDriver);
            DownloadTodaysOrders(webDriver, importType);

            //wait for the files to be downloaded
            Thread.Sleep(10000);
            await ProcessTodaysOrders(ConfigurationManager.AppSettings["DownloadsPath"] ?? "");
        }
        public static void LoginToyotaEPortal(IWebDriver webDriver)
        {
            webDriver.Navigate().GoToUrl(ConfigurationManager.AppSettings["ToyotaPortalUrl"]);
            var userNameInput = webDriver.FindElement(By.Id("Username"));
            userNameInput.SendKeys(ConfigurationManager.AppSettings["ToyotaPortalLoginUsername"]);
            var passwordInput = webDriver.FindElement(By.Id("Password"));
            passwordInput.SendKeys(ConfigurationManager.AppSettings["ToyotaPortalLoginPassword"]);
            //click submit button
            webDriver.FindElement(By.CssSelector("button[type='submit']")).Click();
        }
        public static void DownloadTodaysOrders(IWebDriver webDriver, string importType)
        {
            IJavaScriptExecutor js = (IJavaScriptExecutor)webDriver;
            //body > app-root > div > div > navigation > nav > button:nth-child(4)
            webDriver.FindElement(By.CssSelector("body > app-root > div > div > navigation > nav > button:nth-child(4)")).Click();
            //#mat-menu-panel-1 > div > button:nth-child(1)

            //#mat-menu-panel-1 > div > button:nth-child(1)
            //click in javascript
            js.ExecuteScript("document.querySelector('#mat-menu-panel-1 > div > button:nth-child(1)').click();");


            //body > app-root > div > div > ng-component > div > mat-drawer-container > mat-drawer > div > module-navigation > ul > li:nth-child(2) > button
            webDriver.FindElement(By.CssSelector("body > app-root > div > div > ng-component > div > mat-drawer-container > mat-drawer > div > module-navigation > ul > li:nth-child(2) > button")).Click();

            var todayYYYYMMDD = DateTime.Now.ToString("yyyy MM dd").Replace(" ", "");
            //var todayYYYYMMDD = DateTime.Now.AddDays(1).ToString("yyyy MM dd").Replace(" ", "");
            var todayForJs = DateTime.Now.ToString("dd/MM/yyyy");
            //var todayForJs = DateTime.Now.AddDays(1).ToString("dd/MM/yyyy");

            //$('#mat-input-7').value = '02/06/2024'
            js.ExecuteScript($"document.querySelector('#mat-input-7').value = '{todayForJs}'; document.querySelector('#mat-input-7').dispatchEvent(new Event('change'));");
            js.ExecuteScript($"document.querySelector('#mat-input-8').value = '{todayForJs}'; document.querySelector('#mat-input-7').dispatchEvent(new Event('change'));");

            webDriver.FindElement(By.CssSelector("body > app-root > div > div > ng-component > div > mat-drawer-container > mat-drawer > div > div > module-search-container > div > div.module-search-container__body.module-search-container__full-height > app-ekanban-reprint-search-form-container > ekanban-reprint-search-form > form > button")).Click();

            //body > app-root > div > div > ng-component > div > mat-drawer-container > mat-drawer-content > ng-component > kanban-print-container > page-content-container > div > div.page-content__container--body > div > div.flex__fill--remainder > ekanban-orders-container > ekanban-orders > table > tbody > tr:nth-child(1)
            webDriver.FindElement(By.CssSelector("body > app-root > div > div > ng-component > div > mat-drawer-container > mat-drawer-content > ng-component > kanban-print-container > page-content-container > div > div.page-content__container--body > div > div.flex__fill--remainder > ekanban-orders-container > ekanban-orders > table > tbody > tr:nth-child(1)"));
            var tbody = webDriver.FindElement(By.CssSelector("body > app-root > div > div > ng-component > div > mat-drawer-container > mat-drawer-content > ng-component > kanban-print-container > page-content-container > div > div.page-content__container--body > div > div.flex__fill--remainder > ekanban-orders-container > ekanban-orders > table > tbody"));

            var trs = tbody.FindElements(By.TagName("tr"));
            bool todayStarted = false;
            foreach (var tr in trs)
            {
                //scroll the row into view
                js.ExecuteScript("arguments[0].scrollIntoView(true);", tr);

                var rowDepartDate = tr.FindElement(By.CssSelector(".mat-column-departDate"));
                var rowDepartDateValue = rowDepartDate.Text;
                if (rowDepartDateValue == todayYYYYMMDD)
                {
                    switch (importType)
                    {
                        case Consts.ImportType.Manifest:
                            //find and check the first checkbox in the row
                            var eigthTd = tr.FindElement(By.CssSelector("td:nth-child(8)"));
                            js.ExecuteScript(@"arguments[0].querySelector('input[type=""checkbox""]').click();", eigthTd);
                            break;
                        case Consts.ImportType.Kanban:
                            //find and check the first checkbox in the row
                            var ninthTd = tr.FindElement(By.CssSelector("td:nth-child(9)"));
                            js.ExecuteScript(@"arguments[0].querySelector('input[type=""checkbox""]').click();", ninthTd);
                            break;
                        case Consts.ImportType.Skid:
                            //find and check the first checkbox in the row
                            var tenthTd = tr.FindElement(By.CssSelector("td:nth-child(10)"));
                            js.ExecuteScript(@"arguments[0].querySelector('input[type=""checkbox""]').click();", tenthTd);
                            break;
                        default: break;
                    }
                }
            }

            //find the mat-label element with the text 'Reprint Reason' via javascript
            js.ExecuteScript(@$"document.querySelector('body > app-root > div > div > ng-component > div > mat-drawer-container > mat-drawer-content > ng-component > kanban-print-container > page-content-container > div > div.page-content__container--body > div > div.row > mat-form-field > div').children[0].click()");

            //find all span with the class mat-option-text
            var spans = webDriver.FindElements(By.CssSelector("span.mat-option-text"));
            //find the one where the trimmed text is '01 - Printer Problem'
            var span = spans.FirstOrDefault(s => s.Text.Trim() == "01 - Printer Problem");
            //click the span
            span?.Click();


            webDriver.FindElement(By.CssSelector("body > app-root > div > div > ng-component > div > mat-drawer-container > mat-drawer-content > ng-component > kanban-print-container > page-content-container > div > div.page-content__container--body > div > ekanban-orders-footer > page-content-footer-container > div > button:nth-child(1)"));
            js.ExecuteScript("document.querySelector('body > app-root > div > div > ng-component > div > mat-drawer-container > mat-drawer-content > ng-component > kanban-print-container > page-content-container > div > div.page-content__container--body > div > ekanban-orders-footer > page-content-footer-container > div > button:nth-child(1)').click();");
            webDriver.FindElement(By.CssSelector("#mat-dialog-0 > app-message-dialog > div > div:nth-child(2) > div > button"));
            //js.ExecuteScript("document.querySelector('#mat-dialog-0 > app-message-dialog > div > div:nth-child(2) > div > button').click()");
        }
        public static async Task ProcessTodaysOrders(string downloadsPath)
        {
            var files = Directory.GetFiles(downloadsPath)
                .OrderByDescending(f => new FileInfo(f).CreationTime)
                .Where(z => z.ToLower().Contains("kanban") || z.ToLower().Contains("manifest") || z.ToLower().Contains("skid"))
                .Where(z => GeneralHelpers.IsExcelFile(Path.GetFullPath(z)));

            if (files == null)
            {
                Console.WriteLine("No files found");
                return;
            }
            foreach (var file in files)
            {
                var fileName = Path.GetFileName(file);
                if (fileName.ToLower().Contains("kanban"))
                {
                    //extract xlsx file into kanban class
                    List<KanbanCard> kanbanCards = new List<KanbanCard>();
                    //open excel file and iterate through rows
                    using (var package = new ExcelPackage(new FileInfo(file)))
                    {
                        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                        var worksheet = package.Workbook.Worksheets[0];
                        for (int i = 2; i <= worksheet.Dimension.End.Row; i++)
                        {
                            var kanbanCard = new KanbanCard
                            {
                                SupplierName = worksheet.Cells[i, 1].Value?.ToString() ?? "",
                                SupplierNumber = worksheet.Cells[i, 2].Value?.ToString() ?? "",
                                SupplierPartNumber = worksheet.Cells[i, 3].Value?.ToString() ?? "",
                                ArrivalDate = DateTime.ParseExact(worksheet.Cells[i, 4].Value?.ToString() ?? "", "yyyyMMddHHmm", CultureInfo.InvariantCulture),
                                Shop = worksheet.Cells[i, 5].Value?.ToString() ?? "",
                                PartName = worksheet.Cells[i, 6].Value?.ToString() ?? "",
                                MaterialNumber = worksheet.Cells[i, 7].Value?.ToString() ?? "",
                                KanbanNo = worksheet.Cells[i, 8].Value?.ToString() ?? "",
                                OrderNumber = worksheet.Cells[i, 9].Value?.ToString() ?? "",
                                BinTypeCode = worksheet.Cells[i, 10].Value?.ToString() ?? "",
                                Qty = int.Parse(worksheet.Cells[i, 11].Value?.ToString() ?? "0"),
                                KanbanPrintAddress1 = worksheet.Cells[i, 12].Value?.ToString() ?? "",
                                ReceivingDock = worksheet.Cells[i, 13].Value?.ToString() ?? "",
                                ProgressLane = worksheet.Cells[i, 14].Value?.ToString() ?? "",
                                KanbanID = worksheet.Cells[i, 16].Value?.ToString() ?? "",
                                OnDockRoute = worksheet.Cells[i, 17].Value?.ToString() ?? "",
                                ManifestNumber = worksheet.Cells[i, 18].Value?.ToString() ?? ""
                            };
                            kanbanCards.Add(kanbanCard);
                        }
                    }
                    //add the KanbanCards to the database 
                    var dbContext = new BADbContext();
                    await dbContext.StagingKanbanCard.AddRangeAsync(kanbanCards);
                    await dbContext.SaveChangesAsync();
                }
                else if (fileName.ToLower().Contains("manifest"))
                {
                    //extract xlsx file into manifest class
                    List<Manifest> manifests = new List<Manifest>();
                    //open excel file and iterate through rows
                    using (var package = new ExcelPackage(new FileInfo(file)))
                    {
                        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                        var worksheet = package.Workbook.Worksheets[0];
                        for (int i = 2; i <= worksheet.Dimension.End.Row; i++)
                        {
                            var shipTimeString = worksheet.Cells[i, 8].Value?.ToString() ?? "00:00";
                            int hours = int.Parse(shipTimeString.Substring(0, 2));
                            int minutes = int.Parse(shipTimeString.Substring(3, 2));

                            var manifest = new Manifest
                            {
                                SupplierName = worksheet.Cells[i, 1].Value?.ToString() ?? "",
                                SupplierAddress = worksheet.Cells[i, 2].Value?.ToString() ?? "",
                                City = worksheet.Cells[i, 3].Value?.ToString() ?? "",
                                Region = worksheet.Cells[i, 4].Value?.ToString() ?? "",
                                PostCode = worksheet.Cells[i, 5].Value?.ToString() ?? "",
                                SupplierNumber = worksheet.Cells[i, 6].Value?.ToString() ?? "",

                                ShipDate = DateTime.ParseExact(worksheet.Cells[i, 7].Value?.ToString() ?? "1970/01/01", "yyyy/MM/dd", CultureInfo.InvariantCulture),
                                ShipTime = new TimeSpan(hours, minutes, 0),
                                PickupRoute = worksheet.Cells[i, 9].Value?.ToString() ?? "",
                                SupplierRoute = worksheet.Cells[i, 10].Value?.ToString() ?? "",
                                SupplierArriveDock = worksheet.Cells[i, 11].Value?.ToString() ?? "",
                                SupplierDepartDock = worksheet.Cells[i, 12].Value?.ToString() ?? "",

                                JHB_PE = worksheet.Cells[i, 13].Value?.ToString() ?? "",
                                SupplierArriveDocJHBPE = worksheet.Cells[i, 14].Value?.ToString() ?? "",
                                SupplierDepartDockJHBPE = worksheet.Cells[i, 15].Value?.ToString() ?? "",
                                SupplierOnDockRoute = worksheet.Cells[i, 16].Value?.ToString() ?? "",
                                SupplierArrivalDateParsed = DateTime.ParseExact(worksheet.Cells[i, 17].Value?.ToString() ?? "19700101", "yyyyMMdd", CultureInfo.InvariantCulture),
                                SupplierArrivalTime = TimeSpan.ParseExact(worksheet.Cells[i, 18].Value?.ToString() ?? "0000", "hhmm", null),

                                SupplierProglane = worksheet.Cells[i, 19].Value?.ToString() ?? "",
                                SupplierShop = "",//worksheet.Cells[i, 20].Value?.ToString() ?? "",
                                SupplierManifestNo = int.Parse(worksheet.Cells[i, 20].Value?.ToString() ?? "-"),
                                SupplierReceivingDock = (string)(worksheet.Cells[i, 21].Value?.ToString() ?? "") + "-" + (string)(worksheet.Cells[i, 22].Value?.ToString() ?? ""),
                                SupplierOrderNumber = worksheet.Cells[i, 23].Value?.ToString() ?? "",
                                SupplierKanbanNumber = worksheet.Cells[i, 24].Value?.ToString() ?? "",

                                SupplierMaterialNumber = worksheet.Cells[i, 29].Value?.ToString() ?? "",
                                SupplierPadEasyReferenceNumber = worksheet.Cells[i, 25].Value?.ToString() ?? "",
                                SupplierPadBinTypeCode = worksheet.Cells[i, 26].Value?.ToString() ?? "",
                                SupplierQty = long.Parse(worksheet.Cells[i, 27].Value?.ToString() ?? "0"),
                                SupplierPurchasingDocumentNumber = worksheet.Cells[i, 28].Value?.ToString() ?? "",
                                SupplierBinReq = worksheet.Cells[i, 30].Value?.ToString() ?? "",

                            };
                            manifests.Add(manifest);
                        }
                    }
                    var dbContext = new BADbContext();
                    await dbContext.StagingManifest.AddRangeAsync(manifests);
                    await dbContext.SaveChangesAsync();

                }
                else if (fileName.ToLower().Contains("skid"))
                {
                    //extract xlsx file into skid class
                    List<Skid> skids = new List<Skid>();
                    //open excel file and iterate through rows
                    using (var package = new ExcelPackage(new FileInfo(file)))
                    {
                        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                        var worksheet = package.Workbook.Worksheets[0];
                        for (int i = 2; i <= worksheet.Dimension.End.Row; i++)
                        {
                            var skid = new Skid
                            {
                                SupplierCode = worksheet.Cells[i, 1].Value?.ToString() ?? "",
                                SupplierName = worksheet.Cells[i, 2].Value?.ToString() ?? "",
                                ReceivingDock = worksheet.Cells[i, 3].Value?.ToString() ?? "",
                                ProgressLane = worksheet.Cells[i, 4].Value?.ToString() ?? "",
                                OrderNumber = worksheet.Cells[i, 5].Value?.ToString() ?? "",
                                PickupRoute = worksheet.Cells[i, 6].Value?.ToString() ?? "",
                                DepartDate = DateTime.ParseExact(worksheet.Cells[i, 7].Value?.ToString() ?? "19700101", "yyyyMMdd", CultureInfo.InvariantCulture),
                                Route = worksheet.Cells[i, 8].Value?.ToString() ?? "",
                                XDoc2ArrivalDate = DateTime.ParseExact(worksheet.Cells[i, 9].Value?.ToString() ?? "19700101", "yyyyMMdd", CultureInfo.InvariantCulture),
                                XDoc2ArrivalTime = TimeSpan.ParseExact(worksheet.Cells[i, 10].Value?.ToString() ?? "0000", "hhmm", null),
                                DepartDate2 = DateTime.ParseExact(worksheet.Cells[i, 11].Value?.ToString() ?? "19700101", "yyyyMMdd", CultureInfo.InvariantCulture),
                                DepartDate3 = DateTime.ParseExact(worksheet.Cells[i, 12].Value?.ToString() ?? "19700101", "yyyyMMdd", CultureInfo.InvariantCulture),
                                DepartTime = TimeSpan.ParseExact(worksheet.Cells[i, 13].Value?.ToString() ?? "0000", "hhmm", null),
                                OnDockRoute = worksheet.Cells[i, 14].Value?.ToString() ?? "",
                                ArrivalDate = DateTime.ParseExact(worksheet.Cells[i, 15].Value?.ToString() ?? "19700101", "yyyyMMdd", CultureInfo.InvariantCulture),
                                ArrivalTime = TimeSpan.ParseExact(worksheet.Cells[i, 16].Value?.ToString() ?? "0000", "hhmm", null),
                                TsamDepartDocDate = DateTime.ParseExact(worksheet.Cells[i, 17].Value?.ToString() ?? "19700101", "yyyyMMdd", CultureInfo.InvariantCulture),
                                TsamDepartDocTime = TimeSpan.ParseExact(worksheet.Cells[i, 18].Value?.ToString() ?? "0000", "hhmm", null),
                                ManifestNumber = worksheet.Cells[i, 19].Value?.ToString() ?? ""
                            };
                            skids.Add(skid);
                        }
                    }
                    var dbContext = new BADbContext();
                    await dbContext.StagingSkid.AddRangeAsync(skids);
                    await dbContext.SaveChangesAsync();
                }
            }

            var archivePath = Path.GetFullPath(ConfigurationManager.AppSettings["EdgeDriverPath"]) + "\\ToyotaArchive";
            GeneralHelpers.MoveFilesToArchive(files, archivePath);
            GeneralHelpers.CleanArchiveHistory(archivePath);
        }
    }
}
