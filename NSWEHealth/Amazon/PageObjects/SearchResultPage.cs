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
        protected IWebElement? ChkBoxResolution4K =>
            _webHelper.InitialiseDynamicWebElement(LocatorType.CssSelector,
                "[aria-label*='the filter 4K'] input+i");
        protected IWebElement? ChkBoxModel2024 =>
            _webHelper.InitialiseDynamicWebElement(LocatorType.CssSelector,
                "[aria-label*='the filter 2024'] input+i");
        protected IWebElement? DrpDownSortBy =>
            _webHelper.InitialiseDynamicWebElement(LocatorType.CssSelector,
                "[id='s-result-sort-select']+span[class*='button']");
        protected IWebElement? LabelLowToHigh =>
            _webHelper.InitialiseDynamicWebElement(LocatorType.CssSelector,
                "#s-result-sort-select_1");
        protected IWebElement? SelectedDropdownTxt =>
            _webHelper.InitialiseDynamicWebElement(LocatorType.CssSelector,
                "[data-action='a-dropdown-button'] span[class$='prompt']");

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
        }

        public void FilterByResolution(string resolution)
        {
            var resolutionElement = resolution switch
            {
                AmazonTestConstant.Resolution.FourK => ChkBoxResolution4K,
                _ => null
            };
            _webHelper?.PerformWebDriverAction(resolutionElement, WebDriverAction.Click);
        }

        public void FilterByModel(string model)
        {
            var modelElement = model switch
            {
                AmazonTestConstant.Model.TwentyTwentyFour => ChkBoxModel2024,
                _ => null
            };
            _webHelper?.PerformWebDriverAction(modelElement, WebDriverAction.Click);
        }

        public bool VerifyFilteredResultListDisplayed() =>
            //WebHelper.IsElementDisplayed(ChkBoxModel2024);
        _webHelper.IsChecked(ChkBoxModel2024?.FindElement(By.XPath("preceding-sibling::input")));

        public void SortByPriceLowToHigh()
        {
            _webHelper.PerformWebDriverAction(DrpDownSortBy, WebDriverAction.Click);
            _webHelper.PerformWebDriverAction(LabelLowToHigh, WebDriverAction.Focus, null);
            _webHelper.PerformWebDriverAction(LabelLowToHigh, WebDriverAction.JavaScriptClick);
        }

        public string? GetSortedListText() =>
         _webHelper?.ReturnVisibleText(SelectedDropdownTxt);
    }
}
