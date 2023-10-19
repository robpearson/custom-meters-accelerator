using Newtonsoft.Json;

namespace ManagedApplicationScheduler.Services.Models
{
    /// <summary>
    /// The Meter details definition
    /// </summary>
    public class BillingDetailsDefinitionModel
    {
        /// <summary>
        /// Gets or sets the resource usage id.
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public string ResourceUsageId { get; set; }
    }
}
