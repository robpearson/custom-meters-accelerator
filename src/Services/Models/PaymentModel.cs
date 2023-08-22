using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManagedApplicationScheduler.Services.Models
{
    public class PaymentModel
    {
        /// <summary>
        /// Get or Set Id
        /// </summary>
        public string id { get; set; }
        /// <summary>
        /// Get or Set Payment Name.
        /// </summary>
        public string PaymentName { get; set; }
        /// <summary>
        /// The quantity of the usage.
        /// </summary>
        public double Quantity { get; set; }
        /// <summary>
        /// Dimension identifier.
        /// </summary>
        public string Dimension { get; set; }
        /// <summary>
        /// Plan associated with the purchased offer.
        /// </summary>
        public string PlanId { get; set; }
        /// <summary>
        /// Partition Key
        /// </summary>
        public string PartitionKey { get; set; }
        /// <summary>
        /// Expect start date
        /// </summary>
        public DateTime StartDate { get; set; }

        public string PaymentType { get; set; } //upfront, milestone

        public string OfferId { get; set; }
    }
}
