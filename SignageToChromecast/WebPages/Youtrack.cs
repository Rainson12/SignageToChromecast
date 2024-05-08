using Microsoft.Extensions.Configuration;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignageToChromecast.WebPages
{
    internal class Youtrack : Website
    {
        public Youtrack(IConfigurationRoot config, INavigation navigator, ChromeDriver driver) : base(config, navigator, driver)
        {
        }

        public override void Initialize()
        {
            navigator.GoToUrl(config["Websites:Youtrack:LoginUrl"]);
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(30));
            wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(By.Id("headerNavigationMenuItems")));
            var html = driver.FindElement(By.TagName("html")).GetAttribute("innerHTML");
            if (html.Contains("Log in..."))
            {
                var loginButton = driver.FindElement(By.XPath("//button[.='Log in...']"));
                loginButton.Click();
                Thread.Sleep(5000);
                wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(By.XPath("//iframe[@title='Login dialog']")));

                driver.SwitchTo().Frame(driver.FindElement(By.XPath("//iframe[@title='Login dialog']")));

                // use Windows Single Sign On
                wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(By.XPath("//div[contains(@class, 'login-page__authmodules__module')]/a")));
                driver.FindElement(By.XPath("//div[contains(@class, 'login-page__authmodules__module')]/a")).Click();
                Thread.Sleep(5000);
            }
        }

        public override void Start()
        {
            navigator.GoToUrl(config["Websites:Youtrack:DashboardUrl"]);
        }
    }
}
