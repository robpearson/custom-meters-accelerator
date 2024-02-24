using ManagedApplicationScheduler.DataAccess.Context;
using ManagedApplicationScheduler.DataAccess.Contracts;
using ManagedApplicationScheduler.DataAccess.Entities;

namespace ManagedApplicationScheduler.DataAccess.Services
{
    public class PlanRepository : IPlanRepository
    {
        /// <summary>
        /// The context.
        /// </summary>
        private readonly ApplicationsDBContext context;




        public PlanRepository(ApplicationsDBContext context)
        {
            this.context = context;
            this.context.Database.EnsureCreated();
        }

        /// <summary>
        /// Get All Records
        /// </summary>
        /// <returns></returns>

        public IEnumerable<Plan> GetAll()
        {
            return this.context.Plans;
        }

        /// <summary>
        /// Get record by ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Plan? Get(string id)
        {
            return this.context.Plans.Where(s => s.id == id).FirstOrDefault();
        }

        /// <summary>
        /// Insert new record or Update existing record in Schedule Management
        /// </summary>
        /// <param name="entities"></param>
        /// <returns></returns>
        public int Save(Plan entity)
        {
            this.context.Plans.Add(entity);
            return this.context.SaveChanges();

        }

        public void Update(Plan entity)
        {

            this.context.Plans.Update(entity);
            this.context.SaveChanges();


        }

        public void Remove(Plan entity)
        {
            this.context.Plans.Remove(entity);
            this.context.SaveChanges();
        }
    }
}
