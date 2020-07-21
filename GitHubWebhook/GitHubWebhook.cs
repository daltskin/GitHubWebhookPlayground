using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace GitHubWebhook
{
    public static class GitHubWebhook
    {
        [FunctionName("GitHubWebhook")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req, ILogger log)
        {
            log.LogInformation("C# Webhook for GitHub.");
            string requestBody = null;

            if (!Debugger.IsAttached)
            {
                if (!req.Headers.ContainsKey("X-Hub-Signature"))
                {
                    log.LogError("Missing signature");
                    return new BadRequestObjectResult("Missing signature");
                }

                string signature = req.Headers["X-Hub-Signature"].FirstOrDefault();
                requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                string secret = Environment.GetEnvironmentVariable("Secret");
                if (!CheckSignature.Validate(signature, requestBody, secret))
                {
                    log.LogError($"Signatures don't match");
                    return new BadRequestObjectResult("Signatures don't match");
                }
                log.LogInformation($"Signature check success");
            }

            requestBody ??= await new StreamReader(req.Body).ReadToEndAsync();
            CheckSuiteStatus suiteStatus = JsonConvert.DeserializeObject<CheckSuiteStatus>(requestBody);

            string responseMessage = $"The GitHub Check Suite status for {suiteStatus.check_suite.id} {suiteStatus.action} with {suiteStatus.check_suite.conclusion}";
            return new OkObjectResult(responseMessage);
        }
    }
}
