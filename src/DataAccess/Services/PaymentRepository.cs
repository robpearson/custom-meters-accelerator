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
        private readonly CosmosDbContext context;




        public PaymentRepository(CosmosDbContext context)
        {
            this.context = context;
            this.context.Database.EnsureCreated();
        }

        /// <summary>
        /// Get All Records
        /// </summary>
        /// <returns></returns>

        public IEnumerable<Payment> GetAll()
        {
            return this.context.Payment;
        }

        /// <summary>
        /// Get record by ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Payment? Get(string id)
        {
            return this.context.Payment.Where(s => s.id == id).FirstOrDefault();
        }

        /// <summary>
        /// Insert new record or Update existing record in Schedule Management
        /// </summary>
        /// <param name="entities"></param>
        /// <returns></returns>
        public int Save(Payment entity)
        {
            this.context.Add(entity);
            return this.context.SaveChanges();

        }

        public void Update(Payment entity)
        {

            this.context.Update(entity);
            this.context.SaveChanges();


        }

        public void Remove(Payment entity)
        {
            this.context.Remove(entity);
            this.context.SaveChanges();
        }
    }
}
