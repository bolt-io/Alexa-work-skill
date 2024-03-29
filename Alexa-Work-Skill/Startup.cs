using System;
using System.IO;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Alexa_Work_Skill.Auth;
using Alexa_Work_Skill.Services;

[assembly: FunctionsStartup(typeof(Alexa_Work_Skill.Startup))]

namespace Alexa_Work_Skill
{
    public class Startup : FunctionsStartup
    {
        public override void ConfigureAppConfiguration(IFunctionsConfigurationBuilder builder)
        {
            FunctionsHostBuilderContext context = builder.GetContext();

            // builder.ConfigurationBuilder
            //     .AddJsonFile(Path.Combine(context.ApplicationRootPath, "appsettings.json"), optional: true, reloadOnChange: false)
            //     .AddJsonFile(Path.Combine(context.ApplicationRootPath, $"appsettings.{context.EnvironmentName}.json"), optional: true, reloadOnChange: false)
            //     //.AddAzureAppConfiguration(Environment.GetEnvironmentVariable("AZMAN-AAC-CONNECTION"), optional: true)
            //     .AddEnvironmentVariables();
        }

        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddHttpClient();
            // if (builder.GetContext().EnvironmentName == "Development")
            // {
            //     builder.Services.AddSingleton<ITokenProvider, AzureCliTokenProvider>();
            // }
            // else
            //{
            builder.Services.AddSingleton<ITokenProvider, AzureManagedIdentityServiceTokenProvider>();
            //}
            builder.Services.AddSingleton<IAzureResourceManagementService, AzureResourceManagementService>();
            builder.Services.AddSingleton<IAzureResourceScanner, AzureResourceScanner>();
        }
    }
}