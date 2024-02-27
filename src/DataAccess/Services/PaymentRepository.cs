using ManagedApplicationScheduler.DataAccess.Context;
using ManagedApplicationScheduler.DataAccess.Contracts;
using ManagedApplicationScheduler.DataAccess.Entities;

namespace ManagedApplicationScheduler.DataAccess.Services
{
    public class PaymentRepository : IPaymentRepository
    {
        /// <summary>
        /// The context.
        /// </summary>
        private readonly ApplicationsDBContext context;




        public PaymentRepository(ApplicationsDBContext context)
        {
            this.context = context;
            
        }

        /// <summary>
        /// Get All Records
        /// </summary>
        /// <returns></returns>

        public IEnumerable<Payment> GetAll()
        {
            return this.context.Payments;
        }

        /// <summary>
        /// Get record by ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Payment? Get(string id)
        {
            return this.context.Payments.Where(s => s.id == id).FirstOrDefault();
        }

        /// <summary>
        /// Insert new record or Update existing record in Schedule Management
        /// </summary>
        /// <param name="entities"></param>
        /// <returns></returns>
        public int Save(Payment entity)
        {
            this.context.Payments.Add(entity);
            return this.context.SaveChanges();

        }

        public void Update(Payment entity)
        {

            this.context.Payments.Update(entity);
            this.context.SaveChanges();


        }

        public void Remove(Payment entity)
        {
            this.context.Payments.Remove(entity);
            this.context.SaveChanges();
        }
    }
}
