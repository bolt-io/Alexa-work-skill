
using System.Threading.Tasks;

namespace Alexa_Work_Skill.Services
{
    public interface IAzureResourceManagementService
    {
        Task<string> ShutdownVm(string subscriptionId, string resourceGroupName, string vmName);
        Task<string> StartVm(string subscriptionId, string resourceGroupName, string vmName);
    }
}