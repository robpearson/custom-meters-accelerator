using ManagedApplicationScheduler.DataAccess.Context;
using ManagedApplicationScheduler.DataAccess.Contracts;
using ManagedApplicationScheduler.DataAccess.Entities;

namespace ManagedApplicationScheduler.DataAccess.Services
{

    public class ApplicationConfigurationRepository : IApplicationConfigurationRepository
    {
        /// <summary>
        /// The this.context.
        /// </summary>
        private readonly ApplicationsDBContext context;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationLogRepository"/> class.
        /// </summary>
        /// <param name="context">The this.context.</param>
        public ApplicationConfigurationRepository(ApplicationsDBContext context)
        {
            this.context = context;

        }

        /// <summary>
        /// Get All Records
        /// </summary>
        /// <returns></returns>

        public IEnumerable<ApplicationConfiguration> GetAll()
        {
            return this.context.ApplicationConfigurations;
        }

        /// <summary>
        /// Get record by ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ApplicationConfiguration? Get(string id)
        {
            return this.context.ApplicationConfigurations.Where(s => s.id == id).FirstOrDefault();
        }

        /// <summary>
        /// Insert new record or Update existing record in Schedule Management
        /// </summary>
        /// <param name="entities"></param>
        /// <returns></returns>
        public int Save(ApplicationConfiguration entity)
        {
            this.context.ApplicationConfigurations.Add(entity);
            return this.context.SaveChanges();

        }

        public string? GetValueByName(string name)
        {

            var config = context.ApplicationConfigurations.Where(s => s.Name == name).FirstOrDefault();
            return config?.Value;
        }

        public void Update(ApplicationConfiguration entity)
        {
            this.context.ApplicationConfigurations.Update(entity);
            this.context.SaveChanges();
        }

        public void Remove(ApplicationConfiguration entity)
        {
            this.context.ApplicationConfigurations.Remove(entity);
            this.context.SaveChanges();
        }


    }


}
