// <copyright file="FeedbackModel.cs" company="Analog Devices Inc.">
//     Copyright (c) 2024 Analog Devices Inc. All Rights Reserved.
//     This software is proprietary and confidential to Analog Devices Inc. and its licensors.
// </copyright>

using Microsoft.Extensions.Configuration;

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
