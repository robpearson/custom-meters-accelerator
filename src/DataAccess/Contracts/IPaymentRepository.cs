using ManagedApplicationScheduler.DataAccess.Entities;

namespace ManagedApplicationScheduler.DataAccess.Contracts
{
    public interface IPaymentRepository
    {
        IEnumerable<Payment> GetAll();
        Payment? Get(string id);
        int Save(Payment entity);
        void Update(Payment entity);

        void Remove(Payment entity);
    }
}
