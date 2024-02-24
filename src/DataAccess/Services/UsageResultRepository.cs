using ManagedApplicationScheduler.DataAccess.Context;
using ManagedApplicationScheduler.DataAccess.Contracts;
using ManagedApplicationScheduler.DataAccess.Entities;

namespace ManagedApplicationScheduler.DataAccess.Services
{
    public class UsageResultRepository : IUsageResultRepository
    {
        /// <summary>
        /// The context.
        /// </summary>
        private readonly ApplicationsDBContext context;



        public UsageResultRepository(ApplicationsDBContext context)
        {
            this.context = context;
            this.context.Database.EnsureCreated();

        }

        /// <summary>
        /// Get All Records
        /// </summary>
        /// <returns></returns>

        public IEnumerable<UsageResult> GetAll()
        {
            return this.context.UsageResults;
        }

        /// <summary>
        /// Get record by ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public UsageResult? Get(string id)
        {
            return this.context.UsageResults.Where(s => s.id == id).FirstOrDefault();
        }

        /// <summary>
        /// Insert new record or Update existing record in Schedule Management
        /// </summary>
        /// <param name="entities"></param>
        /// <returns></returns>
        public int Save(UsageResult entity)
        {
            this.context.UsageResults.Add(entity);
            return this.context.SaveChanges();

        }
    }
}
