using Microsoft.Extensions.Configuration;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignageToChromecast.WebPages
{
    internal class Otobo : Website
    {
        public Otobo(IConfigurationRoot config, INavigation navigator, ChromeDriver driver) : base(config, navigator, driver)
        {
        }

        public override void Initialize()
        {
            navigator.GoToUrl(config["Websites:Otobo:OtoboUrl"]);
            if (driver.FindElements(By.Id("LoginBox")).Count > 0)
            {
                driver.FindElement(By.Name("User")).SendKeys(config["Websites:Otobo:Username"]);
                driver.FindElement(By.Name("Password")).SendKeys(config["Websites:Otobo:Password"]);
                driver.FindElement(By.Id("LoginButton")).Click();
            }
        }

        public override void Start()
        {
            navigator.GoToUrl(config["Websites:Otobo:OtoboUrl"]);
        }
    }
}
