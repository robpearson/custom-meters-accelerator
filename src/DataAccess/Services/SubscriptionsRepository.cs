using ManagedApplicationScheduler.DataAccess.Context;
using ManagedApplicationScheduler.DataAccess.Contracts;
using ManagedApplicationScheduler.DataAccess.Entities;

namespace ManagedApplicationScheduler.DataAccess.Services
{
    public class SubscriptionsRepository : ISubscriptionsRepository
    {

        /// <summary>
        /// The context.
        /// </summary>
        private readonly ApplicationsDBContext context;




        public SubscriptionsRepository(ApplicationsDBContext context)
        {
            this.context = context;
            this.context.Database.EnsureCreated();


        }

        /// <summary>
        /// Get All Records
        /// </summary>
        /// <returns></returns>

        public IEnumerable<Subscription> GetAll()
        {
            return this.context.Subscriptions.ToList();
        }

        /// <summary>
        /// Get record by ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Subscription? Get(string id)
        {
            return context.Subscriptions.Where(s => s.id == id).ToList().FirstOrDefault();
        }


        /// <summary>
        /// Insert new record or Update existing record in Schedule Management
        /// </summary>
        /// <param name="entities"></param>
        /// <returns></returns>
        public int Save(Subscription entity)
        {

            this.context.Subscriptions.Add(entity);
            return this.context.SaveChanges();


        }

        public void Update(Subscription entity)
        {
            this.context.Subscriptions.Update(entity);
            this.context.SaveChanges();
        }
        public void Remove(Subscription entity)
        {
            this.context.Subscriptions.Remove(entity);
            this.context.SaveChanges();
        }


    }
}

