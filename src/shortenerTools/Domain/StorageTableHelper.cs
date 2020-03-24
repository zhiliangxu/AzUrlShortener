using System.Linq;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Table;

namespace Cloud5mins.domain
{
    public static class StorageTableHelper
    {
        public static async Task<TableResult>FindRowByKeysAsync(string partitionKey, string rowKey, CloudTable tableOut)
        {
            var selOp = TableOperation.Retrieve<ShortUrl>(partitionKey,rowKey);
            return await tableOut.ExecuteAsync(selOp);  
        }

        public static ShortUrl BuildRow(string host, string longUrl, string endUrl){

            var newUrl = new ShortUrl
            {
                PartitionKey = endUrl.First().ToString(),
                RowKey = endUrl,
                Url = longUrl
            };
            return newUrl;
        }
    }
}