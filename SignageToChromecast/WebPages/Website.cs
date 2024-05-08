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
    internal abstract class Website
    {
        internal readonly IConfigurationRoot config;
        internal readonly INavigation navigator;
        internal readonly ChromeDriver driver;
        public Website(IConfigurationRoot config, INavigation navigator, ChromeDriver driver)
        {
            this.config = config;
            this.navigator = navigator;
            this.driver = driver;
        }
        public abstract void Initialize();
        public abstract void Start();
        public int ScreenTimeInSeconds { get => 30; }
    }
}
