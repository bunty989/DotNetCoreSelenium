using OpenQA.Selenium;
using NSWEHealth.Framework.Wrapper;
using LocatorType = NSWEHealth.Framework.Wrapper.
    TestConstant.LocatorType;
using WebDriverAction = NSWEHealth.Framework.Wrapper.
    TestConstant.WebDriverAction;


namespace NSWEHealth.Amazon.PageObjects
{
    internal class HomePage
    {
        private readonly WebHelper _webHelper;
        protected IWebElement? BtnContinueShopping =>
            _webHelper.InitialiseDynamicWebElement(LocatorType.CssSelector,
                "[alt='Continue shopping']");
        protected IWebElement? TxtSearch =>
            _webHelper.InitialiseDynamicWebElement(LocatorType.CssSelector,
                "#twotabsearchtextbox");
        protected IWebElement? BtnSearch =>
            _webHelper.InitialiseDynamicWebElement(LocatorType.CssSelector,
                "[id$='submit-button']");

        public HomePage(IWebDriver? driver) =>
            _webHelper = new WebHelper(driver);

        public void NavigateToAmazonAustralia()
        {
            if (_webHelper.FindWebElementFromDomUsingCssSelector("[id='nav-bb-logo']") != null)
            {
                _webHelper.PageRefresh();
                _webHelper.GetPageReady();
            }
        }

        public void CheckHomePageIsDisplayed() {
            if (_webHelper.FindWebElementFromDomUsingCssSelector("[alt='Continue shopping']") != null)
            { 
                _webHelper.PerformWebDriverAction(BtnContinueShopping,WebDriverAction.Click, null);
            }
        }

        public void SearchForAnItem(string itemName)
        {
            EnterItemToSearch(itemName);
            ClickSubmitBtn();
        }

        private void EnterItemToSearch(string itemName)
        {
            if(!_webHelper.GetDriverType().ToLowerInvariant().Contains("safari"))
            _webHelper.PerformWebDriverAction(TxtSearch, WebDriverAction.Input,
                itemName);
            else
            {
                TxtSearch?.SendKeys(itemName);
            }
        }

        private void ClickSubmitBtn()
        {
            _webHelper.PerformWebDriverAction(BtnSearch, WebDriverAction.Click,
                null);
        }
    }
}
