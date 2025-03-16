using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BA.OrderScraper.Shared
{
    public static class Consts
    {
        public static class JavaScript
        {
            const string waitForElmScript = @"
						var callback = arguments[arguments.length - 1];
						async function waitForElm(selector, timeout = 800) {
							return new Promise(async function(resolve, reject) {

								if (document.querySelector(selector)) {
									await delay(200);
									return resolve(document.querySelector(selector));
								}

								const observer = new MutationObserver((mutationsList, observer) => {
									const element = document.querySelector(selector);
									if (element) {
										observer.disconnect();
										resolve(element);
									}
								});

								observer.observe(document.body, {
									childList: true,
									subtree: true
								});

							});
						};";
            const string triggerChangeScript = @"
							async function triggerElementChange(e){
								if(document.createEvent){
									event = document.createEvent(""HTMLEvents"");
									event.initEvent(""change"", true, true);
									event.eventName = ""change"";
									e.dispatchEvent(event);
								} else {
									event = document.createEventObject();
									event.eventName = ""change"";
									event.eventType = ""change"";
									e.fireEvent(""on"" + event.eventType, event);
								}
								await delay(250);
							}";
            const string getElementByXPath = @"function getElementByXPath(xpath) {
    let result = document.evaluate(xpath, document, null, XPathResult.FIRST_ORDERED_NODE_TYPE, null);
    return result.singleNodeValue;
}";
            const string triggerInputScript = @"async function triggerElementInput(e){
	if(document.createEvent){
		event = document.createEvent('HTMLEvents');
		event.initEvent('input', true, true);
		event.eventName = 'input';
		e.dispatchEvent(event);
	} else {
		event = document.createEventObject();
		event.eventName = 'input';
		event.eventType = 'input';
		e.fireEvent('on' + event.eventType, event);
	}
	await delay(10);
}";

            const string triggerKeyUpScript = @"async function triggerInputKeyUp(e){
	if(document.createEvent){
		event = document.createEvent('HTMLEvents');
		event.initEvent('keyup', true, true);
		event.eventName = 'keyup';
		e.dispatchEvent(event);
	} else {
		event = document.createEventObject();
		event.eventName = 'keyup';
		event.eventType = 'keyup';
		e.fireEvent('on' + event.eventType, event);
	}
	await delay(10);
}";

            const string triggerFocusoutScript = @"async function triggerFocusout(e){
	if(document.createEvent){
		event = document.createEvent('HTMLEvents');
		event.initEvent('focusout', true, true);
		event.eventName = 'focusout';
		e.dispatchEvent(event);
	} else {
		event = document.createEventObject();
		event.eventName = 'focusout';
		event.eventType = 'focusout';
		e.fireEvent('on' + event.eventType, event);
	}
	await delay(10);
}";
            const string generalScript = @"const delay = ms => new Promise(res => setTimeout(res, ms));var event;";
            public const string baseScript = getElementByXPath + triggerFocusoutScript + triggerKeyUpScript + waitForElmScript + triggerChangeScript + triggerInputScript + generalScript;
        }
        public static class JobType
        {
            public const string ToyotaOrdersImport = "toyotaordersimport";
            public const string SysproOrderCreate = "sysproordercreate";
        }

        public static class ImportType
        {
            public const string Manifest = "manifest";
            public const string Skid = "skid";
            public const string Kanban = "kanban";
        }
    }
}
