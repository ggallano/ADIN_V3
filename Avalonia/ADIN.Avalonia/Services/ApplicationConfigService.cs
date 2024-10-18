using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADIN.Avalonia.Services
{
    public class ApplicationConfigService
    {
        IConfiguration config;
        IConfigurationSection section;

        public ApplicationConfigService()
        {
            config = new ConfigurationBuilder()
                .AddJsonFile("appConfig.json", optional: false, reloadOnChange: true)
                .Build();

            section = config.GetSection("Settings");
        }

        public string GetConfigValue(string key)
        {
            return section[key];
        }
    }
}
