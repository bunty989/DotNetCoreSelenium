using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using Serilog;
using Browser = NSWEHealth.Framework.Wrapper.TestConstant.BrowserType;

namespace NSWEHealth.Framework.Drivers
{
    internal class BrowserVersionHelper
    {
        public static string? GetBrowserVersion(Browser browserName)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                string? exePath = browserName switch
                {
                    Browser.Chrome or Browser.ChromeHeadless or Browser.ChromeIncognito =>
                        Registry.GetValue(
                            @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\chrome.exe", "",
                            "C:\\Program Files\\Google\\Chrome\\Application\\chrome.exe")?.ToString(),
                    Browser.Firefox or Browser.FirefoxHeadless =>
                        Registry.GetValue(
                            @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\firefox.exe", "",
                            "C:\\Program Files\\Mozilla Firefox\\firefox.exe")?.ToString(),
                    Browser.Edge or Browser.EdgeHeadless =>
                        Registry.GetValue(
                            @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\msedge.exe", "",
                            "C:\\Program Files (x86)\\Microsoft\\Edge\\Application\\msedge.exe")?.ToString(),
                    Browser.InternetExplorer =>
                        Registry.GetValue(
                            @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\iexplore.exe", "",
                            "C:\\Program Files (x86)\\Internet Explorer\\iexplore.exe")?.ToString(),
                    Browser.Safari => null,
                    _ => null
                };
                if (!string.IsNullOrEmpty(exePath) && File.Exists(exePath))
                {
                    return FileVersionInfo.GetVersionInfo(exePath).FileVersion;
                }
                return null;
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                string userAppDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "/Applications";
                string? appBundlePath = null;

                switch (browserName)
                {
                    case Browser.Chrome:
                    case Browser.ChromeHeadless:
                    case Browser.ChromeIncognito:
                        appBundlePath = new[] {
                            "/Applications/Google Chrome.app",
                            userAppDir + "/Google Chrome.app"
                        }.FirstOrDefault(Directory.Exists);
                        break;
                    case Browser.Firefox:
                    case Browser.FirefoxHeadless:
                        appBundlePath = new[] {
                            "/Applications/Firefox.app",
                            userAppDir + "/Firefox.app"
                        }.FirstOrDefault(Directory.Exists);
                        break;
                    case Browser.Edge:
                    case Browser.EdgeHeadless:
                        appBundlePath = new[] {
                            "/Applications/Microsoft Edge.app",
                            userAppDir + "/Microsoft Edge.app"
                        }.FirstOrDefault(Directory.Exists);
                        break;
                    case Browser.Safari:
                        appBundlePath = "/Applications/Safari.app";
                        break;
                }

                if (browserName == Browser.Safari)
                {
                    try
                    {
                        var process = new Process
                        {
                            StartInfo = new ProcessStartInfo
                            {
                                FileName = "defaults",
                                Arguments = "read /Applications/Safari.app/Contents/Info CFBundleShortVersionString",
                                RedirectStandardOutput = true,
                                UseShellExecute = false,
                                CreateNoWindow = true
                            }
                        };
                        process.Start();
                        string? output = process.StandardOutput.ReadLine();
                        process.WaitForExit();

                        if (!string.IsNullOrEmpty(output))
                        {
                            return output.Trim();
                        }
                    }
                    catch
                    {
                        Log.Error("Error occurred while trying to get the Safari version. Please check if Safari is installed and accessible.");
                    }
                    return null;
                }

                if (!string.IsNullOrEmpty(appBundlePath) && Directory.Exists(appBundlePath))
                {
                    try
                    {
                        var process = new Process
                        {
                            StartInfo = new ProcessStartInfo
                            {
                                FileName = "mdls",
                                Arguments = $"-name kMDItemVersion \"{appBundlePath}\"",
                                RedirectStandardOutput = true,
                                UseShellExecute = false,
                                CreateNoWindow = true
                            }
                        };
                        process.Start();
                        string? output = process.StandardOutput.ReadToEnd();
                        process.WaitForExit();

                        // Output: kMDItemVersion = "124.0"
                        if (!string.IsNullOrEmpty(output))
                        {
                            var match = System.Text.RegularExpressions.Regex.Match(output, "\"([0-9.]+)\"");
                            if (match.Success)
                                return match.Groups[1].Value;
                        }
                    }
                    catch
                    {
                        Log.Error($"Error occurred while trying to get the {browserName} version using mdls. Please check if the browser is installed and accessible.");
                    }
                }

                return null;
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                string? browserCmd = null;
                string? arguments = "--version";

                if (browserName == Browser.Chrome || browserName == Browser.ChromeHeadless || browserName == Browser.ChromeIncognito)
                {
                    browserCmd = "google-chrome";
                }
                else if (browserName == Browser.Firefox || browserName == Browser.FirefoxHeadless)
                {
                    browserCmd = "firefox";
                }
                else if (browserName == Browser.Edge || browserName == Browser.EdgeHeadless)
                {
                    // Try common Edge binary names and use the full path
                    string[] edgeCandidates = { "microsoft-edge", "microsoft-edge-stable", "edge" };
                    foreach (var candidate in edgeCandidates)
                    {
                        try
                        {
                            var whichProcess = new Process
                            {
                                StartInfo = new ProcessStartInfo
                                {
                                    FileName = "which",
                                    Arguments = candidate,
                                    RedirectStandardOutput = true,
                                    UseShellExecute = false,
                                    CreateNoWindow = true
                                }
                            };
                            whichProcess.Start();
                            string? path = whichProcess.StandardOutput.ReadLine();
                            whichProcess.WaitForExit();
                            if (!string.IsNullOrEmpty(path) && File.Exists(path.Trim()))
                            {
                                browserCmd = path.Trim();
                                break;
                            }
                        }
                        catch
                        {
                            // Ignore and try next candidate
                        }
                    }
                }

                if (string.IsNullOrEmpty(browserCmd))
                    return null;

                try
                {
                    var process = new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = browserCmd,
                            Arguments = arguments,
                            RedirectStandardOutput = true,
                            UseShellExecute = false,
                            CreateNoWindow = true
                        }
                    };
                    process.Start();
                    string? output = process.StandardOutput.ReadToEnd();
                    process.WaitForExit();

                    // Output is like "Microsoft Edge 123.0.2420.53" or "Mozilla Firefox 124.0"
                    if (!string.IsNullOrEmpty(output))
                    {
                        var parts = output.Split(new[] { ' ', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (var part in parts)
                        {
                            if (Version.TryParse(part, out _))
                                return part;
                        }
                    }
                }
                catch
                {
                    Log.Error("Error occurred while trying to get the browser version. Please check if the browser is installed and accessible.");
                }
                return null;
            }
            else
            {
                Log.Error("Unsupported OS platform. This method only supports Windows, Linux, and macOS.");
                return null;
            }
        }
    }
}