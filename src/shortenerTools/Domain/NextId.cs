using Microsoft.WindowsAzure.Storage.Table;

namespace Cloud5mins.domain
{
    public class NextId : TableEntity 
    {
        public int Id { get; set; }
    }
}