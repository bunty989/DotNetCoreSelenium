using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.Extensions;
using OpenQA.Selenium.Support.UI;
using Serilog;
using ExpectedConditions = SeleniumExtras.WaitHelpers.ExpectedConditions;
using ConfigType = NSWEHealth.Framework.Wrapper.TestConstant.ConfigTypes;
using ConfigKey = NSWEHealth.Framework.Wrapper.TestConstant.ConfigTypesKey;
using LocatorType = NSWEHealth.Framework.Wrapper.TestConstant.LocatorType;
using WebDriverAction = NSWEHealth.Framework.Wrapper.TestConstant.WebDriverAction;

namespace NSWEHealth.Framework.Wrapper
{
    public class WebHelper
    {
        protected readonly IWebDriver? Driver;
        private LocatorType _locator;
        private string? _locatorInfo;
        private string? _webElementName;
        private string? _webElementValue;
        private string? _elementDisplayedText;

        public WebHelper(IWebDriver? driver)
        {
            Driver = driver;
        }
        public IWebElement? InitialiseDynamicWebElement(LocatorType locatorType, string? locatorInfo)
        {
            _locator = locatorType;
            _locatorInfo = locatorInfo;
            var dWait = new WebDriverWait(Driver ?? throw new ArgumentNullException(nameof(Driver)),
                        TimeSpan.FromSeconds(int.Parse(ConfigHelper.ReadConfigValue(ConfigType.WebDriverConfig,
                            ConfigKey.ObjectIdentificationTimeOut) ?? "0")));
            dWait.IgnoreExceptionTypes(typeof(StaleElementReferenceException),
                typeof(NoSuchElementException),
                typeof(ElementNotInteractableException),
                typeof(ElementNotInteractableException));
            try
            {
                IWebElement? dynamicElement;
                List<IWebElement?> webElements;
                switch (locatorType)
                {
                    case LocatorType.Id:
                        {
                            dynamicElement = dWait.Until(ExpectedConditions.ElementToBeClickable(By.Id(locatorInfo)));
                            webElements = [.. Driver?.FindElements(By.Id(locatorInfo))];
                            if (webElements.Count > 1)
                            {
                                foreach (var webE in webElements.Where(IsElementDisplayed))
                                {
                                    return webE;
                                }
                            }

                            break;
                        }
                    case LocatorType.ClassName:
                        {
                            dynamicElement =
                                dWait.Until(ExpectedConditions.ElementToBeClickable(By.ClassName(locatorInfo)));
                            break;
                        }
                    case LocatorType.Name:
                        {
                            dynamicElement = dWait.Until(ExpectedConditions.ElementToBeClickable(By.Name(locatorInfo)));
                            break;
                        }
                    case LocatorType.XPath:
                        {
                            dynamicElement = dWait.Until(ExpectedConditions.ElementToBeClickable(By.XPath(locatorInfo)));
                            webElements = [.. Driver?.FindElements(By.XPath(locatorInfo))];
                            if (webElements.Count > 1)
                            {
                                foreach (var webE in webElements.Where(IsElementDisplayed))
                                {
                                    return webE;
                                }
                            }

                            break;
                        }
                    case LocatorType.CssSelector:
                        {
                            dynamicElement =
                                dWait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector(locatorInfo)));
                            break;
                        }
                    case LocatorType.LinkText:
                        {
                            dynamicElement =
                                dWait.Until(ExpectedConditions.ElementToBeClickable(By.LinkText(locatorInfo)));
                            break;
                        }
                    case LocatorType.PartialLinkText:
                        {
                            dynamicElement =
                                dWait.Until(ExpectedConditions.ElementToBeClickable(By.PartialLinkText(locatorInfo)));
                            break;
                        }
                    case LocatorType.TagName:
                        {
                            dynamicElement =
                                dWait.Until(ExpectedConditions.ElementToBeClickable(By.TagName(locatorInfo)));
                            break;
                        }
                    default:
                        throw new ArgumentOutOfRangeException(nameof(locatorType), locatorType, null);
                }

                var webElementName = dynamicElement?.GetAttribute("name");
                var webElementValue = dynamicElement?.GetAttribute("value");
                var elementDisplayedText = string.IsNullOrEmpty(webElementValue) ? webElementName : webElementValue;
                Log.Debug("WebElement {0} is identified successfully", elementDisplayedText);

                _webElementName = dynamicElement?.GetAttribute("name");
                _webElementValue = dynamicElement?.GetAttribute("value");
                _elementDisplayedText = string.IsNullOrEmpty(_webElementValue) ? _webElementName : _webElementValue;
                return dynamicElement;
            }
            catch (StaleElementReferenceException ex)
            {
                Log.Debug("WebElement {0} identification error due to {1} handled", _locatorInfo, ex.Message);
                return ReIdentifyAndInitialiseDynamicWebElement();
            }
            catch (ElementNotInteractableException ex)
            {
                Log.Debug("WebElement {0} identification error due to {1} handled", _locatorInfo, ex.Message);
                return ReIdentifyAndInitialiseDynamicWebElement();
            }
            catch (UnhandledAlertException ex)
            {
                Log.Debug("WebElement {0} identification error due to {1} handled", _locatorInfo, ex.Message);
                return ReIdentifyAndInitialiseDynamicWebElement();
            }
            catch (Exception ex)
            {
                var strTemp = locatorInfo + " - failed to identify.";
                Log.Error(strTemp + Environment.NewLine + ex.Message);
                _elementDisplayedText = "WebElement with locator " + _locator + " and locator info as " + _locatorInfo;
                return null;
            }
        }

        public void PerformWebDriverAction(IWebElement? objWebElement, WebDriverAction webDriverAction, string? actionData = null)
        {
            if (objWebElement == null)
            {
                Log.Error("{0} Web Element is not identified so no action is performed", _elementDisplayedText);
                Assert.Fail($"{_elementDisplayedText} Web Element is not identified so no action is performed");
            }
            bool boolExecStep;
            var actFocus = new Actions(Driver);
            try
            {
                switch (webDriverAction)
                {
                    case WebDriverAction.Clear:
                        {
                            objWebElement?.Click();
                            objWebElement?.Clear();
                            boolExecStep = true;
                            Log.Debug("Clearing TextBox {0}", actionData);
                            break;
                        }
                    case WebDriverAction.Input:
                        {
                            objWebElement?.Click();
                            objWebElement?.Clear();
                            objWebElement?.SendKeys(actionData);
                            boolExecStep = true;
                            Log.Debug("Entering text {0} to TextBox {1}", actionData, _elementDisplayedText);
                            break;
                        }
                    case WebDriverAction.WaitInput:
                        {
                            objWebElement?.Click();
                            objWebElement?.SendKeys(Keys.Control + "a");
                            objWebElement?.SendKeys(Keys.Delete);
                            objWebElement?.SendKeys(actionData);
                            boolExecStep = true;
                            Log.Debug("Entering text {0} to TextBox {1}", actionData, _elementDisplayedText);
                            break;
                        }
                    case WebDriverAction.JavaScriptInput:
                        {
                            var js = "document.querySelector(\"" + _locatorInfo + "\")";
                            bool stepCompletion;
                            do
                            {
                                ExecuteJs(js + ".value=" + actionData);
                                stepCompletion = ReturnHiddenWebElementsValue(js).Equals(actionData);
                            } while (!stepCompletion);
                            boolExecStep = true;
                            Log.Debug("Entering text {0} to TextBox {1}", actionData, _locatorInfo);
                            break;
                        }
                    case WebDriverAction.Select:
                        {
                            objWebElement?.Click();
                            var selector = new SelectElement(objWebElement);
                            selector.SelectByText(actionData);
                            boolExecStep = true;
                            Log.Debug("Selecting option {0} from Selector {1}", actionData, _elementDisplayedText);
                            break;
                        }
                    case WebDriverAction.Click:
                        {
                            objWebElement?.Click();
                            boolExecStep = true;
                            Log.Debug("Clicking on button {0}", _elementDisplayedText);
                            break;
                        }
                    case WebDriverAction.JavaScriptClick:
                        {
                            var executor = Driver as IJavaScriptExecutor;
                            executor?.ExecuteScript("arguments[0].click();", objWebElement);
                            boolExecStep = true;
                            Log.Debug("Clicking on button {0}", _elementDisplayedText);
                            break;
                        }
                    case WebDriverAction.Focus:
                        {
                            actFocus.MoveToElement(objWebElement).Build().Perform();
                            boolExecStep = true;
                            Log.Debug("Focussing on button {0}", _elementDisplayedText);
                            break;
                        }
                    case WebDriverAction.DoubleClick:
                        {
                            actFocus.MoveToElement(objWebElement).DoubleClick().Build().Perform();
                            boolExecStep = true;
                            Log.Debug("Double clicking on button {0}", _elementDisplayedText);
                            break;
                        }
                    case WebDriverAction.ScrollToTop:
                        {
                            actFocus.MoveToElement(objWebElement).SendKeys(Keys.Home).Build().Perform();
                            boolExecStep = true;
                            Log.Debug("Double clicking on button {0}", _elementDisplayedText);
                            break;
                        }
                    case WebDriverAction.ScrollToBottom:
                        {
                            actFocus.MoveToElement(objWebElement).SendKeys(Keys.End).Build().Perform();
                            boolExecStep = true;
                            Log.Debug("Double clicking on button {0}", _elementDisplayedText);
                            break;
                        }
                    default:
                        {
                            boolExecStep = false;
                            break;
                        }
                }
            }
            catch (Exception ex)
            {
                var strException = ex.Message;
                switch (ex)
                {
                    case StaleElementReferenceException:
                        {
                            WebElementExceptionHandler(webDriverAction, actionData);
                            Log.Debug("Stale Element Exception for WebElement {0} Handled", _elementDisplayedText);
                            return;
                        }
                    case ElementClickInterceptedException:
                        {
                            Driver?.FindElement(By.XPath("//html")).Click();
                            WebElementExceptionHandler(webDriverAction, actionData);
                            Log.Debug("Element Click Intercepted Exception for WebElement {0} Handled", _elementDisplayedText);
                            return;
                        }
                    case ElementNotInteractableException:
                        {
                            Driver?.FindElement(By.XPath("//html")).Click();
                            WebElementExceptionHandler(webDriverAction, actionData);
                            Log.Debug("Element Not Interactable Exception for WebElement {0} Handled", _elementDisplayedText);
                            return;
                        }
                    case InvalidElementStateException:
                        {
                            Driver?.FindElement(By.XPath("//html")).Click();
                            WebElementExceptionHandler(webDriverAction, actionData);
                            Log.Debug("Element Invalid State Exception for WebElement {0} Handled", _elementDisplayedText);
                            return;
                        }
                    case UnhandledAlertException:
                        {
                            ReadAndHandleAlert();
                            Driver?.FindElement(By.XPath("//html")).Click();
                            WebElementExceptionHandler(webDriverAction, actionData);
                            Log.Debug("Unhandled Alert Exception for WebElement {0} Handled", _elementDisplayedText);
                            return;
                        }
                    default:
                        {
                            boolExecStep = false;
                            Log.Error("Unable to perform Action on WebElement {0} due to {1}",
                                _elementDisplayedText,
                                strException);
                            Assert.Fail($"Unable to perform Action on WebElement {_elementDisplayedText} due to {strException}");
                            break;
                        }
                }
            }
            if (!boolExecStep) { Log.Information("No Action was performed"); }
        }

        public List<IWebElement?> InitialiseWebElementsCollection(LocatorType locatorType, string locatorInfo)
        {
            var webElements = locatorType switch
            {
                LocatorType.Id => new List<IWebElement?>(Driver?.FindElements(By.Id(locatorInfo))),
                LocatorType.Name => new List<IWebElement?>(Driver?.FindElements(By.Name(locatorInfo))),
                LocatorType.ClassName => new List<IWebElement?>(Driver?.FindElements(By.ClassName(locatorInfo))),
                LocatorType.CssSelector => new List<IWebElement?>(Driver?.FindElements(By.CssSelector(locatorInfo))),
                LocatorType.XPath => new List<IWebElement?>(Driver?.FindElements(By.XPath(locatorInfo))),
                LocatorType.LinkText => new List<IWebElement?>(Driver?.FindElements(By.LinkText(locatorInfo))),
                LocatorType.PartialLinkText => new List<IWebElement?>(Driver?.FindElements(By.PartialLinkText(locatorInfo))),
                LocatorType.TagName => new List<IWebElement?>(Driver?.FindElements(By.TagName(locatorInfo))),
                _ => [.. Driver?.FindElements(By.XPath(locatorInfo))]
            };
            return webElements;
        }

        public string? ReadAndHandleAlert()
        {
            try
            {
                var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(10));
                wait.Until(ExpectedConditions.AlertIsPresent());
                var alert = Driver?.SwitchTo().Alert();
                var strWarning = alert?.Text;
                alert?.Accept();
                wait.Until(ExpectedConditions.AlertState(false));
                Log.Debug("Alert displayed as {0}", strWarning);
                return strWarning;
            }

            catch (Exception e)
            {
                Log.Error("Alert not displayed due to {0}", e.Message);
                return "Alert not displayed";
            }
        }

        public string? ReturnVisibleText(IWebElement? element)
        {
            string? visibleText;
            try
            {
                visibleText = element?.Text;
            }
            catch (Exception ex)
            {
                Log.Error("WebElement {0} threw exception {1} while fetching the visible text", element, ex.Message);
                visibleText = InitialiseDynamicWebElement(_locator, _locatorInfo)?.Text;
            }
            Log.Debug("The visible text for the webElement is {0}", visibleText);
            return visibleText;
        }

        public string? ReturnWebAttribute(IWebElement element, string webAttribute)
        {
            string? attributeValue;
            try
            {
                attributeValue = element.GetAttribute(webAttribute);
            }
            catch (Exception)
            {
                attributeValue = InitialiseDynamicWebElement(_locator, _locatorInfo)?.GetAttribute(webAttribute);
            }
            Log.Debug("The attribute {0} for the webElement is {1}", webAttribute, attributeValue);
            return attributeValue;
        }

        public string? ReturnCssAttribute(IWebElement element, string cssAttribute)
        {
            string? attributeValue;
            try
            {
                attributeValue = element.GetCssValue(cssAttribute);
            }
            catch (Exception)
            {
                attributeValue = InitialiseDynamicWebElement(_locator, _locatorInfo)?.GetAttribute(cssAttribute);
            }
            Log.Debug("The attribute {0} for the webElement is {1}", cssAttribute, attributeValue);
            return attributeValue;
        }

        public void GetPageReady()
        {
            try
            {
                new WebDriverWait(Driver, TimeSpan.FromSeconds(int.Parse(ConfigHelper.ReadConfigValue
                    (ConfigType.WebDriverConfig, ConfigKey.ObjectIdentificationTimeOut)))).Until(
                    d => ((IJavaScriptExecutor)d).ExecuteScript("return document.readyState").Equals("complete"));
            }
            catch (UnhandledAlertException)
            {
                ReadAndHandleAlert();
                GetPageReady();
            }
            catch (Exception ex)
            {
                Log.Error("Unable to get Page Ready state due to {0}", ex.Message);
            }
        }

        public static bool IsElementDisplayed(IWebElement? webElement)
        {
            try
            {
                var isDisplayed = webElement != null && webElement.Displayed;
                Log.Debug("The visibility of WebElement {0} is {1}",
                    webElement?.GetAttribute("value"),
                    isDisplayed.ToString());
                return isDisplayed;
            }
            catch (Exception ex)
            {
                Log.Error("The WebElement is not loaded correctly due to : \n {0}", ex.Message);
                return false;
            }
        }

        public void ExecuteJs(string javaScript, params object[] args)
        {
            Driver.ExecuteJavaScript(javaScript, args);
        }

        public string? GetWindowTitle()
        {
            GetPageReady();
            Log.Debug("The Title of the webpage is fetched as {0}", Driver?.Title);
            return Driver?.Title;
        }

        public IWebElement? FindWebElementFromDomUsingCssSelector(string cssSelector)
        {
            GetPageReady();
            try
            {
                var jScript = "return document.querySelector(\"" + cssSelector + "\")";
                var webElementFound = Driver.ExecuteJavaScript<IWebElement>(jScript);
                Log.Debug(
                    webElementFound != null
                        ? "The WebElement with CssSelector {0} is found on the DOM"
                        : "The WebElement with CssSelector {0} is not found on the DOM", cssSelector);
                return webElementFound;
            }
            catch (Exception ex)
            {
                Log.Error("The WebElement with CssSelector as {0} cant be found due to {1}", cssSelector, ex.Message);
                return null;
            }
        }

        public string ReturnHiddenWebElementsValue(string identifierJScript)
        {
            try
            {
                var hiddenWebElementsValue = Driver.ExecuteJavaScript<string>("return " + identifierJScript + ".value");
                Log.Debug("The Value of the WebElement {0} on screen is displayed as {1}",
                    identifierJScript,
                    hiddenWebElementsValue);
                return hiddenWebElementsValue;
            }
            catch (Exception ex)
            {
                Log.Error("The value of WebElement {0} cant be determined due to {1}",
                    identifierJScript, ex.Message);
                return "false";
            }
        }

        public Boolean IsChecked(IWebElement? chkBoxElement)
        {
            try
            {
                var isChecked = chkBoxElement != null && chkBoxElement.Selected;
                Log.Debug("The WebElement {0} is {1}",
                    chkBoxElement?.GetAttribute("value"),
                    isChecked.ToString());
                return isChecked;
            }
            catch (Exception ex)
            {
                Log.Error("The WebElement is not loaded correctly due to : \n {0}", ex.Message);
                return false;
            }
        }

        private void WebElementExceptionHandler(WebDriverAction webDriverAction, string? strData = null)
        {
            var objWebElement = InitialiseDynamicWebElement(_locator, _locatorInfo);
            PerformWebDriverAction(objWebElement, webDriverAction, strData);
        }

        private IWebElement? ReIdentifyAndInitialiseDynamicWebElement()
        {
            return InitialiseDynamicWebElement(_locator, _locatorInfo);
        }
    }
}