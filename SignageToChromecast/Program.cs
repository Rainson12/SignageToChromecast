using Microsoft.Extensions.Configuration;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using Sharpcaster.Interfaces;
using Sharpcaster;
using System.Diagnostics;
using SignageToChromecast.WebPages;

namespace SignageToChromecast
{    
    internal class Program
    {
        static int SceenWidth = 1280;
        static int ScreenHeight = 720;
        static ChromeDriver driver;

        public static async Task Main(string[] args)
        {
            //Process[] chromeInstances = Process.GetProcessesByName("chrome");

            //foreach (Process p in chromeInstances)
            //    p.Kill();

            AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;
            var environment = Environment.GetEnvironmentVariable("NETCORE_ENVIRONMENT");
            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddJsonFile($"appsettings.{environment}.json", optional: true)
                .AddUserSecrets<Program>();
            var config = builder.Build();
            
            try
            {
                IChromecastLocator locator = new MdnsChromecastLocator();
                var chromecasts = await locator.FindReceiversAsync();
                if (chromecasts.Any(x => x.Name == config["ChromecastName"]))
                {
                    var chromecast = chromecasts.First(x => x.Name == config["ChromecastName"]);
                    var client = new ChromecastClient();
                    await client.ConnectChromecast(chromecast);
                    var chromecastStatus = client.GetChromecastStatus();
                    if (chromecastStatus.IsStandBy || chromecastStatus.Applications.Count > 0 && chromecastStatus.Applications[0].DisplayName == "Backdrop") // nothing is currently running so we can start rotation
                    {
                        var options = new ChromeOptions();
                        options.AcceptInsecureCertificates = true;

                        driver = new ChromeDriver(options);

                        var navigator = driver.Navigate();
                        SetBrowserWindowSize();

                        var services = new List<Website>
                        {
                            new Nagios(config, navigator, driver),
                            new Otobo(config, navigator, driver),
                            new AwsOpenSearch(config, navigator, driver),
                            new Youtrack(config, navigator, driver)
                        };
                        
                        foreach(var service in services)
                        {
                            service.Initialize();
                        }
                        while (true)
                        {
                            // if not connected to chromecast, try to connect
                            await client.ConnectChromecast(chromecast);
                            chromecastStatus = client.GetChromecastStatus();
                            if (chromecastStatus.IsStandBy || chromecastStatus.Applications.Count > 0 && chromecastStatus.Applications[0].DisplayName == "Backdrop") // nothing is currently running so we can start rotation
                            {
                                MirrorToChromeCast(config["ChromecastName"]);
                            }

                            if (DateTime.Now > new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, int.Parse(config["RunUntilHour"]), int.Parse(config["RunUntilMinute"]), 0))
                            {
                                // close app 
                                return;
                            }

                            foreach (var service in services)
                            {
                                try
                                {
                                    service.Start();
                                    Thread.Sleep(service.ScreenTimeInSeconds * 1000);
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine(ex.Message);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
                Console.WriteLine(ex.Message);

                File.WriteAllText("stack.txt", ex.StackTrace);
                File.WriteAllText("error.txt", ex.Message);
            }
        }

        private static void SetBrowserWindowSize()
        {
            driver.Manage().Window.Size = new System.Drawing.Size(SceenWidth, ScreenHeight);

            var content = driver.FindElement(By.TagName("html"));
            var inner_width = int.Parse(content.GetAttribute("clientWidth"));
            var inner_height = int.Parse(content.GetAttribute("clientHeight"));

            var offsetWidth = SceenWidth - inner_width;
            var offsetHeight = ScreenHeight - inner_height;

            driver.Manage().Window.Size = new System.Drawing.Size(SceenWidth + offsetWidth, ScreenHeight + offsetHeight);
        }

        private static void MirrorToChromeCast(string chromeCastName)
        {
            do
            {
                Console.WriteLine("Waiting for Chromecast to be available. Current Cast Sinks:" + driver.GetCastSinks().Count);
                if (driver.GetCastSinks().Count > 0)
                    Console.WriteLine("sinks: " + driver.GetCastSinks().SelectMany(x => x.Keys).ToList().Aggregate((x, y) => x + ", " + y));

                Thread.Sleep(100);
            }
            while (driver.GetCastSinks().Count == 0);
            driver.StartTabMirroring(chromeCastName);
        }

        private static void CurrentDomain_ProcessExit(object? sender, EventArgs e)
        {
            if (driver != null)
            {
                driver.Close();
                driver.Quit();
            }
        }
    }
}
