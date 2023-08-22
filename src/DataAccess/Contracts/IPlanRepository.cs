using ManagedApplicationScheduler.DataAccess.Entities;

namespace ManagedApplicationScheduler.DataAccess.Contracts
{
    public interface IPlanRepository
    {
        IEnumerable<Plan> GetAll();
        Plan? Get(string id);
        int Save(Plan entity);
        void Update(Plan entity);

        void Remove(Plan entity);
    }
}
