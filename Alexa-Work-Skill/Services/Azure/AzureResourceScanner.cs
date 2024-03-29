﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Management.ResourceGraph;
using Microsoft.Azure.Management.ResourceGraph.Models;
using Alexa_Work_Skill.Auth;
using Alexa_Work_Skill.Models;

namespace Alexa_Work_Skill.Services
{
    public sealed class AzureResourceScanner : IAzureResourceScanner
    {
        private readonly ITokenProvider _tokenProvider;
        private readonly HttpClient _httpClient;
        private readonly ILogger<AzureResourceScanner> _log;

        // todo: configuration for national clouds, e.g., https://management.chinacloudapi.cn
        private readonly string _managementEndpoint = "https://management.azure.com/";
        private readonly string _managementAzureAdResourceId = "https://management.azure.com/";
        // todo: configuration to allow/deny specific subscriptions
        private readonly List<string> _subscriptionIds;

        public AzureResourceScanner(ITokenProvider tokenProvider, IHttpClientFactory httpFactory, ILoggerFactory loggerFactory)
        {
            _tokenProvider = tokenProvider;
            _httpClient = httpFactory.CreateClient();
            _log = loggerFactory.CreateLogger<AzureResourceScanner>();
            _subscriptionIds = new List<string>();
        }


        private async Task<IEnumerable<string>> FindAccessibleSubscriptions(bool forceRefresh = false)
        {
            // todo: hack for testing until configurable subscription list is available
            _subscriptionIds.Add("a33a1ff4-7b42-4380-b817-9e48e089a17c"); // msdn
            _subscriptionIds.Add("f00c3ce7-0cd4-49e0-8244-f22a9759c65b"); // access centre dev
            if (!forceRefresh || _subscriptionIds.Any())
            {
                return _subscriptionIds;
            }

            _log.LogTrace($"Getting subscription list starting at ${DateTime.UtcNow}");
            _log.LogTrace($"Getting access token for ${_managementAzureAdResourceId}");

            var token = await _tokenProvider.GetAccessTokenAsync(new[] { _managementAzureAdResourceId }, false);
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Token);

            // resource graph is expecting an array of subscriptions, so get the subscription list first
            var subRequest = await _httpClient.GetAsync($"{_managementEndpoint}subscriptions?api-version=2020-01-01");
            if (!subRequest.IsSuccessStatusCode)
            {
                _log.LogError(new EventId((int)subRequest.StatusCode, subRequest.StatusCode.ToString()), await subRequest.Content.ReadAsStringAsync());
                return _subscriptionIds;
            }

            var data = await JsonDocument.ParseAsync(await subRequest.Content.ReadAsStreamAsync());
            var subscriptionArray = data.RootElement.GetProperty("value").EnumerateArray();
            var subscriptions = subscriptionArray.Select(x => x.GetProperty("subscriptionId").ToString());

            _log.LogTrace($"Got subscription IDs: {string.Join(',', subscriptions)}");
            _subscriptionIds.AddRange(subscriptions);

            return _subscriptionIds;
        }

        private async Task<IEnumerable<ResourceSearchResult>> QueryResourceGraph(string queryText)
        {
            var subscriptions = await FindAccessibleSubscriptions();
            var token = await _tokenProvider.GetAccessTokenAsync(new[] { _managementAzureAdResourceId });
            var graphClient = new ResourceGraphClient(new Microsoft.Rest.TokenCredentials(token.Token));
            var query = await graphClient.ResourcesAsync(new QueryRequest(subscriptions.ToList(), queryText));

            var resources = new List<ResourceSearchResult>();
            // the ResourceGraphClient uses Newtonsoft under the hood
            if (((dynamic)query.Data).rows is Newtonsoft.Json.Linq.JArray j)
            {
                resources.AddRange(
                    j.Select(x => new ResourceSearchResult()
                    {
                        // I'm sure there is a better way here - looking at the columns property, for example, 
                        // to find the position of the column in the row we're interested in - follows query order
                        // so for now, 0, 1, 3, & 4
                        ResourceId = x.ElementAt(0).ToString(),
                        SubscriptionId = x.ElementAt(1).ToString(),
                        PowerStateCode = x.ElementAt(3).ToString(),
                        ResourceGroup = x.ElementAt(4).ToString()
                    }));
            }

            return resources;
        }
        //resources | where (isnotnull(tags.['use'])) and type == 'microsoft.compute/virtualmachines' | project name, subscriptionId, id, properties.extended.instanceView.powerState.code, resourceGroup
        public Task<IEnumerable<ResourceSearchResult>> ScanForDailyWorkResources()
        {
            var taggedQuery = @"resources | where (isnotnull(tags.['use'])) 
                                                       and type == 'microsoft.compute/virtualmachines'
                                                       and tags['use'] == 'DailyWork'
                                                     | project name, subscriptionId, id, properties.extended.instanceView.powerState.code, resourceGroup";
            return QueryResourceGraph(taggedQuery);
        }

        public Task<IEnumerable<ResourceSearchResult>> ScanForIllegalResourceGroups(string? subcriptionId = null)
        {
            
            var illegalResourceGroupQuery = string.Format(@"ResourceContainers | where type == 'microsoft.resources/subscriptions/resourcegroups'
                                                       and (isempty(tags)
                                                       or isnull(tags['owner'])
                                                       or isnull(tags['purpose'])
                                                       or isnull(tags['lifecycle'])
                                                       or isnull(tags['environment231']))
                                                       and (subscriptionId == '{0}')
                                                     | project name, subscriptionId, id, resourceGroup", subcriptionId);

            _log.LogTrace($"Querying for illegal resource groups: {illegalResourceGroupQuery}");
            return QueryResourceGraph(illegalResourceGroupQuery);
        }


    }
}
