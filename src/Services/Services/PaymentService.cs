using ManagedApplicationScheduler.DataAccess.Contracts;
using ManagedApplicationScheduler.DataAccess.Entities;
using ManagedApplicationScheduler.DataAccess.Services;
using ManagedApplicationScheduler.Services.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManagedApplicationScheduler.Services.Services
{
    public class PaymentService
    {
        private IPaymentRepository paymentRepository;
        private IScheduledTasksRepository scheduledTasksRepository;

        public PaymentService(IPaymentRepository paymentRepository, IScheduledTasksRepository scheduledTasksRepository)
        {
            this.paymentRepository = paymentRepository;
            this.scheduledTasksRepository = scheduledTasksRepository;
        }

        public int SavePayment(PaymentModel paymentModel)
        {
            

            var entity = new Payment();
            entity.id = Guid.NewGuid().ToString();
            entity.PaymentName = paymentModel.PaymentName;
            entity.OfferId = paymentModel.OfferId;
            entity.PaymentType = paymentModel.PaymentType;
            entity.Quantity = paymentModel.Quantity;
            entity.Dimension = paymentModel.Dimension;
            entity.StartDate = paymentModel.StartDate;
            entity.PlanId = paymentModel.PlanId;
            entity.PartitionKey = entity.id;
            return this.paymentRepository.Save(entity);
        }

        public List<PaymentModel> GetAllPayment()
        {
            var paymentList = new List<PaymentModel>();
            var entities = this.paymentRepository.GetAll().ToList();
            foreach (Payment entity in entities)
            {
                var payment = new PaymentModel();
                payment.id = entity.id;
                payment.PlanId = entity.PlanId;
                payment.OfferId = entity.OfferId;
                payment.Quantity = entity.Quantity;
                payment.Dimension = entity.Dimension;
                payment.StartDate = entity.StartDate.Value;
                payment.PaymentName = entity.PaymentName;
                payment.PaymentType = entity.PaymentType;
                payment.PartitionKey = entity.PartitionKey;
                paymentList.Add(payment);
            }

            return paymentList;
        }


        public void UpdatePayment(PaymentModel paymentModel)
        {
            var entity = this.paymentRepository.Get(paymentModel.id);
            entity.PaymentName = paymentModel.PaymentName;
            entity.PaymentType = paymentModel.PaymentType;
            entity.Quantity = paymentModel.Quantity;
            entity.OfferId = paymentModel.OfferId;
            entity.Dimension = paymentModel.Dimension;
            entity.StartDate = paymentModel.StartDate;
            entity.PlanId = paymentModel.PlanId;
            this.paymentRepository.Update(entity);
        }

        public void DeletePayment(string id)
        {
            // Check if the subscription has any Tasks
            var entity = this.paymentRepository.Get(id);
            this.paymentRepository.Remove(entity);

        }

        
        public PaymentModel GetPaymentID(string id)
        {
            if (string.IsNullOrEmpty(id))
                return null;

            var payment = new PaymentModel();
            var entity = this.paymentRepository.Get(id);
            payment.id = entity.id;
            payment.PlanId = entity.PlanId;
            payment.Quantity = entity.Quantity;
            payment.Dimension = entity.Dimension;
            payment.StartDate = entity.StartDate.Value;
            payment.OfferId = entity.OfferId;
            payment.PaymentName = entity.PaymentName;
            payment.PaymentType = entity.PaymentType;
            payment.PartitionKey = entity.PartitionKey;

            return payment;
        }

        public List<Payment> GetPaymentByOfferByPlan(string offerId,string planId)
        {
            return paymentRepository.GetAll().Where(x=>x.PlanId==planId && x.OfferId == offerId).ToList();
        }

        public List<Payment> GetPaymentByOfferByPlanByDimByType(string offerId, string planId,string dim, string paymentType)
        {
            return paymentRepository.GetAll().Where(x => x.PlanId == planId && x.OfferId == offerId && x.Dimension == dim && x.PaymentType == paymentType).ToList();
        }

        public List<Payment> GetPaymentByName(string paymentName)
        {
            return paymentRepository.GetAll().Where(x => x.PaymentName ==  paymentName).ToList();
        }
        public void SaveMilestonePayment(SubscriptionModel subscription)
        {
            var appName = subscription.ResourceUri.Split("/")[8];
                var payment = GetPaymentByOfferByPlan(subscription.Product, subscription.PlanId);
                foreach(var item in payment)
                {
                    ScheduledTasks task = new ScheduledTasks()
                    {
                        ScheduledTaskName = item.PaymentName+"-"+appName,
                        ResourceUri = subscription.ResourceUri,
                        PlanId = item.PlanId,
                        Dimension = item.Dimension,
                        StartDate = item.StartDate.Value,
                        Frequency = SchedulerFrequencyEnum.OneTime.ToString(),
                        Quantity = item.Quantity,
                        Status = "Scheduled",
                        id = Guid.NewGuid().ToString()
                    };
                    task.PartitionKey = task.id;
                    if(item.PaymentType=="Upfront")
                    {
                        DateTime rounded = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour+1, 0, 0);
                        task.StartDate = rounded;
                    }
                    this.scheduledTasksRepository.Save(task);
                }
                
             

        }
    }
}
