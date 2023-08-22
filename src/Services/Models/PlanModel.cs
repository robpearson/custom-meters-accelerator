using Newtonsoft.Json;

namespace ManagedApplicationScheduler.Services.Models
{
    /// <summary>
    /// The marketplace plan.
    /// </summary>
    public class PlanModel
    {
        /// <summary>
        /// Gets or sets the publisher.
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public string Publisher { get; set; }

        /// <summary>
        /// Gets or sets the product (offer ID).
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public string Product { get; set; }//This is Offer ID

        /// <summary>
        /// Gets or sets the SKU or PlanID.
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public string Name { get; set; } // This is planID

        /// <summary>
        /// Gets or sets the plan version.
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public string Version { get; set; }


        public string PlanName { get; set; }

        public string ProductName { get; set; }

        public string Dimension { get; set; }

        public string id { get; set; }
    }
}
