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
    internal class AwsOpenSearch : Website
    {
        public AwsOpenSearch(IConfigurationRoot config, INavigation navigator, ChromeDriver driver) : base(config, navigator, driver)
        {
        }

        public override void Initialize()
        {
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(60));
            navigator.GoToUrl(config["Websites:AWS:LoginUrl"]);
            string html = "";
            do
            {
                try
                {
                    wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(By.TagName("html")));
                    html = driver.FindElement(By.TagName("html")).GetAttribute("innerHTML");
                    Thread.Sleep(100);
                }
                catch
                {

                }
                // page is still loading and validating credentials
            }
            while (!html.Contains("ConvergedSignIn") && !html.Contains(">Applications<"));

            bool autoSignIn = true; // SSO
            if (autoSignIn)
            {
                wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(By.XPath("//span[ contains (text(),'Applications')]")));
            }
            else if (html.Contains("ConvergedSignIn"))
            {
                // login required
                wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(By.Name("loginfmt")));
                driver.FindElement(By.Name("loginfmt")).SendKeys(config["Websites:AWS:Username"]);
                Thread.Sleep(100);
                driver.FindElement(By.XPath("//input[@type='submit']")).Click();

                wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(By.Name("passwd")));
                driver.FindElement(By.Name("passwd")).SendKeys(config["Websites:AWS:Password"]);
                driver.FindElement(By.XPath("//input[@type='submit']")).Click();
                wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(By.Id("idSIButton9")));
                driver.FindElement(By.XPath("//input[@type='submit']")).Click();
                Console.WriteLine("Waiting for login to complete");
                wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(By.TagName("portal-application-list")));
                Console.WriteLine("Login completed");
            }
            navigator.GoToUrl(config["Websites:AWS:OpenSearchUrl"]);
            Thread.Sleep(10 * 1000);
        }

        public override void Start()
        {
            navigator.GoToUrl(config["Websites:AWS:OpenSearchDashboardUrl"]);
        }
    }
}
