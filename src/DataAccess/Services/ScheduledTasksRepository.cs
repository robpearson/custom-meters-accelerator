using ManagedApplicationScheduler.DataAccess.Contracts;
using ManagedApplicationScheduler.DataAccess.Entities;
using ManagedApplicationScheduler.DataAccess.Context;
namespace ManagedApplicationScheduler.DataAccess.Services
{
    public class ScheduledTasksRepository : IScheduledTasksRepository
    {
        /// <summary>
        /// The context.
        /// </summary>
        private readonly ApplicationsDBContext context;




        public ScheduledTasksRepository(ApplicationsDBContext context)
        {
            this.context = context;
            
        }

        /// <summary>
        /// Get All Records
        /// </summary>
        /// <returns></returns>

        public IEnumerable<ScheduledTasks> GetAll()
        {
            return this.context.ScheduledTasks;
        }

        /// <summary>
        /// Get record by ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ScheduledTasks? Get(string id)
        {
            return this.context.ScheduledTasks.Where(s => s.id == id).FirstOrDefault();
        }

        /// <summary>
        /// Insert new record or Update existing record in Schedule Management
        /// </summary>
        /// <param name="entities"></param>
        /// <returns></returns>
        public int Save(ScheduledTasks entity)
        {
            this.context.ScheduledTasks.Add(entity);
            return this.context.SaveChanges();

        }

        public void Update(ScheduledTasks entity)
        {

            this.context.ScheduledTasks.Update(entity);
            this.context.SaveChanges();


        }

        public void Remove(ScheduledTasks entity)
        {
            this.context.ScheduledTasks.Remove(entity);
            this.context.SaveChanges();
        }
    }
}
