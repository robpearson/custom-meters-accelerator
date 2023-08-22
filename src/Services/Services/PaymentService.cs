using ManagedApplicationScheduler.DataAccess.Contracts;
using ManagedApplicationScheduler.DataAccess.Entities;
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

        public PaymentService(IPaymentRepository paymentRepository)
        {
            this.paymentRepository = paymentRepository;
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

        
    }
}
