using NSWEHealth.Amazon.PageObjects;
using NSWEHealth.Amazon.Setup;
using NUnit.Framework;
using NSWEHealth.Framework.Drivers;
using NSWEHealth.Framework.Wrapper;
using TechTalk.SpecFlow;
using ConfigType = NSWEHealth.Framework.Wrapper.TestConstant.ConfigTypes;
using ConfigKey = NSWEHealth.Framework.Wrapper.TestConstant.ConfigTypesKey;
using OpenQA.Selenium;


namespace NSWEHealth.Amazon.Steps
{
    [Binding]
    internal class AmazonTestSteps
    {
        private readonly HomePage? _homePage;
        private readonly SearchResultPage? _searchResultPage;
        private readonly IWebDriver? _driver;
        private readonly CommonMethods? _commonMethods;
        private readonly ScenarioContext? _scenarioContext;
        private static string Protocol => ConfigHelper.ReadConfigValue(ConfigType.WebDriverConfig, ConfigKey.Protocol) ?? string.Empty;
        private static string Url => SetAppUrl.SetUrl(Protocol);

        public AmazonTestSteps(ScenarioContext scenarioContext)
        {
            _driver = DriverHelper.Driver;
            _commonMethods = new CommonMethods();
            _scenarioContext = scenarioContext;
            _homePage = new HomePage(_driver);
            _searchResultPage = new SearchResultPage(_driver);
        }

        [Given(@"I open the Amazon australia home page")]
        public void GivenIOpenTheAmazonAustraliaHomePage()
        {
            DriverHelper.Navigate(Url);
            _homePage?.NavigateToAmazonAustralia();
        }


        [When(@"I want to search for '([^']*)'")]
        public void WhenIWantToSearchFor(string itemName)
        {
            _scenarioContext["ItemName"] = itemName;
            _homePage?.CheckHomePageIsDisplayed();
            _homePage?.SearchForAnItem(itemName);
        }

        [Then(@"the result shows the list of the searched item")]
        public void ThenTheResultShowsTheListOfTheSearchedItem()
        {
            var result = _searchResultPage?.GetLabelDisplayedResultFor();
            Assert.That(_scenarioContext["ItemName"], Is.EqualTo(result?.Replace("\"", "")));
        }

        [When(@"I select filter for '([^']*)' as '([^']*)'")]
        public void WhenISelectFilterForAs(string filterType, string filterValue)
        {
            switch (filterType.ToLowerInvariant())
            {
                case "brand":
                {
                    _searchResultPage?.FilterByBrand(
                        Enum.Parse<AmazonTestConstant.BrandName>(filterValue, true));
                    break;
                }
                case "displaytech":
                {
                    _searchResultPage?.FilterByDisplayTech(filterValue);
                    break;
                }
                case "screensize":
                {
                    _searchResultPage?.FilterByScreenSize(filterValue);
                    break;
                }
                case "displayresolution":
                {
                    _searchResultPage?.FilterByDisplayResolution(filterValue);
                    break;
                }
            }
        }

        [Then(@"I verify all the filter checkboxes are checked")]
        public void ThenIVerifyAllTheFilterCheckboxesAreChecked()
        {
            Assert.That(_searchResultPage?.VerifyFilteredResultListDisplayed(), Is.True, "All filter checkboxes are not checked");
        }

        [When(@"I sort the result by price low to high")]
        public void WhenISortTheResultByPriceLowToHigh()
        {
            _searchResultPage?.SortByPriceLowToHigh();
        }

        [Then(@"items are sorted by the price from low to high")]
        public void ThenItemsAreSortedByThePriceFromLowToHigh()
        {
            Assert.That(_searchResultPage?.GetSortedListText(), Is.EqualTo("Price: Low to high"), 
                "Items are not sorted by price low to high");
        }

    }
}
