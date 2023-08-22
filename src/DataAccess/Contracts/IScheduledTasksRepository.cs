using ManagedApplicationScheduler.DataAccess.Entities;

namespace ManagedApplicationScheduler.DataAccess.Contracts
{
    public interface IScheduledTasksRepository
    {
        IEnumerable<ScheduledTasks> GetAll();
        ScheduledTasks? Get(string id);
        int Save(ScheduledTasks entity);
        void Update(ScheduledTasks entity);

        void Remove(ScheduledTasks entity);
    }
}
