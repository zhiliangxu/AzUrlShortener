using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using System.Net.Http.Headers;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Linq;
using System.Net;
using System.Net.Http;
using Cloud5mins.domain;
using Microsoft.WindowsAzure.Storage.Table;

namespace Cloud5mins.Function
{
    public static class UrlRedirect
    {
        [FunctionName("UrlRedirect")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = "UrlRedirect/{shortUrl}")] HttpRequestMessage req,
            [Table(tableName: "UrlsDetails")]CloudTable inputTable,
            string shortUrl,
            ILogger log)
        {
            log.LogInformation($"C# HTTP trigger function processed for Url: {shortUrl}");

            var redirectUrl = "http://azure.com";

            if (!String.IsNullOrWhiteSpace(shortUrl))
            {
                var partitionKey = shortUrl.First().ToString();
                TableResult result = await StorageTableHelper.FindRowByKeysAsync(partitionKey, shortUrl, inputTable);

                if (result.Result is ShortUrl fullUrl)
                {
                    log.LogInformation($"Found it: {fullUrl.Url}");
                    redirectUrl = WebUtility.UrlDecode(fullUrl.Url);
                }
            }
            else
            {
                log.LogInformation("Bad Link, resorting to fallback.");
            }

            var res = req.CreateResponse(HttpStatusCode.Redirect);
            res.Headers.Add("Location", redirectUrl);
            return res;
        }
    }
}
