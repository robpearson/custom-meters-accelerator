using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManagedApplicationScheduler.Services.Models
{
    public class PaymentFormModel
    {
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
        public string SelectedDimension { get; set; }
        public SelectList DimensionsList { get; set; }

        public string SelectedProduct { get; set; }
        public SelectList ProductList { get; set; }

        /// <summary>
        /// Plan associated with the purchased offer.
        /// </summary>
        public string SelectedPlan { get; set; }
        public SelectList PlanList { get; set; }

        /// <summary>
        /// Expect start date
        /// </summary>
        public DateTime StartDate { get; set; }

        public SelectList PaymentTypeList { get; set; } //upfront, milestone
        public string SelectedPaymentType { get; set; }

        public int TimezoneOffset { get; set; }
        public bool IsUpfrontPayment { get; set; }

        public string Error {  get; set; }
    }
}
