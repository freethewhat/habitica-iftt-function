using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace habitica_iftt_function
{
    public static class HabiticaTodo
    {
        [FunctionName("HabiticaTodo")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            // Get app API Key and url pareameters
            var appApiKey = System.Environment.GetEnvironmentVariable("FUNCTION_API_KEY");
            string title = req.Query["title"];
            string apiKey = req.Query["key"];

            // Test title and API key
            if (string.IsNullOrEmpty(title)) return new BadRequestObjectResult("title was not provided.");

            if (string.IsNullOrEmpty(apiKey)) return new BadRequestObjectResult("API key was not provided.");

            if (!apiKey.Equals(appApiKey)) return new UnauthorizedResult();

            log.LogInformation($"Attempting to create new Habitica todo: ${title}");
            var result = await CreateTodo(title);

            return new OkObjectResult(result);
        }

        private static async Task<string> CreateTodo(string title)
        {
            // Get base URL and Habitica API auth info
            var baseUrl = "https://habitica.com/api/v3/tasks/user";
            var userId = Environment.GetEnvironmentVariable("HABITICA_USER_ID");
            var apiKey = Environment.GetEnvironmentVariable("HABITICA_API_KEY");

            // Create client and add auth headers.
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("x-api-user", userId);
            client.DefaultRequestHeaders.Add("x-api-key", apiKey);

            // Create body content
            var value = new Dictionary<string, string>
            {
                { "text", title },
                { "type", "todo" }
            };
            var json = JsonConvert.SerializeObject(value);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            // Make response to Habitica API.
            var response = await client.PostAsync(baseUrl, content);
            return await response.Content.ReadAsStringAsync();
        }
    }
}

