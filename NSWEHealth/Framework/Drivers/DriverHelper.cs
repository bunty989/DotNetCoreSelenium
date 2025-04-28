using System;
using NSWEHealth.Framework.Wrapper;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.IE;
using Serilog;
using Browser = NSWEHealth.Framework.Wrapper.TestConstant.BrowserType;

namespace NSWEHealth.Framework.Drivers
{
    public static class DriverHelper
    {
        public static IWebDriver? Driver;
        private static string? _browserName;
        private static string? _browserVersion;
        //=> ((RemoteWebDriver)Driver).Capabilities.GetCapability("browserName").ToString();
        private static BrowserVersionHelper _browserVersionHelper => new();

        public static IWebDriver? InvokeDriverInstance(Browser browserType)
        {
            _browserVersion = _browserVersionHelper.GetBrowserVersion(browserType);
            switch (browserType)
            {
                case Browser.Chrome:
                    {
                        var chromeOption = new ChromeOptions();
                        chromeOption.AddArguments("start-maximized", "--disable-gpu", "--no-sandbox");
                        chromeOption.AddExcludedArgument("enable-automation");
                        //chromeOption.AddAdditionalCapability("useAutomationExtension", false);
                        chromeOption.AddUserProfilePreference("credentials_enable_service", false);
                        chromeOption.AddUserProfilePreference("profile.password_manager_enabled", false);
                        chromeOption.PageLoadStrategy = PageLoadStrategy.Eager;
                        Driver = new ChromeDriver(chromeOption);
                        break;
                    }
                case Browser.InternetExplorer:
                    {
                        var ieOptions = new InternetExplorerOptions
                        {
                            IntroduceInstabilityByIgnoringProtectedModeSettings = true,
                            RequireWindowFocus = true,
                            EnsureCleanSession = true,
                            IgnoreZoomLevel = true
                        };
                        ieOptions.AddAdditionalInternetExplorerOption(CapabilityType.AcceptSslCertificates, true);
                        ieOptions.PageLoadStrategy = PageLoadStrategy.Eager;
                        Driver = new InternetExplorerDriver(ieOptions);
                        break;
                    }
                case Browser.Firefox:
                    {
                        var ffOptions = new FirefoxOptions
                        {
                            AcceptInsecureCertificates = true
                        };
                        ffOptions.SetPreference("permissions.default.image", 1);
                        ffOptions.PageLoadStrategy = PageLoadStrategy.Eager;
                        Driver = new FirefoxDriver(ffOptions);
                        break;
                    }
                case Browser.Edge:
                    {
                        var edgeOptions = new EdgeOptions
                        {
                            AcceptInsecureCertificates = true,
                            PageLoadStrategy = PageLoadStrategy.Eager
                        };
                        Driver = new EdgeDriver(edgeOptions);
                        break;
                    }
                case Browser.ChromeHeadless:
                    {
                        var chromeOption = new ChromeOptions();
                        chromeOption.AddArguments("disable-gpu", "no-sandbox", "window-size=1280,800", "--headless=new");
                        chromeOption.AddExcludedArgument("enable-automation");
                        chromeOption.AddUserProfilePreference("credentials_enable_service", false);
                        chromeOption.AddUserProfilePreference("profile.password_manager_enabled", false);
                        chromeOption.PageLoadStrategy = PageLoadStrategy.Eager;
                        Driver = new ChromeDriver(chromeOption);
                        break;
                    }
                case Browser.ChromeIncognito:
                    {
                        var chromeOption = new ChromeOptions();
                        chromeOption.AddArguments("start-maximized", "--disable-gpu", "--no-sandbox", "--incognito");
                        chromeOption.AddExcludedArgument("enable-automation");
                        chromeOption.AddAdditionalChromeOption("useAutomationExtension", false);
                        chromeOption.AddUserProfilePreference("credentials_enable_service", false);
                        chromeOption.AddUserProfilePreference("profile.password_manager_enabled", false);
                        chromeOption.PageLoadStrategy = PageLoadStrategy.Eager;
                        Driver = new ChromeDriver(chromeOption);
                        break;
                    }
                default:
                    {
                        var chromeOption = new ChromeOptions();
                        chromeOption.AddArguments("start-maximized", "--disable-gpu", "--no-sandbox");
                        chromeOption.AddExcludedArgument("enable-automation");
                        chromeOption.AddAdditionalChromeOption("useAutomationExtension", false);
                        chromeOption.AddUserProfilePreference("credentials_enable_service", false);
                        chromeOption.AddUserProfilePreference("profile.password_manager_enabled", false);
                        chromeOption.PageLoadStrategy = PageLoadStrategy.Eager;
                        Driver = new ChromeDriver(chromeOption);
                        break;
                    }
            }
            _browserName = browserType.ToString();
            Log.Information("Started {0} WebDriver successfully", _browserName);
            Driver.Manage().Window.Maximize();
            Driver.Manage().Timeouts().ImplicitWait =
                TimeSpan.FromSeconds(int.Parse
                (ConfigHelper.ReadConfigValue
                (TestConstant.ConfigTypes.WebDriverConfig, TestConstant.ConfigTypesKey.ImplicitWaitTimeout)));
            Driver.Manage().Timeouts().PageLoad =
                TimeSpan.FromSeconds(int.Parse
                (ConfigHelper.ReadConfigValue
                    (TestConstant.ConfigTypes.WebDriverConfig, TestConstant.ConfigTypesKey.PageLoadTimeOut)));
            return Driver;
        }

        public static void Navigate(string url)
        {
            Driver?.Navigate().GoToUrl(url);
            Log.Information("Driver successfully Navigated to {0}", Driver?.Url);
        }

        public static void QuitDriverInstance()
        {
            if (Driver == null)
            {
                Log.Information("Driver Instance already Killed");
                return;
            }
            try
            {
                Driver.Quit();
                Log.Information("Quit {0} WebDriver successfully", _browserName);
            }
            catch (Exception e)
            {
                Log.Error("Unable to Quit {0} WebDriver due to {1}", _browserName, e.Message);
            }
        }
    }
}
