using System;
using System.Net.Http;
using System.Threading.Tasks;
using Alexa_Work_Skill.Auth;
using Microsoft.Extensions.Logging;

namespace Alexa_Work_Skill.Services
{
    public sealed class AzureResourceManagementService : IAzureResourceManagementService
    {

        private readonly HttpClient _httpClient;
        private readonly ILogger<AzureResourceManagementService> _log;
        private readonly ITokenProvider _tokenProvider;
        private readonly Azure.Core.TokenCredential _tokenCredential;
        public AzureResourceManagementService(IHttpClientFactory httpFactory, ILoggerFactory loggerFactory, ITokenProvider tokenProvider)
        {
            _httpClient = httpFactory.CreateClient();
            _log = loggerFactory.CreateLogger<AzureResourceManagementService>();
            _tokenProvider = tokenProvider;
            _tokenCredential = new ExternalAzureTokenCredential(_tokenProvider);
        }

        public async Task<string> StartVm(string subscriptionId, string resourceGroupName, string vmName)
        {
            var token = await _tokenProvider.GetAccessTokenAsync(new[] { "https://management.azure.com/" });
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token.Token);
            var exportUri = new Uri($"https://management.azure.com/subscriptions/{subscriptionId}/resourceGroups/{resourceGroupName}/providers/Microsoft.Compute/virtualMachines/{vmName}/start?api-version=2020-06-01");

            var request = await _httpClient.PostAsync(exportUri, null);
            if (!request.IsSuccessStatusCode) return string.Empty;

            var templateData = await request.Content.ReadAsStringAsync();
            _log.LogTrace($"VmStart request response: {templateData}");
            return templateData;

            // POST https://management.azure.com/subscriptions/{subscriptionId}/resourcegroups/{resourceGroupName}/exportTemplate?api-version=2020-06-01
            // todo: tweak based on output and ease of re-deploy
            // var resourceManagerClient = new ResourcesManagementClient(subscriptionId, _tokenCredential);
            // var exportedTemplate = await resourceManagerClient.ResourceGroups.StartExportTemplateAsync(groupName,
            // // todo: this is required but read-only? hmm
            // new Azure.ResourceManager.Resources.Models.ExportTemplateRequest() { Resources = new[] { "*" } });
            // new Azure.ResourceManager.Resources.Models.ExportTemplateRequest() { });
            // if (exportedTemplate.HasValue) return (string)exportedTemplate.Value.Template; // todo: y tho?
            // return string.Empty;
        }


        public async Task<string> ShutdownVm(string subscriptionId, string resourceGroupName, string vmName)
        {
            var token = await _tokenProvider.GetAccessTokenAsync(new[] { "https://management.azure.com/" });
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token.Token);
            var exportUri = new Uri($"https://management.azure.com/subscriptions/{subscriptionId}/resourceGroups/{resourceGroupName}/providers/Microsoft.Compute/virtualMachines/{vmName}/powerOff?api-version=2020-06-01");

            var request = await _httpClient.PostAsync(exportUri, null);
            if (!request.IsSuccessStatusCode) return string.Empty;

            var templateData = await request.Content.ReadAsStringAsync();
            _log.LogTrace($"VmStart request response: {templateData}");
            return templateData;

            // POST https://management.azure.com/subscriptions/{subscriptionId}/resourcegroups/{resourceGroupName}/exportTemplate?api-version=2020-06-01
            // todo: tweak based on output and ease of re-deploy
            // var resourceManagerClient = new ResourcesManagementClient(subscriptionId, _tokenCredential);
            // var exportedTemplate = await resourceManagerClient.ResourceGroups.StartExportTemplateAsync(groupName,
            // // todo: this is required but read-only? hmm
            // new Azure.ResourceManager.Resources.Models.ExportTemplateRequest() { Resources = new[] { "*" } });
            // new Azure.ResourceManager.Resources.Models.ExportTemplateRequest() { });
            // if (exportedTemplate.HasValue) return (string)exportedTemplate.Value.Template; // todo: y tho?
            // return string.Empty;
        }

    }
}