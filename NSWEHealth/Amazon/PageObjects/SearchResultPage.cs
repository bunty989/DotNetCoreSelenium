using OpenQA.Selenium;
using NSWEHealth.Framework.Wrapper;
using LocatorType = NSWEHealth.Framework.Wrapper.
    TestConstant.LocatorType;
using WebDriverAction = NSWEHealth.Framework.Wrapper.
    TestConstant.WebDriverAction;
using BrandName = NSWEHealth.Amazon.AmazonTestConstant.
    BrandName;


namespace NSWEHealth.Amazon.PageObjects
{
    internal class SearchResultPage
    {
        private readonly WebHelper _webHelper;
        protected IWebElement? LabelSearchResult =>
            _webHelper.InitialiseDynamicWebElement(LocatorType.CssSelector,
                "[data-cel-widget^='UPPER-RESULT_INFO_BAR'] h2 span[class]");
        protected IWebElement? BtnSearch =>
            _webHelper.InitialiseDynamicWebElement(LocatorType.CssSelector,
                "[id$='submit-button']");
        protected IWebElement? ChkBoxBrandSony =>
            _webHelper.InitialiseDynamicWebElement(LocatorType.CssSelector,
                "[aria-label*='the filter Sony'] input+i");
        protected IWebElement? ChkBoxDisplayTechOled =>
            _webHelper.InitialiseDynamicWebElement(LocatorType.CssSelector,
                "[aria-label*='the filter OLED'] input+i");
        protected IWebElement? ChkBoxScreenSize50In =>
            _webHelper.InitialiseDynamicWebElement(LocatorType.CssSelector,
                "[aria-label*='the filter 50-59 in'] input+i");
        protected IWebElement? ChkBoxScreenSize60In =>
            _webHelper.InitialiseDynamicWebElement(LocatorType.CssSelector,
                "[aria-label*='the filter 60-69 in'] input+i");
        protected IWebElement? DrpDownSortBy =>
            _webHelper.InitialiseDynamicWebElement(LocatorType.CssSelector,
                "[id='s-result-sort-select']+span[class*='button']");
        protected IWebElement? LabelLowToHigh =>
            _webHelper.InitialiseDynamicWebElement(LocatorType.CssSelector,
                "#s-result-sort-select_1");
        protected IWebElement? SelectedDropdownTxt =>
            _webHelper.InitialiseDynamicWebElement(LocatorType.CssSelector,
                "[data-action='a-dropdown-button'] span[class$='prompt']");
        protected IWebElement? Spinner =>
            _webHelper.FindWebElementFromDomUsingCssSelector
            ("[class^='s-result-list-placeholder'] span[class^='a-spinner']");

        public SearchResultPage(IWebDriver? driver) =>
            _webHelper = new WebHelper(driver);

        public string GetLabelDisplayedResultFor()
         => _webHelper?.ReturnVisibleText(LabelSearchResult);

        public void FilterByBrand(BrandName brandName)
        {
            var brandElement = brandName switch
            {
                BrandName.Sony => ChkBoxBrandSony,
                _ => null
            };
            _webHelper?.PerformWebDriverAction(brandElement, WebDriverAction.Click);
            WaitTillSpinnerExists();
        }

        public void FilterByDisplayTech(string displayTech)
        {
            var displayTechElement = displayTech switch
            {
                AmazonTestConstant.DisplayTech.OLED => ChkBoxDisplayTechOled,
                _ => null
            };
            _webHelper?.PerformWebDriverAction(displayTechElement, WebDriverAction.Click);
            WaitTillSpinnerExists();
        }

        public void FilterByScreenSize(string screenSize)
        {
            var screenSizeElement = screenSize switch
            {
                AmazonTestConstant.ScreenSize.SixtyToSixtyNine => ChkBoxScreenSize60In,
                AmazonTestConstant.ScreenSize.FiftyToFiftyNine => ChkBoxScreenSize50In,
                _ => null
            };
            _webHelper?.PerformWebDriverAction(screenSizeElement, WebDriverAction.Click);
            WaitTillSpinnerExists();
        }

        public bool VerifyFilteredResultListDisplayed() =>
            //WebHelper.IsElementDisplayed(ChkBoxModel2024);
        _webHelper.IsChecked(ChkBoxScreenSize50In?.FindElement(By.XPath("preceding-sibling::input")));

        public void SortByPriceLowToHigh()
        {
            bool flag = false;
            do
            {
                _webHelper.PerformWebDriverAction(DrpDownSortBy, WebDriverAction.Click);
                _webHelper.PerformWebDriverAction(LabelLowToHigh, WebDriverAction.Focus, null);
                _webHelper.GetPageReady();
                flag = WebHelper.IsElementDisplayed(LabelLowToHigh);
            }
            while (!flag);
            var locator = "#s-result-sort-select_1";
            var js = "document.querySelector(\""+locator+"\").click()";
            _webHelper.ExecuteJs(js);
            WaitTillSpinnerExists();
            //_webHelper.PerformWebDriverAction(LabelLowToHigh, WebDriverAction.Click);
        }

        public string? GetSortedListText() =>
         _webHelper?.ReturnVisibleText(SelectedDropdownTxt);

        public void WaitTillSpinnerExists()
        {
            bool spinnerFlag = true;
            var count = 0;
            do
            {
                Thread.Sleep(500);
                spinnerFlag = WebHelper.IsElementDisplayed(Spinner);
                count++;
            }
            while (spinnerFlag && count<30);
        }
    }
}
