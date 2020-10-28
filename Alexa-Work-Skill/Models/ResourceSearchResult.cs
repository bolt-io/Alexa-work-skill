namespace Alexa_Work_Skill.Models
{
    public struct ResourceSearchResult
    {
        public string SubscriptionId { get; set; }
        public string ResourceId { get; set; }
        public string PowerStateCode { get; set; }
        public string ResourceGroup { get; set; }

        //public IEnumerable<KeyValuePair<string, T> ExtraData {get;set;}
        public ResourceSearchResult(string subscriptionId, string resourceId, string powerStateCode, string resourceGroup)
        {
            this.SubscriptionId = subscriptionId;
            this.ResourceId = resourceId;
            PowerStateCode = powerStateCode;
            ResourceGroup = resourceGroup;
        }
    }
}