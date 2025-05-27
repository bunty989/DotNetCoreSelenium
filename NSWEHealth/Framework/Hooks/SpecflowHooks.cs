using AventStack.ExtentReports;
using AventStack.ExtentReports.Gherkin.Model;
using AventStack.ExtentReports.Model;
using AventStack.ExtentReports.Reporter;
using NSWEHealth.Framework.Drivers;
using NSWEHealth.Framework.Wrapper;
using OpenQA.Selenium;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using TechTalk.SpecFlow;
using Log = Serilog.Log;

namespace NSWEHealth.Framework.Hooks
{
    [Binding]
    public sealed class SpecflowHooks
    {
        [ThreadStatic]
        private static IWebDriver? _driver;
        [ThreadStatic]
        private static ExtentTest? _feature;
        [ThreadStatic]
        private static ExtentTest? _scenario;
        [ThreadStatic]
        private static ExtentTest? _step;

        private static ExtentReports? _extent;
        private static string _scenarioType = "ui";
        private static string BrowserType => ConfigHelper.ReadConfigValue
            (TestConstant.ConfigTypes.WebDriverConfig, TestConstant.ConfigTypesKey.Browser);
        private static string? BrowserVersion => BrowserVersionHelper.GetBrowserVersion(
            Enum.Parse<TestConstant.BrowserType>(BrowserType, true));

        [BeforeScenario]
        public static void BeforeScenario(ScenarioContext context)
        {
            var scenarioName = context.ScenarioInfo.Title;
            if (context.ScenarioInfo.Arguments?.Count > 0)
            {
                scenarioName = scenarioName +
                               "{" +
                               context.ScenarioInfo.Arguments.Keys.OfType<string>()
                                   .Skip(0)
                                   .First() +
                               ", " +
                               (string?)context.ScenarioInfo.Arguments[0] +
                               "}";
            }
            _scenario = _feature?.CreateNode<Scenario>(scenarioName);
            Log.Information("Selecting Scenario {0} to run", scenarioName);
            if (context.ScenarioInfo.Tags.Contains("ignore")) return;
            foreach (var tags in context.ScenarioInfo.ScenarioAndFeatureTags)
            {
                if (tags.ToLower().Contains("api"))
                {
                    _scenarioType = "api";
                }
            }
            if (!_scenarioType.Equals("ui")) return;
            DriverHelper.InvokeDriverInstance(
                (TestConstant.BrowserType)Enum.Parse(typeof(TestConstant.BrowserType), BrowserType, true));
            _driver = DriverHelper.Driver;
        }

        [BeforeTestRun]
        public static void BeforeTestRun()
        {
            var formattedDateTime = DateTime.Now.ToString("dd-MM-yyyy-HH-mm-ss");
            var reportFilePath =
                TestConstant.PathVariables.GetBaseDirectory + Path.DirectorySeparatorChar + TestConstant.PathVariables.HtmlReportFolder
                                                            + Path.DirectorySeparatorChar + formattedDateTime;
            CommonMethods.CreateFolder(reportFilePath);
            var levelSwitch = new LoggingLevelSwitch(GetLogLevel());
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.ControlledBy(levelSwitch)
                .WriteTo.File(reportFilePath + TestConstant.PathVariables.LogName,
                    outputTemplate: "{Timestamp: yyyy-MM-dd HH:mm:ss.fff} | {Level:u3} | {Message} | {NewLine}",
                    rollingInterval: RollingInterval.Day).CreateLogger();
            ExtentSparkReporter htmlReport = new(reportFilePath + Path.DirectorySeparatorChar + "ExtentReport.html");
            htmlReport.LoadXMLConfig(TestConstant.PathVariables.ReportPath + Path.DirectorySeparatorChar
                                                                        + TestConstant.PathVariables.ExtentConfigName);
            _extent = new ExtentReports();
            Dictionary<string, string?> sysInfo = new()
            {
                { "Host Name", Environment.MachineName },
                { "Domain", Environment.UserDomainName },
                { "Username", Environment.UserName },
                {"Browser Name", BrowserType},
                {"Browser Version", BrowserVersion }
            };
            foreach (var (key, value) in sysInfo) { _extent.AddSystemInfo(key, value); }
            _extent.AttachReporter(htmlReport);
        }

        [BeforeFeature]
        public static void BeforeFeature(FeatureContext context)
        {
            _feature = _extent?.CreateTest<Feature>(context.FeatureInfo.Title);
            Log.Information("Selecting feature file {0} to run", context.FeatureInfo.Title);
        }

        [BeforeStep]
        public static void BeforeStep()
        {
            _step = _scenario;
        }

        [AfterStep]
        public void AfterStep(ScenarioContext context)
        {
            var stepType = context.StepContext.StepInfo.StepDefinitionType + " ";
            var stepStatus = context.StepContext.Status;
            switch (stepStatus)
            {
                case ScenarioExecutionStatus.Skipped:
                    {
                        SkipStep(context, stepType);
                        break;
                    }
                case ScenarioExecutionStatus.OK:
                    {
                        if (_scenarioType.Equals("api"))
                        {
                            switch (stepType.ToUpper().Trim())
                            {
                                case "GIVEN":
                                    {
                                        _step?.CreateNode<Given>(context.StepContext.StepInfo.Text).Pass(stepType.Trim());
                                        break;
                                    }
                                case "WHEN":
                                    {
                                        _step?.CreateNode<When>(context.StepContext.StepInfo.Text).Pass(stepType.Trim());
                                        break;
                                    }
                                case "THEN":
                                    {
                                        _step?.CreateNode<Then>(context.StepContext.StepInfo.Text).Pass(stepType.Trim());
                                        break;
                                    }
                                case "AND":
                                    {
                                        _step?.CreateNode<And>(context.StepContext.StepInfo.Text).Pass(stepType.Trim());
                                        break;
                                    }
                            }
                        }
                        else
                        {
                            var mediaEntity = AttachScreenShot(context.ScenarioInfo.Title);
                            switch (stepType.ToUpper().Trim())
                            {
                                case "GIVEN":
                                    {
                                        _step?.CreateNode<Given>(context.StepContext.StepInfo.Text).Pass(stepType.Trim(), mediaEntity);
                                        break;
                                    }
                                case "WHEN":
                                    {
                                        _step?.CreateNode<When>(context.StepContext.StepInfo.Text).Pass(stepType.Trim(), mediaEntity);
                                        break;
                                    }
                                case "THEN":
                                    {
                                        _step?.CreateNode<Then>(context.StepContext.StepInfo.Text).Pass(stepType.Trim(), mediaEntity);
                                        break;
                                    }
                                case "AND":
                                    {
                                        _step?.CreateNode<And>(context.StepContext.StepInfo.Text).Pass(stepType.Trim(), mediaEntity);
                                        break;
                                    }
                            }
                        }
                        break;
                    }
                case ScenarioExecutionStatus.StepDefinitionPending:
                    {
                        SkipStep(context, stepType);
                        break;
                    }
                case ScenarioExecutionStatus.UndefinedStep:
                    {
                        SkipStep(context, stepType);
                        break;
                    }
                case ScenarioExecutionStatus.BindingError:
                    {
                        ErrorStep(context, stepType);
                        break;
                    }
                case ScenarioExecutionStatus.TestError:
                    {
                        ErrorStep(context, stepType);
                        break;
                    }
                default:
                    {
                        ErrorStep(context, stepType);
                        break;
                    }
            }
        }

        [AfterFeature]
        public static void AfterFeature(FeatureContext context)
        {
            _extent?.Flush();
            Log.Information("Ending feature file {0} execution", context.FeatureInfo.Title);
        }

        [AfterScenario]
        public void AfterScenario(ScenarioContext context)
        {
            if (_scenarioType.Equals("api")) return;
            DriverHelper.QuitDriverInstance();
            Log.Debug("Ending Scenario {0} execution", context.ScenarioInfo.Title);
        }

        [AfterTestRun]
        public static void AfterTestRun()
        {
            Log.CloseAndFlush();
        }

        private static Media AttachScreenShot(string name)
        {
            var base64 = TakesScreenShot();
            return MediaEntityBuilder.CreateScreenCaptureFromBase64String(base64, name).Build();
        }

        private static string? TakesScreenShot()
        {
            return (_driver as ITakesScreenshot)?.GetScreenshot().AsBase64EncodedString;
        }

        private static LogEventLevel GetLogLevel()
        {
            var logEventLevel =
                ConfigHelper.ReadConfigValue("", TestConstant.LoggerLevel.LogLevel)?.ToLower() switch
                {
                    "all" => LogEventLevel.Verbose,
                    "info" => LogEventLevel.Information,
                    "warning" => LogEventLevel.Warning,
                    "error" => LogEventLevel.Error,
                    "debug" => LogEventLevel.Debug,
                    _ => LogEventLevel.Debug
                };
            return logEventLevel;
        }

        private void ErrorStep(ScenarioContext context, string stepType)
        {
            Log.Error("Test Step Failed due to | " + context.TestError.Message);
            if (_scenarioType.Equals("api"))
            {
                switch (stepType.ToUpper().Trim())
                {
                    case "GIVEN":
                        {
                            _step?.CreateNode<Given>(context.StepContext.StepInfo.Text).Fail(context.TestError.Message);
                            break;
                        }
                    case "WHEN":
                        {
                            _step?.CreateNode<When>(context.StepContext.StepInfo.Text).Fail(context.TestError.Message);
                            break;
                        }
                    case "THEN":
                        {
                            _step?.CreateNode<Then>(context.StepContext.StepInfo.Text).Fail(context.TestError.Message);
                            break;
                        }
                    case "AND":
                        {
                            _step?.CreateNode<And>(context.StepContext.StepInfo.Text).Fail(context.TestError.Message);
                            break;
                        }
                }
            }
            else
            {
                var mediaEntity = AttachScreenShot(context.ScenarioInfo.Title);
                switch (stepType.ToUpper().Trim())
                {
                    case "GIVEN":
                        {
                            _step?.CreateNode<Given>(context.StepContext.StepInfo.Text).Fail(context.TestError.Message, mediaEntity);
                            break;
                        }
                    case "WHEN":
                        {
                            _step?.CreateNode<When>(context.StepContext.StepInfo.Text).Fail(context.TestError.Message, mediaEntity);
                            break;
                        }
                    case "THEN":
                        {
                            _step?.CreateNode<Then>(context.StepContext.StepInfo.Text).Fail(context.TestError.Message, mediaEntity);
                            break;
                        }
                    case "AND":
                        {
                            _step?.CreateNode<And>(context.StepContext.StepInfo.Text).Fail(context.TestError.Message, mediaEntity);
                            break;
                        }
                }
            }
        }

        private static void SkipStep(ScenarioContext context, string stepType)
        {
            switch (stepType.ToUpper().Trim())
            {
                case "GIVEN":
                    {
                        _step?.CreateNode<Given>(context.StepContext.StepInfo.Text).Skip(stepType.Trim());
                        break;
                    }
                case "WHEN":
                    {
                        _step?.CreateNode<When>(context.StepContext.StepInfo.Text).Skip(stepType.Trim());
                        break;
                    }
                case "THEN":
                    {
                        _step?.CreateNode<Then>(context.StepContext.StepInfo.Text).Skip(stepType.Trim());
                        break;
                    }
                case "AND":
                    {
                        _step?.CreateNode<And>(context.StepContext.StepInfo.Text).Skip(stepType.Trim());
                        break;
                    }
            }
        }
    }
}