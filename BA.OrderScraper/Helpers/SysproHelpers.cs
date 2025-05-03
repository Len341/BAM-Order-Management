using BA.OrderScraper.EFCore;
using BA.OrderScraper.Models;
using BA.OrderScraper.Models.DTO;
using BA.OrderScraper.Services;
using BA.OrderScraper.Shared;
using LinqKit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Update.Internal;
using OfficeOpenXml.Drawing.Theme;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

namespace BA.OrderScraper.Helpers
{
    public static class SysproHelpers
    {
        static SysproOrderCreationHistoryAppService sysproOrderCreationHistoryAppService = new SysproOrderCreationHistoryAppService();
        static ManifestAppService manifestAppService = new ManifestAppService();
        public static async Task CreateSysproOrders(IWebDriver? webDriver)
        {
            var currentOrder = new SysproOrderItem();
            SysproOrderCreationHistory orderInProgress = null;
            try
            {
                int contractExpiredPopupCount = 0;

                webDriver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(30);
                webDriver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(120);
                WebDriverWait wait = new WebDriverWait(webDriver, TimeSpan.FromSeconds(30));
                var jsExecutor = (IJavaScriptExecutor)webDriver;

                await LoginSysproAvantiPortal(webDriver);
                jsExecutor.ExecuteScript(Consts.JavaScript.baseScript);
                await Task.Delay(5000);
                await NavigateToExpressOrderEntry(wait, webDriver);
                await Task.Delay(4000);

                List<SysproOrderItem> orderItems = new List<SysproOrderItem>();
                IWebElement? orderNumberInput;
                IWebElement row;
                ReadOnlyCollection<IWebElement> rowGroup = new ReadOnlyCollection<IWebElement>(new List<IWebElement>());

                //check for in progress orders
                var inProgressOrders = (await sysproOrderCreationHistoryAppService
                    .GetSysproOrderCreationHistoryAsync(inProgress: true))
                    .Take(5);
                if (inProgressOrders != null && inProgressOrders.Any())
                {
                    foreach (var inProgressOrder in inProgressOrders)
                    {
                        var order = await manifestAppService.GetSysproOrderByManifestNo(inProgressOrder.ManifestNumber, true);
                        order.OrderNumber = inProgressOrder.OrderNumber ?? "";
                        orderItems.Add(order);
                    }
                }
                orderItems.AddRange(await manifestAppService.GetTopNManifestsToCreate(5));

                foreach (var sysproOrder in orderItems)
                {
                    if (sysproOrder.Items.Count == 0) continue;
                    Console.WriteLine($"Processing manifest: {sysproOrder.CustomerPurchaseOrder}");
                    currentOrder = sysproOrder;
                    var windowHandles = webDriver.WindowHandles;
                    webDriver.SwitchTo().Window(windowHandles.LastOrDefault());

                    orderInProgress = new SysproOrderCreationHistory();
                    string orderNumber = "";
                    int startingItemRow = 0;
                    if (sysproOrder.InProgress)
                    {
                        //we have an order number already, we should search for it and continue
                        //ID = Toolbar.SORPOETB00009, click 'maintain order'
                        webDriver.FindElement(By.Id("Toolbar.SORPOETB00009")).Click();
                        await Task.Delay(500);
                        Console.WriteLine($"Continuing with order: {sysproOrder.OrderNumber}");
                        webDriver.FindElement(By.Id("Toolbar.SORPOETB38002"));
                        jsExecutor.ExecuteScript($"document.getElementById('Toolbar.SORPOETB38002').value = '{sysproOrder.OrderNumber}'");
                        //await Task.Delay(1500);
                        jsExecutor.ExecuteScript("document.getElementById('Toolbar.SORPOETB38002').dispatchEvent(new Event('change'))");
                        await Task.Delay(3000);

                        try
                        {
                            bool orderIsBeingMaintainedVisible = (bool)jsExecutor.ExecuteScript(
                                "return document.querySelector('body > div.k-widget.k-window.k-dialog > div.k-dialog-buttongroup.k-dialog-button-layout-stretched > button:nth-child(1)').checkVisibility()");
                            if (orderIsBeingMaintainedVisible)
                            {
                                //close the modal
                                webDriver.FindElement(By.CssSelector("body > div.k-widget.k-window.k-dialog > div.k-dialog-buttongroup.k-dialog-button-layout-stretched > button:nth-child(1)")).Click();
                            }
                            await Task.Delay(2000);
                            bool resetOrderIsVisible = (bool)jsExecutor.ExecuteScript(
                               "return document.querySelector('body > div:nth-child(57) > div.k-dialog-buttongroup.k-dialog-button-layout-stretched > button.k-button.k-primary').checkVisibility()");
                            if (resetOrderIsVisible)
                            {
                                //close the modal
                                webDriver.FindElement(By.CssSelector("body > div:nth-child(57) > div.k-dialog-buttongroup.k-dialog-button-layout-stretched > button.k-button.k-primary")).Click();
                            }
                        }
                        catch (Exception anyException)
                        {
                            //throw;
                            //continue with work
                        }

                        orderInProgress = await sysproOrderCreationHistoryAppService
                            .GetSysproOrderCreationHistoryByManifestNumberAsync(sysproOrder.CustomerPurchaseOrder);

                        await Task.Delay(500);
                        int retryCount = 0;
                        while (retryCount < 3)
                        {
                            try
                            {
                                webDriver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5);
                                rowGroup = webDriver
                                    .FindElement(By.XPath("//*[@id=\"main_column_0\"]/div[2]/div/div/div/div[2]/div/div[3]/div[1]/table/tbody"))
                                    .FindElements(By.TagName("tr"));
                                break;
                            }
                            catch (Exception ex)
                            {
                                retryCount++;
                            }
                            finally
                            {
                                webDriver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(30);
                            }
                        }

                        startingItemRow = rowGroup?.Count ?? 0;
                    }
                    else
                    {
                        webDriver.FindElement(By.Id("Toolbar.SORPOETB38000"));
                        jsExecutor.ExecuteScript("document.getElementById('Toolbar.SORPOETB38000').value = 'TOY020'");
                        //await Task.Delay(1500);
                        jsExecutor.ExecuteScript("document.getElementById('Toolbar.SORPOETB38000').dispatchEvent(new Event('change'))");
                        //webDriver.FindElement(By.CssSelector("#main_column_0 > div:nth-child(2) > div > h4 > i")).Click();
                        await Task.Delay(3000);

                        //await ClickElement(webDriver, By.Id("Fields.TOOLBAR:SORPOETB39000"));
                        jsExecutor.ExecuteScript("document.getElementById('Fields.TOOLBAR:SORPOETB39000').click()");

                        await Task.Delay(2500);
                        orderNumberInput = webDriver.FindElement(By.Id("Toolbar.SORPOETB38002"));
                        orderNumber = orderNumberInput.GetAttribute("value");
                        int orderNumberRetryCount = 0;
                        while (string.IsNullOrEmpty(orderNumber))
                        {
                            if (orderNumberRetryCount > 5)
                            {
                                throw new Exception("Could not get order number");
                            }
                            //we should get the order number from the order number input
                            jsExecutor.ExecuteScript("document.getElementById('Fields.TOOLBAR:SORPOETB39000').click()");
                            await Task.Delay(3000);
                            orderNumber = jsExecutor.ExecuteScript("return document.getElementById('Toolbar.SORPOETB38002').value").ToString();
                            orderNumberRetryCount++;
                        }

                        orderInProgress = await sysproOrderCreationHistoryAppService
                            .CreateOrUpdateSysproOrderCreationHistoryAsync(new SysproOrderCreationHistory()
                            {
                                ManifestNumber = sysproOrder.CustomerPurchaseOrder,
                                InProgress = true,
                                UpdatedDate = DateTime.Now,
                                OrderNumber = orderNumber
                            });
                    }

                    #region Create Sales Order



                    //ensure we scroll to top of table
                    //jsExecutor.ExecuteScript("document.querySelector('.k-scrollbar.k-scrollbar-vertical').scrollTo(0,0)");

                    #region Order Items Creation
                    int scrollDown = 28;
                    var loopCount = startingItemRow;
                    //after each item, move the scrollbar down by 28 pixels (this is the height of each row item)
                    for (var i = startingItemRow; i < sysproOrder.Items.Count; i++)
                    {
                        Actions actions = new Actions(webDriver);
                        wait.Timeout = TimeSpan.FromSeconds(20);
                        jsExecutor.ExecuteScript("document.getElementById('gridtoolbar.newrow').click()");

                        jsExecutor.ExecuteScript(@"if(document.querySelector('.material-icons.close-error-notification') != null){
    document.querySelector('.material-icons.close-error-notification').click();}");

                        WaitForNewRowToBeAdded(wait, jsExecutor, i);
                        await Task.Delay(1500);
                        actions.SendKeys(Keys.Tab).Perform();
                        UpdateItemsRows(webDriver, i, out row, out rowGroup);

                        IWebElement td5;

                        while (true)
                        {
                            int retryCount = 0;
                            try
                            {
                                td5 = row.FindElements(By.TagName("td"))[4];
                                break;
                            }
                            catch (Exception ex) when (ex is NoSuchElementException || ex is StaleElementReferenceException)
                            {
                                await Task.Delay(500);
                                UpdateItemsRows(webDriver, i, out row, out rowGroup);
                                retryCount++;
                                if (retryCount > 6)
                                {
                                    throw new Exception($"Could not find the fifth td element for row {i}");
                                }
                            }
                            catch
                            {
                                throw;
                            }
                        }

                        IWebElement warehouseInput = null;

                        var td5StaleRetryCount = 0;
                        while (td5StaleRetryCount < 10)
                        {
                            webDriver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(1);
                            try { warehouseInput = td5.FindElement(By.TagName("input")); break; }
                            catch (StaleElementReferenceException)
                            {
                                UpdateItemsRows(webDriver, i, out row, out rowGroup);
                                td5 = row.FindElements(By.TagName("td"))[4];
                                td5StaleRetryCount++;
                            }
                            catch (NotFoundException)
                            {
                                ClickTd(webDriver, i, 4);
                                UpdateItemsRows(webDriver, i, out row, out rowGroup);
                                td5 = row.FindElements(By.TagName("td"))[4];
                                td5StaleRetryCount++;
                            }
                            catch
                            {
                                throw;
                            }
                        }
                        webDriver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(30);

                        var warehouseText = warehouseInput.GetAttribute("value");
                        while (warehouseText.Trim() == string.Empty)
                        {
                            try
                            {
                                warehouseInput.SendKeys("10");//use warehouse 10 as a constant for now: TODO
                                await Task.Delay(200);
                                warehouseText = warehouseInput.GetAttribute("value");
                            }
                            catch (Exception)
                            {
                                UpdateItemsRows(webDriver, i, out row, out rowGroup);
                                td5 = row.FindElements(By.TagName("td"))[4];
                                ClickTd(webDriver, i, 4);
                                warehouseInput = td5.FindElement(By.TagName("input"));
                                warehouseText = warehouseInput.GetAttribute("value"); //stale element reference exception here ?
                            }
                        }

                        UpdateItemsRows(webDriver, i, out row, out rowGroup);
                        var td6 = row.FindElements(By.TagName("td"))[5];
                        //actions.SendKeys(Keys.Tab).Perform();
                        //await Task.Delay(1500);

                        ClickTd(webDriver, i, 5);
                        await Task.Delay(loopCount == 0 ? 6000 : 1500);
                        //WaitUntilElementStale(wait, td6, 5);
                        UpdateItemsRows(webDriver, i, out row, out rowGroup);
                        if (loopCount == 0) ClickTd(webDriver, i, 5);
                        while (true)
                        {
                            try
                            {
                                webDriver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(2);
                                td6 = row.FindElements(By.TagName("td"))[5];
                                td6.FindElement(By.TagName("a")).Click();
                                break;
                            }
                            catch (StaleElementReferenceException)
                            {
                                await Task.Delay(500);
                                UpdateItemsRows(webDriver, i, out row, out rowGroup);
                            }
                            catch (NoSuchElementException)
                            {
                                ClickTd(webDriver, i, 5);
                                await Task.Delay(1000);
                                UpdateItemsRows(webDriver, i, out row, out rowGroup);
                            }
                            catch
                            {
                                throw;
                            };
                            webDriver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(30);
                        }
                        //webDriver.FindElement(By.Id("Filter_AlternateKey1_InvMaster_0"));
                        WaitUntilElementVisible(wait, By.Id("Filter_AlternateKey1_InvMaster_0"), 5);
                        await Task.Delay(500);
                        jsExecutor.ExecuteScript($"document.getElementById('Filter_AlternateKey1_InvMaster_0').value = '{sysproOrder.Items[i].QuickReference}'");
                        await Task.Delay(250);
                        jsExecutor.ExecuteScript(Consts.JavaScript.baseScript + " triggerElementChange(document.querySelector('#Filter_AlternateKey1_InvMaster_0'))");

                        await Task.Delay(500);

                        jsExecutor.ExecuteScript("document.querySelector('.avanti-search-button').click();");

                        var checkTableRowsEndTime = DateTime.Now.AddSeconds(6);
                        bool isEmpty = false;
                        while (DateTime.Now < checkTableRowsEndTime)
                        {
                            try
                            {
                                isEmpty = (bool)jsExecutor.ExecuteScript("return document.querySelector('#example .k-selectable tbody').children.length == 0");
                                if (!isEmpty) break;
                                await Task.Delay(500);
                            }
                            catch (Exception ex)
                            {
                                //do nothing
                            }
                        }

                        if (isEmpty)
                        {
                            //item could not be found, what do we do in this case ?
                            //close the modal and continue, for now
                            //body > div.k-widget.k-window.compact-window > div.k-window-titlebar.k-header > div > a:nth-child(3)
                            jsExecutor.ExecuteScript("document.querySelector('body > div.k-widget.k-window.compact-window > div.k-window-titlebar.k-header > div > a:nth-child(3)').click()");

                            UpdateItemsRows(webDriver, i, out row, out rowGroup);
                            //find input in fifth td
                            row.FindElements(By.TagName("td"))[0].FindElement(By.TagName("a")).Click();
                            var manifestItemsNotFound = await manifestAppService.GetManifestListAsync(sysproOrder.CustomerPurchaseOrder, sysproOrder.Items[i].QuickReference);

                            foreach (var manifest in manifestItemsNotFound)
                            {
                                manifest.SupplierPadEasyReferenceNumberNotFound = true;
                                await manifestAppService.UpdateManifestAsync(manifest);
                            }

                            sysproOrder.Items.RemoveAt(i);
                            i--;

                            await Task.Delay(1500);
                        }
                        else
                        {
                            var tbody = webDriver.FindElement(By.Id("example"))
                                .FindElement(By.ClassName("k-selectable"))
                                .FindElement(By.TagName("tbody"));

                            var rows = tbody.FindElements(By.TagName("tr"));


                            string warehouse = string.Empty;
                            string itemDescription = string.Empty;

                            int retryItemDetailsCount = 0;
                            while (true)
                            {
                                try
                                {
                                    warehouse = rows[0].FindElements(By.TagName("td"))[1].GetAttribute("innerText");
                                    itemDescription = rows[0].FindElements(By.TagName("td"))[3].GetAttribute("innerText").ToLower().Trim().Replace(" ", "");
                                    break;
                                }
                                catch (Exception ex) when (ex is StaleElementReferenceException)
                                {
                                    await Task.Delay(500);
                                    tbody = webDriver.FindElement(By.Id("example"))
                                        .FindElement(By.ClassName("k-selectable"))
                                        .FindElement(By.TagName("tbody"));
                                    rows = tbody.FindElements(By.TagName("tr"));
                                    retryItemDetailsCount++;
                                    if (retryItemDetailsCount > 10)
                                    {
                                        throw;
                                    }
                                }
                            }

                            var clickableItemSearchResultRetryCount = 0;
                            while (true)
                            {

                                try
                                {
                                    wait.Timeout = TimeSpan.FromSeconds(10);
                                    wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(rows[0]));
                                    break;
                                }
                                catch (Exception ex)
                                {
                                    clickableItemSearchResultRetryCount++;
                                    if (clickableItemSearchResultRetryCount > 10)
                                    {
                                        throw new Exception("The search result item row did not become clickable");
                                    }
                                    await Task.Delay(500);
                                    tbody = webDriver.FindElement(By.Id("example"))
                                        .FindElement(By.ClassName("k-selectable"))
                                        .FindElement(By.TagName("tbody"));
                                    rows = tbody.FindElements(By.TagName("tr"));
                                }
                            }

                            var clickItemSearchResultRetryCount = 0;
                            while (true)
                            {
                                try
                                {
                                    rows[0].Click();
                                    await Task.Delay(1000);
                                    break;
                                }
                                catch (Exception ex) when (ex is StaleElementReferenceException || ex is ElementClickInterceptedException)
                                {
                                    await Task.Delay(500);
                                    tbody = webDriver.FindElement(By.Id("example"))
                                        .FindElement(By.ClassName("k-selectable"))
                                        .FindElement(By.TagName("tbody"));
                                    rows = tbody.FindElements(By.TagName("tr"));
                                    clickItemSearchResultRetryCount++;
                                }
                                if (clickItemSearchResultRetryCount > 10)
                                {
                                    throw new Exception("Could not click on search result item row");
                                }
                            }
                            actions.SendKeys(Keys.Tab).Perform();
                            //await Task.Delay(1000);
                            //actions.SendKeys(Keys.Tab).Perform();
                            UpdateItemsRows(webDriver, i, out row, out rowGroup);

                            int retryItemDescriptionCheckCount = 0;
                            while (true)
                            {
                                try
                                {
                                    webDriver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(1);
                                    var input = row.FindElements(By.TagName("td"))[6].FindElement(By.TagName("input"));
                                    var inputText = input.GetAttribute("value").ToLower().Trim().Replace(" ", "");
                                    if (inputText != string.Empty) break;

                                    if (retryItemDescriptionCheckCount > 10)
                                    {
                                        throw new Exception($"Item description could not be filled");
                                    }
                                }
                                catch (StaleElementReferenceException)
                                {
                                    await Task.Delay(500);
                                    UpdateItemsRows(webDriver, i, out row, out rowGroup);
                                    retryItemDescriptionCheckCount++;
                                }
                                catch (NotFoundException)
                                {
                                    ClickTd(webDriver, i, 6);
                                    await Task.Delay(500);
                                    retryItemDescriptionCheckCount++;
                                }
                                catch
                                {
                                    throw;
                                }
                            }

                            UpdateItemsRows(webDriver, i, out row, out rowGroup);
                            var td8 = row.FindElements(By.TagName("td"))[8];
                            ClickTd(webDriver, i, 8);
                            var qtyInput = td8.FindElements(By.TagName("input")).LastOrDefault();
                            jsExecutor.ExecuteScript($"arguments[0].value = '{sysproOrder.Items[i].Quantity}';", qtyInput);
                            await Task.Delay(500);
                            UpdateItemsRows(webDriver, i, out row, out rowGroup);
                            //ClickTd(webDriver, i, 10);
                            actions.SendKeys(Keys.Tab).Perform();
                            await Task.Delay(4000);
                            try
                            {
                                //var closeExpiredContractPopupButton
                                //await Task.Delay(2000);
                                webDriver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(1);
                                var closeExpiredContractPopupButtons = webDriver.FindElements(
                                    By.CssSelector("body > div.k-widget.k-window.k-dialog > div.k-window-titlebar.k-dialog-titlebar.k-header > div > a"));

                                foreach (var elem in closeExpiredContractPopupButtons)
                                {
                                    var isHidden = (bool)jsExecutor.ExecuteScript("return (arguments[0].offsetParent === null)", elem);
                                    if (!isHidden)
                                    {
                                        elem.Click();
                                        contractExpiredPopupCount++;
                                    }
                                }
                            }
                            catch (Exception)
                            {
                                //continue with work
                            }
                            await Task.Delay(500);

                            try
                            {
                                //check for the 'insufficient stock' popup, for now we will just close by clicking the X
                                //Toolbar.FORMSORPOEFE60000
                                webDriver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(1);
                                var saveInsufficientStockPopupButton = webDriver.FindElement(By.Id("Toolbar.FORMSORPOEFE41000"));
                                saveInsufficientStockPopupButton.Click();
                            }
                            catch (Exception)
                            {
                                //continue with work
                            }

                            webDriver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(30);

                            UpdateItemsRows(webDriver, i, out row, out rowGroup);
                        }
                        jsExecutor.ExecuteScript($"document.querySelector('.k-scrollbar.k-scrollbar-vertical').scrollTo(0,{scrollDown})");
                        scrollDown += 28;
                        loopCount++;
                    }

                    #endregion

                    #region Save Order

                    await FillOrderHeader(jsExecutor, sysproOrder);

                    try
                    {
                        webDriver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(4);
                        rowGroup = webDriver
                            .FindElement(By.XPath("//*[@id=\"main_column_0\"]/div[2]/div/div/div/div[2]/div/div[3]/div[1]/table/tbody"))
                            .FindElements(By.TagName("tr"));
                    }
                    catch (NotFoundException notFound)
                    {
                        rowGroup = new ReadOnlyCollection<IWebElement>(new List<IWebElement>());
                    }
                    catch (StaleElementReferenceException staleElem)
                    {
                        await Task.Delay(500);
                        rowGroup = webDriver
                            .FindElement(By.XPath("//*[@id=\"main_column_0\"]/div[2]/div/div/div/div[2]/div/div[3]/div[1]/table/tbody"))
                            .FindElements(By.TagName("tr"));
                    }
                    finally
                    {
                        webDriver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(30);
                    }

                    if (rowGroup.Count > 0)
                    {
                        //only save the order if there are items in the order
                        //Save Order
                        int retrySaveCount = 0;
                        while (true)
                        {
                            try
                            {
                                webDriver.FindElement(By.Id("Toolbar.SORPOETB40123")).Click();
                                break;
                            }
                            catch (Exception ex) when (ex is ElementClickInterceptedException)
                            {
                                await Task.Delay(500);
                                retrySaveCount++;
                                if (retrySaveCount > 10)
                                {
                                    throw;
                                }
                            }
                        }


                        for (var i = 0; i < contractExpiredPopupCount; i++)
                        {
                            try
                            {
                                //var closeExpiredContractPopupButton
                                //await Task.Delay(2000);
                                webDriver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(3);
                                var closeExpiredContractPopupButtons = webDriver.FindElements(
                                    By.CssSelector("body > div.k-widget.k-window.k-dialog > div.k-window-titlebar.k-dialog-titlebar.k-header > div > a"));

                                foreach (var elem in closeExpiredContractPopupButtons)
                                {
                                    var isHidden = (bool)jsExecutor.ExecuteScript("return (arguments[0].offsetParent === null)", elem);
                                    if (!isHidden) elem.Click();
                                }
                            }
                            catch (WebDriverTimeoutException timeout)
                            {
                                //continue with work
                            }
                            catch (NotFoundException notFound)
                            {
                                //continue with work
                            }
                        }
                        #endregion

                        rowGroup = webDriver
                                .FindElement(By.XPath("//*[@id=\"main_column_0\"]/div[2]/div/div/div/div[2]/div/div[3]/div[1]/table/tbody"))
                                .FindElements(By.TagName("tr"));

                        await CheckOrderSaveStatus(webDriver, orderInProgress);
                    }
                    else
                    {
                        //the order was not created, we should save the record trail in the database
                        orderInProgress.UpdatedDate = DateTime.Now;
                        orderInProgress.InProgress = false;
                        orderInProgress.CreationSuccess = false;
                        orderInProgress.ErrorMessage = "Order was not created successfully - no item rows present before saving";

                        await sysproOrderCreationHistoryAppService
                             .CreateOrUpdateSysproOrderCreationHistoryAsync(orderInProgress);
                    }

                    //after database logging, move onto the next order
                    //await CreateSysproOrders(webDriver);

                    if (windowHandles.Count > 1)
                    {
                        //close current tab
                        webDriver.Close();
                        windowHandles = webDriver.WindowHandles;
                        webDriver.SwitchTo().Window(windowHandles.LastOrDefault());
                    }

                    await NavigateToExpressOrderEntry(wait, webDriver, false);
                    await Task.Delay(10000);
                    //give the page 10 seconds to reload before continuing
                    #endregion

                }
            }
            catch (Exception ex)
            {
                //save order if there are rows
                try
                {
                    webDriver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(4);
                    var rows = webDriver
                        .FindElement(By.XPath("//*[@id=\"main_column_0\"]/div[2]/div/div/div/div[2]/div/div[3]/div[1]/table/tbody"))
                        .FindElements(By.TagName("tr"));
                    if (rows.Count > 0)
                    {
                        //save order
                        await FillOrderHeader((IJavaScriptExecutor)webDriver, currentOrder);
                        webDriver.FindElement(By.Id("Toolbar.SORPOETB40123")).Click();
                        await CheckOrderSaveStatus(webDriver, orderInProgress, ex);
                    }
                }
                catch (Exception ex2)
                {
                    //do nothing, for now since this codes errors will mask any of the actual errors
                }
                finally
                {
                    webDriver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(30);
                }
                throw;
            }

            static async Task FillOrderHeader(IJavaScriptExecutor jsExecutor, SysproOrderItem sysproOrder)
            {
                #region Base Order Details

                jsExecutor.ExecuteScript("document.querySelector('#main_column_0 > div:nth-child(1) > div > div > div > div > div > div > a').click()");
                await Task.Delay(3000);

                //Fields.SORPOEF1CustomerPurchaseOrder -- Input the MANIFEST NUMBER
                jsExecutor.ExecuteScript($"document.getElementById('Fields.SORPOEF1CustomerPurchaseOrder').value = '{sysproOrder.CustomerPurchaseOrder}'");
                jsExecutor.ExecuteScript("document.getElementById('Fields.SORPOEF1CustomerPurchaseOrder').dispatchEvent(new Event('change'))");
                //Fields.SORPOEF1ShipDate -- Input the SHIP DATE
                jsExecutor.ExecuteScript($"document.getElementById('Fields.SORPOEF1ShipDate').value = '{sysproOrder.ShipDate.ToString("dd/MM/yyyy")}'");
                //trigger change event for the above input
                jsExecutor.ExecuteScript("document.getElementById('Fields.SORPOEF1ShipDate').dispatchEvent(new Event('change'))");

                //Fields.SORPOEF2ShipVia
                jsExecutor.ExecuteScript($"document.getElementById('Fields.SORPOEF2ShipVia').value = '{sysproOrder.ShipVia}'");
                await Task.Delay(500);
                jsExecutor.ExecuteScript("document.querySelector('#offcanvas-919d59a6-f029-4118-c014-a91c4bdccd33 > header > span > i').click()");
                await Task.Delay(1000);

                #endregion
            }
        }

        private static async Task CheckOrderSaveStatus(IWebDriver? webDriver, SysproOrderCreationHistory orderInProgress, Exception exception = null)
        {
            await Task.Delay(10000);
            IWebElement row;
            ReadOnlyCollection<IWebElement> rowGroup;
            ReadOnlyCollection<IWebElement> gridListCellNotes = null;
            try
            {
                webDriver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(4);
                gridListCellNotes = webDriver.FindElements(By.ClassName("gridlist-cell-note"));
            }
            catch (NotFoundException notFound)
            {
                //continue with work
            }
            if (gridListCellNotes?.Count > 0)
            {
                //means we potentially have an issue, get the message and log it instead
                //use data-content attribute
                var errorMessage = gridListCellNotes![0].GetAttribute("data-content");

                orderInProgress.UpdatedDate = DateTime.Now;
                orderInProgress.InProgress = false;
                orderInProgress.ErrorMessage = errorMessage;

                await sysproOrderCreationHistoryAppService
                    .CreateOrUpdateSysproOrderCreationHistoryAsync(orderInProgress);
                UpdateItemsRows(webDriver, 0, out row, out rowGroup);
                //remove all rows so that we can create the next order
                for (var i = 0; i < rowGroup.Count; i++)
                {
                    UpdateItemsRows(webDriver, 0, out row, out rowGroup);
                    row.FindElements(By.TagName("td"))[0].FindElement(By.TagName("a")).Click();
                    await Task.Delay(500);
                }
            }
            else if (exception == null)
            {
                orderInProgress.UpdatedDate = DateTime.Now;
                orderInProgress.InProgress = false;
                orderInProgress.CreationSuccess = true;
                orderInProgress.ErrorMessage = "";

                await sysproOrderCreationHistoryAppService
                    .CreateOrUpdateSysproOrderCreationHistoryAsync(orderInProgress);
            }
            else if (exception != null)
            {
                orderInProgress.UpdatedDate = DateTime.Now;
                orderInProgress.InProgress = true;
                orderInProgress.CreationSuccess = false;
                orderInProgress.ErrorMessage = exception.Message;
                await sysproOrderCreationHistoryAppService
                    .CreateOrUpdateSysproOrderCreationHistoryAsync(orderInProgress);
            }
        }

        private static async Task ClickElement(IWebDriver? webDriver, By by)
        {
            int retryCount = 0;
            while (true)
            {
                try
                {
                    webDriver.FindElement(by).Click();
                    break;
                }
                catch (Exception ex) when (ex is ElementClickInterceptedException)
                {
                    retryCount++;
                    await Task.Delay(500);
                    if (retryCount > 10) throw;
                }
            }
        }

        private static async void WaitUntilElementStale(WebDriverWait wait, IWebElement element, int timeoutInSeconds)
        {
            try
            {
                wait.Timeout = TimeSpan.FromSeconds(timeoutInSeconds);
                wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.StalenessOf(element));
            }
            catch (WebDriverTimeoutException timeout)
            {
                //continue with work
            }
            finally
            {
                wait.Timeout = TimeSpan.FromSeconds(30);
                await Task.Delay(1000);
            }
        }

        private static async void WaitUntilElementIsNotStale(WebDriverWait wait, IWebElement element, int timeoutInSeconds)
        {
            //run a loop every 500ms and check if the element is stale, if it is not stale, update it
            var stale = false;
            var ms = 500;
        }

        private static async void WaitUntilElementVisible(WebDriverWait wait, By by, int timeoutInSeconds)
        {
            try
            {
                wait.Timeout = TimeSpan.FromSeconds(timeoutInSeconds);
                wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(by));
            }
            catch (WebDriverTimeoutException timeout)
            {
                //continue with work
            }
            finally
            {
                wait.Timeout = TimeSpan.FromSeconds(30);
                await Task.Delay(500);
            }
        }

        private static async void WaitForNewRowToBeAdded(WebDriverWait wait, IJavaScriptExecutor jsExecutor, int i)
        {
            try
            {
                wait.Timeout = TimeSpan.FromSeconds(10);
                wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementExists(By.CssSelector
                    ($"#main_column_0 > div:nth-child(2) > div > div > div > div > div > div > div > table > tbody > tr:nth-child({i + 1})")));
            }
            catch (WebDriverTimeoutException timeout)
            {
                //new row was not added, attempt to add again
                jsExecutor.ExecuteScript("document.getElementById('gridtoolbar.newrow').click()");
                WaitForNewRowToBeAdded(wait, jsExecutor, i);
            }
            finally
            {
                wait.Timeout = TimeSpan.FromSeconds(30);
                await Task.Delay(1000);
            }
        }

        private static async Task NavigateToExpressOrderEntry(
            WebDriverWait wait,
            IWebDriver webDriver,
            bool firstOrder = true)
        {
            #region Navigate to Sales Order Entry Express
            //webDriver.Navigate().GoToUrl(ConfigurationManager.AppSettings["SysproAvantiPortalUrl"]);
            //await Task.Delay(4000);
            //wait.Until
            //    (
            //        SeleniumExtras.WaitHelpers.ExpectedConditions
            //        .ElementToBeClickable(By.CssSelector("#fusion-toolbar > div > ul.nav.navbar-nav.navbar-left.mr-auto > li:nth-child(1)"))
            //    ).Click();
            //click using js
            ((IJavaScriptExecutor)webDriver).ExecuteScript("document.querySelector('#fusion-toolbar > div > ul.nav.navbar-nav.navbar-left.mr-auto > li:nth-child(1)').click()");

            await Task.Delay(1000);
            //scroll to the bottom of #fusion-sidebar-wrapper2
            ((IJavaScriptExecutor)webDriver).ExecuteScript("document.querySelector('#fusion-sidebar-wrapper2').scrollTo(0, document.querySelector('#fusion-sidebar-wrapper2').scrollHeight)");
            await Task.Delay(500);
            //click the 'sales orders' item in the dropdownlist
            if (firstOrder)
            {
                wait.Until
                (
                    SeleniumExtras.WaitHelpers.ExpectedConditions
                    .ElementExists(By.CssSelector("#programListMenu > ul > li:nth-child(33) > a"))
                );
                //click using js
                ((IJavaScriptExecutor)webDriver).ExecuteScript("document.querySelector('#programListMenu > ul > li:nth-child(33) > a').click()");

                await Task.Delay(500);

                //scroll down again
                ((IJavaScriptExecutor)webDriver).ExecuteScript("document.querySelector('#fusion-sidebar-wrapper2').scrollTo(0, document.querySelector('#fusion-sidebar-wrapper2').scrollHeight)");

                //click the 'sales order processing' item in the dropdownlist
                ((IJavaScriptExecutor)webDriver).ExecuteScript("document.querySelector('#programListMenu > ul > li.treeview.menu-open > ul > li:nth-child(17) > a').click()");
            }
            else  //scroll down again
                ((IJavaScriptExecutor)webDriver).ExecuteScript("document.querySelector('#fusion-sidebar-wrapper2').scrollTo(0, document.querySelector('#fusion-sidebar-wrapper2').scrollHeight)");
            //click the 'sales order entry express' item in the dropdownlist
            ((IJavaScriptExecutor)webDriver).ExecuteScript("document.querySelector('#programListMenu > ul > li.treeview.menu-open > ul > li.treeview.menu-open > ul > li:nth-child(10) > a').click();");

            #endregion
        }

        public static async void ClickTd(IWebDriver? webDriver, int rowIndex, int tdIndex)
        {
            //var script = $@"var tableRow = document.querySelectorAll('#main_column_0 table tbody tr:not(.k-footer-template)')[{rowIndex}];
            //                var td = tableRow.querySelectorAll('td')[{tdIndex}];
            //                td.click();";
            //((IJavaScriptExecutor)webDriver).ExecuteScript(script);
            Actions actions = new Actions(webDriver);
            var retryCount = 0;
            while (true)
            {
                try
                {
                    var tableRow = webDriver.FindElements(By.CssSelector("#main_column_0 table tbody tr:not(.k-footer-template)"))[rowIndex];
                    var td = tableRow.FindElements(By.TagName("td"))[tdIndex];
                    //td.Click();
                    actions.MoveToElement(td).Click().Perform();
                    break;
                }
                catch (Exception ex)
                {
                    if (ex is ElementClickInterceptedException || ex is NotFoundException)
                    {
                        //check if there is no stock popup, and click save if there is
                        try
                        {
                            //check for the 'insufficient stock' popup, for now we will just close by clicking the X
                            //Toolbar.FORMSORPOEFE60000
                            webDriver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(4);
                            var saveInsufficientStockPopupButton = webDriver.FindElement(By.Id("Toolbar.FORMSORPOEFE41000"));
                            saveInsufficientStockPopupButton.Click();
                        }
                        catch (Exception)
                        {
                            //continue with work
                        }
                    }
                    retryCount++;
                    await Task.Delay(500);
                    if (retryCount > 20) throw;
                }
            }
        }

        private static void UpdateItemsRows(IWebDriver? webDriver, int i, out IWebElement row, out ReadOnlyCollection<IWebElement> rowGroup)
        {
            try
            {
                rowGroup = webDriver
                                .FindElement(By.XPath("//*[@id=\"main_column_0\"]/div[2]/div/div/div/div[2]/div/div[3]/div[1]/table/tbody"))
                                .FindElements(By.TagName("tr"));
                row = rowGroup[i];
            }
            catch (IndexOutOfRangeException outOfRange)
            {
                throw;
            }
            catch (Exception ex)
            {
                //continue with work
                UpdateItemsRows(webDriver, i, out row, out rowGroup);
            }
        }
        public static String generateXPATH(IWebElement childElement, String current)
        {
            String childTag = childElement.TagName;
            if (childTag.Equals("html"))
            {
                return "/html[1]" + current;
            }
            var parentElement = childElement.FindElement(By.XPath(".."));
            var childrenElements = parentElement.FindElements(By.XPath("*"));
            int count = 0;
            for (int i = 0; i < childrenElements.Count; i++)
            {
                var childrenElement = childrenElements.ElementAt(i);
                String childrenElementTag = childrenElement.TagName;
                if (childTag.Equals(childrenElementTag))
                {
                    count++;
                }
                if (childElement.Equals(childrenElement))
                {
                    return generateXPATH(parentElement, "/" + childTag + "[" + count + "]" + current);
                }
            }
            return null;
        }
        public static async Task LoginSysproAvantiPortal(IWebDriver webDriver)
        {
            webDriver.Navigate().GoToUrl(ConfigurationManager.AppSettings["SysproAvantiPortalUrl"]);
            var userNameInput = webDriver.FindElement(By.Id("UserName"));
            userNameInput.SendKeys(ConfigurationManager.AppSettings["SysproAvantiPortalUsername"]);
            var passwordInput = webDriver.FindElement(By.Id("UserPwd"));
            passwordInput.SendKeys(ConfigurationManager.AppSettings["SysproAvantiPortalPassword"]);
            //click submit button
            //wait for 5 seconds

            await Task.Delay(2000);

            await SelectLoginCompany(webDriver);

            await Task.Delay(2000);

            ((IJavaScriptExecutor)webDriver).ExecuteScript("document.getElementById('keep_signed_in').click();");
            await Task.Delay(200);

            var companyText = webDriver.FindElement(
                By.CssSelector("#main_column_0 > div > div > form > div:nth-child(4) > div.col-xs-11.col-11.sys-pd-l-15 > span > span > input"))
                .GetAttribute("value");

            var checkCompCounter = 0;
            while (companyText != "2 - BRACE ABLE")
            {
                await SelectLoginCompany(webDriver);
                await Task.Delay(2000);
                companyText = webDriver.FindElement(
                    By.CssSelector("#main_column_0 > div > div > form > div:nth-child(4) > div.col-xs-11.col-11.sys-pd-l-15 > span > span > input"))
                    .GetAttribute("value");
                checkCompCounter++;
                if (checkCompCounter > 5)
                {
                    throw new Exception("Could not select correct company");
                }
            }

            //webDriver.FindElement(By.Id("SignInBtn")).Click();
            //click using js
            ((IJavaScriptExecutor)webDriver).ExecuteScript("document.getElementById('SignInBtn').click()");
            await Task.Delay(2000);
        }

        private static async Task SelectLoginCompany(IWebDriver webDriver)
        {
            webDriver.FindElement(By.CssSelector("#main_column_0 > div > div > form > div:nth-child(4) > div.col-xs-11.col-11.sys-pd-l-15 > span > span > span.k-select"));
            ((IJavaScriptExecutor)webDriver).ExecuteScript("document.querySelector('#main_column_0 > div > div > form > div:nth-child(4) > div.col-xs-11.col-11.sys-pd-l-15 > span > span > span.k-select').click()");
            await Task.Delay(2500);
            ((IJavaScriptExecutor)webDriver).ExecuteScript("document.querySelectorAll('#CompId_listbox li')[1].click()");
        }
    }
}
