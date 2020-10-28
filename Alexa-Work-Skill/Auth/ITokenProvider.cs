using System.Threading.Tasks;

namespace Alexa_Work_Skill.Auth
{
    public interface ITokenProvider
    {
        AccessTokenResponse GetAccessToken(string[] scopes, bool forceRefresh = false);
        Task<AccessTokenResponse> GetAccessTokenAsync(string[] scopes, bool forceRefresh = false);
    }
}