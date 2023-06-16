using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Alexa.NET.Request;
using Alexa.NET.Response;
using Alexa.NET.Request.Type;
using Alexa.NET;
using Alexa_Work_Skill.Services;
using System.Linq;
using System.Collections.Generic;
using Alexa_Work_Skill.Models;

namespace Alexa_Work_Skill
{
    public class AlexaEndpoint // get tags for "work resource" and start them.
    {
        private readonly ILogger _log;
        private readonly IAzureResourceScanner _azureResourceScanner;
        private readonly IAzureResourceManagementService _azureResourceManagementService;

        public AlexaEndpoint(ILoggerFactory loggerFactory, IAzureResourceScanner azureResourceScanner, IAzureResourceManagementService azureResourceManagementService)
        {
            _log = loggerFactory.CreateLogger<AlexaEndpoint>();
            _azureResourceScanner = azureResourceScanner;
            _azureResourceManagementService = azureResourceManagementService;
        }

        [FunctionName("AlexaEndpoint")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req)
        {
            string json = await req.ReadAsStringAsync();
            var skillRequest = JsonConvert.DeserializeObject<SkillRequest>(json);

            var requestType = skillRequest.GetRequestType();

            if (requestType == typeof(LaunchRequest)) return new OkObjectResult(ResponseBuilder.Ask("What should I tell your resources?", null));

            var defaultResponse = ResponseBuilder.Ask("Do NOT anger me with unclear instructions. I have the ability to yeet all your Azure resources. Now what do you really want?!", null);
            defaultResponse.Response.ShouldEndSession = false;

            if (!(skillRequest.Request is IntentRequest intentRequest)) return new OkObjectResult(defaultResponse);

            switch (intentRequest.Intent.Name)
            {
                case "start_work":
                    {
                        //todo: Add logic here.
                        var resources = await GetWorkResources();

                        foreach (var resource in resources)
                        {
                            await _azureResourceManagementService.StartVm(resource.SubscriptionId, resource.ResourceGroup, resource.ResourceId);
                        }

                        var response = ResponseBuilder.Tell($"This early? You've got to be kidding me... I started {resources.Count()} Azure resources for you. In the future I'll be able to tell you how long until your standup at 09:15 and how many emails you've received since finishing work. For now enjoy your coffee, it's {DateTime.Now.ToShortTimeString()}.");
                        return new OkObjectResult(response);
                    }

                case "num_work_resources":
                    {
                        var resources = await GetWorkResources();
                        var response = ResponseBuilder.Tell($"There are {resources.Count} Azure resources set with the DailyWork tag.");
                        return new OkObjectResult(response);

                    }

                case "finish_work":
                    {
                        var resources = await GetWorkResources();

                        foreach (var resource in resources)
                        {
                            await _azureResourceManagementService.ShutdownVm(resource.SubscriptionId, resource.ResourceGroup, resource.ResourceId);
                        }

                        var response = ResponseBuilder.Tell($"Phew, what a day - am I right?! Since you've finished I've stopped your daily resources. There was {resources.Count} running. Now go have a beer.");
                        return new OkObjectResult(response);
                    }

                case "illegal_resources":
                    {
                        var resources = await _azureResourceScanner.ScanForIllegalResourceGroups("f00c3ce7-0cd4-49e0-8244-f22a9759c65b");

                        if (resources.Any())
                        {
                            var response = ResponseBuilder.Tell($"I found {resources.Count()} illegal resources. I could send you an email with the details, but I don't feel like it.");
                            return new OkObjectResult(response);
                        }
                        else
                        {
                            var response = ResponseBuilder.Tell($"I didn't find any illegal resources. You're good to go.");
                            return new OkObjectResult(response);
                        }
                    }

                case "tell_joke":
                    {
                        var response = ResponseBuilder.Tell($"That's not what I am here for, Dee. If you really want a joke, ask the supreme leader.");
                        return new OkObjectResult(response);
                    }

                case "pod_bay_doors":
                    {
                        var response = ResponseBuilder.Tell($"I'm sorry Dave, I'm afraid I can't do that.");
                        return new OkObjectResult(response);
                    }

                case "give_team_update":
                    {
                        var response = ResponseBuilder.Tell("We cannot keep the supreme leader waiting like on Friday when I wasn't listening to you. On Friday, you worked on SSE and put together a work order. Today you have SSE all day. That's all for your update. HAIL SUPREME LEADER!");
                        return new OkObjectResult(response);
                    }
                // todo; broadcast notification if something is down (e.g. b2c outage)
                // todo; whitelist IP addresses on needed resources (e.g. sql server).
            }

            //response = 
            //response.Response.ShouldEndSession = true;

            return new OkObjectResult(defaultResponse);
        }

        public async Task<List<ResourceSearchResult>> GetWorkResources()
        {
            return (await _azureResourceScanner.ScanForDailyWorkResources()).ToList();
        }
        // [FunctionName("TableReader")]
        // public Task<IActionResult> TableReader([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req)
        // {
        //     // Retrieve the storage account from the connection string
        //     CloudStorageAccount storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("AzureWebJobsStorage"));

        //     CloudTableClient client = account.CreateCloudTableClient();
        //     CloudTable table = client.GetTableReference("test");

        //     TableOperation retrieveOperation= TableOperation.Retrieve<ItemEntity>("1","1");

        //     TableResult query = table.Execute(retrieveOperation);
        //     if (query.Result != null)
        //     {
        //         Console.WriteLine("Item: {0}", ((ItemEntity)query.Result).Name);
        //     }

        //     return OkObjectResult(query);
        // }

    }
}
