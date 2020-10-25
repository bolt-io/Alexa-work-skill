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

namespace Alexa_Start_Work
{
    public static class AlexaEndpoint // get tags for "work resource" and start them.
    {
        [FunctionName("AlexaEndpoint")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            string json = await req.ReadAsStringAsync();
            var skillRequest = JsonConvert.DeserializeObject<SkillRequest>(json);

            var requestType = skillRequest.GetRequestType();

            SkillResponse response = null;

            //if (requestType == typeof(LaunchRequest))
            {
                response = ResponseBuilder.Tell("Don't anger me!!");
                response.Response.ShouldEndSession = false;
            }

            return new OkObjectResult(response);
        }
    }
}
