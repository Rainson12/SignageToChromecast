using Microsoft.Extensions.Configuration;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SignageToChromecast.WebPages
{
    internal class Nagios : Website
    {
        public Nagios(IConfigurationRoot config, INavigation navigator, ChromeDriver driver) : base(config, navigator, driver)
        {
        }

        public override void Initialize()
        {
            navigator.GoToUrl(config["Websites:Nagios:LoginUrl"]);
            if (driver.FindElements(By.Id("login_window")).Count > 0)
            {
                driver.FindElement(By.Name("_username")).SendKeys(config["Websites:Nagios:Username"]);
                driver.FindElement(By.Name("_password")).SendKeys(config["Websites:Nagios:Password"]);
                driver.FindElement(By.Id("_login")).Click();
            }
        }

        public override void Start()
        {
            navigator.GoToUrl(config["Websites:Nagios:DashboardUrl"]);
        }

    }
}
