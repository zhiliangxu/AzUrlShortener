using Microsoft.WindowsAzure.Storage.Table;

namespace Cloud5mins.domain
{
    public class ShortUrl : TableEntity
    {
        public string Id { get; set; }
        public string Url { get; set; }
    }
}
