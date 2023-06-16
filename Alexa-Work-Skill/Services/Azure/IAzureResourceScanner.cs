
using System.Collections.Generic;
using System.Threading.Tasks;
using Alexa_Work_Skill.Models;

namespace Alexa_Work_Skill.Services
{
    public interface IAzureResourceScanner
    {
        Task<IEnumerable<ResourceSearchResult>> ScanForDailyWorkResources();
        Task<IEnumerable<ResourceSearchResult>> ScanForIllegalResourceGroups(string? subscriptionId = null);

    }
}