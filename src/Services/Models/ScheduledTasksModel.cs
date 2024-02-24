using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ManagedApplicationScheduler.Services.Models
{
    /// <summary>
    /// The usage event definition.
    /// </summary>
    public class ScheduledTasksModel
    {
        /// <summary>
        /// Get or Set Id
        /// </summary>
        public string id { get; set; }


        /// <summary>
        /// Get or Set Scheduled Task Name.
        /// </summary>
        [JsonPropertyName("scheduledTaskName")]
        public string ScheduledTaskName { get; set; }


        /// <summary>
        /// Identifier of the resource against which usage is emitted.
        /// </summary>
        [JsonPropertyName("resourceUri")]
        public string ResourceUri { get; set; }

        /// <summary>
        /// The quantity of the usage.
        /// </summary>
        [JsonPropertyName("quantity")]
        public double Quantity { get; set; }

        /// <summary>
        /// Dimension identifier.
        /// </summary>
        [JsonPropertyName("dimension")]
        public string Dimension { get; set; }

        /// <summary>
        /// Time in UTC when the usage event occurred.
        /// </summary>
        [JsonPropertyName("nextRunTime")]
        public DateTime? NextRunTime { get; set; }
        [JsonPropertyName("startDate")]
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// Plan associated with the purchased offer.
        /// </summary>
        [JsonPropertyName("planId")]
        public string PlanId { get; set; }
        [JsonPropertyName("frequency")]
        public string Frequency { get; set; }

        public string Status { get; set; } //Enabled, Canceled, Completed, Error


        public List<MeteredUsageResultModel> MeteredUsageResult { get; set; }

    }
}
