using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http;
using System.Linq;
using Microsoft.WindowsAzure.Storage.Table;
using Cloud5mins.domain;
using Microsoft.Azure.WebJobs.Host.Config;
using Microsoft.Extensions.Hosting;

namespace Cloud5mins.Function
{
    public static class UrlShortener
    {

        static async Task Main()
        {
            var builder = new HostBuilder();
            //builder.UseEnvironment("development");
            builder.ConfigureWebJobs(b =>
                    {
                        b.AddAzureStorage();
                        b.AddHttp();
                    });
            var host = builder.Build();
            using (host)
            {
                await host.RunAsync();
            }
        }


        [FunctionName("UrlShortener")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestMessage req,
            [Table("UrlsDetails", "1", "KEY", Take = 1)]NextId keyTable,
            [Table("UrlsDetails")]CloudTable tableOut,
            ILogger log)
        {
            log.LogInformation($"C# HTTP trigger function processed this request: {req}");

            // Validation of the inputs
            if (req == null)
            {
                return req.CreateResponse(HttpStatusCode.NotFound);
            }

            ShortRequest input = await req.Content.ReadAsAsync<ShortRequest>();
            if (input == null)
            {
                return req.CreateResponse(HttpStatusCode.NotFound);
            }

            if (keyTable == null)
            {
                keyTable = new NextId
                {
                    PartitionKey = "1",
                    RowKey = "KEY",
                    Id = 1024
                };
                var keyAdd = TableOperation.Insert(keyTable);
                await tableOut.ExecuteAsync(keyAdd); 
            }
            
            var result = new ShortResponse();

            try
            {
                ShortUrl newRow;
                string longUrl = input.Url.Trim();
                string vanity = input.Vanity.Trim();

                var host = req.RequestUri.GetLeftPart(UriPartial.Authority);
                string endUrl = Utility.GetValidEndUrl(vanity, keyTable.Id++);

                //log.LogInformation($"host={host} endUrl={endUrl}");

                result = Utility.BuildResponse(host, longUrl, endUrl);
                newRow = StorageTableHelper.BuildRow(host, longUrl, endUrl);
          

                async Task saveKeyAsync()
                {
                    var updOp = TableOperation.Replace(keyTable);
                    await tableOut.ExecuteAsync(updOp);
                }

                async Task<bool> CheckIfExistRowAsync(){
                    var rowCheck = await StorageTableHelper.FindRowByKeysAsync(newRow.PartitionKey, newRow.RowKey, tableOut);  
                    return(rowCheck.HttpStatusCode == (int)HttpStatusCode.OK);
                }

                async Task saveRowAsync()
                {
                    var insOp = TableOperation.Insert(newRow);
                    var operationResult = await tableOut.ExecuteAsync(insOp);  
                }

                //await buildOutputs();
                await saveKeyAsync();

                if(await CheckIfExistRowAsync())
                    return req.CreateResponse(HttpStatusCode.Conflict, "This Short URL already exist.");
                else
                    await saveRowAsync();

                log.LogInformation("Short Url created.");
            }
            catch (Exception ex)
            {
                log.LogError(ex, "An unexpected error was encountered.");
                return req.CreateErrorResponse(HttpStatusCode.BadRequest, ex);
            }

            return req.CreateResponse(HttpStatusCode.OK, result);
        }
    }
}
