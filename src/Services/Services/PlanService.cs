using ManagedApplicationScheduler.DataAccess.Contracts;
using ManagedApplicationScheduler.DataAccess.Entities;
using ManagedApplicationScheduler.Services.Models;
using ManagedApplicationScheduler.Services.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace ManagedApplicationScheduler.Services.Services
{
    public class PlanService
    {
        private IPlanRepository PlanRepository;

        public PlanService(IPlanRepository PlanRepository)
        {
            this.PlanRepository = PlanRepository;
        }
        public int SavePlan(PlanModel planModel)
        {


            var model = this.GetPlanByOfferIdPlanId(planModel.Name,planModel.Product);
            if (model != null)
            {
                var entity = this.PlanRepository.Get(model.id);
                entity.PlanName = planModel.PlanName;
                entity.PlanId = planModel.Name; // plan ID in notification payload
                entity.OfferName = planModel.ProductName;
                entity.OfferId = planModel.Product; // Offer ID in notification payload
                entity.Dimension = planModel.Dimension;
                this.PlanRepository.Update(entity);
                return 0;
            }
            else
            {

                var entity = new Plan();
                entity.id = Guid.NewGuid().ToString();
                entity.PartitionKey = entity.id;
                entity.PlanName = planModel.PlanName;
                entity.PlanId = planModel.Name; // plan ID in notification payload
                entity.OfferName = planModel.ProductName;
                entity.OfferId = planModel.Product; // Offer ID in notification payload
                entity.Dimension = planModel.Dimension;
                return this.PlanRepository.Save(entity);
            }


        }

        public List<PlanModel> GetAllPlan()
        {
            var planList = new List<PlanModel>();
            var entities = this.PlanRepository.GetAll().ToList() ;
            foreach (Plan entity in entities)
            {
                var plan = new PlanModel();
                plan.id = entity.id ;
                plan.PlanName = entity.PlanName;
                plan.Name = entity.PlanId; // plan ID in notification payload
                plan.ProductName = entity.OfferName;
                plan.Product = entity.OfferId; // Offer ID in notification payload
                plan.Dimension = entity.Dimension;
                plan.Publisher = "";
                plan.Version = "";
                planList.Add(plan);
            }

            return planList;
        }


        public void DeletePlan(string id)
        {
            // Check if the subscription has any Tasks
            var entity = this.PlanRepository.Get(id);
            this.PlanRepository.Remove(entity);

        }
        public PlanModel GetPlanID(string id)
        {
            var plan = new PlanModel();
            var entity = this.PlanRepository.Get(id);
            plan.id = entity.id;
            plan.PlanName = entity.PlanName;
            plan.Name = entity.PlanId; // plan ID in notification payload
            plan.ProductName = entity.OfferName;
            plan.Product = entity.OfferId; // Offer ID in notification payload
            plan.Dimension = entity.Dimension;
            return plan;
        }

        public List<string> GetOfferList()
        {
            var offers = this.PlanRepository.GetAll().ToList().Select(x => x.OfferId).Distinct().ToList();

            return offers;
        }

        public string GetDimListByOfferIDByPlanID(string offerId,string planId)
        {
            string dims = "";
            var dimsList = new List<string>();

            var entities = this.PlanRepository.GetAll().Where(x => x.OfferId == offerId && x.PlanId==planId).ToList();
            foreach (var entity in entities)
            {
                dimsList.Add(entity.Dimension);
            }

            if (dimsList.Count > 0)
            {
                return string.Join<string>("|", dimsList);
            }
            return dims;


        }

        public List<PlanModel> GetPlanListByOfferId(string offerId)
        {
            var entities = this.PlanRepository.GetAll().Where(x => x.OfferId == offerId).ToList();

            var PlanList = new List<PlanModel>();
            
            foreach (Plan entity in entities)
            {
                var plan = new PlanModel();
                plan.id = entity.id;
                plan.PlanName = entity.PlanName;
                plan.Name = entity.PlanId; // plan ID in notification payload
                plan.ProductName = entity.OfferName;
                plan.Product = entity.OfferId; // Offer ID in notification payload
                plan.Dimension = entity.Dimension;
                PlanList.Add(plan);
            }

            
            return PlanList;
        }

        public PlanModel GetPlanByOfferIdPlanId(string  planId, string offerId)
        {
            
            var entity = this.PlanRepository.GetAll().Where(x=>x.PlanId==planId && x.OfferId==offerId).FirstOrDefault();
            if (entity != null)
            {
                var plan = new PlanModel();
                plan.id = entity.id;
                plan.PlanName = entity.PlanName;
                plan.Name = entity.PlanId; // plan ID in notification payload
                plan.ProductName = entity.OfferName;
                plan.Product = entity.OfferId; // Offer ID in notification payload
                plan.Dimension = entity.Dimension;
                return plan;
            }
            
            return null;
        }

        public async Task GetAllMeteredPlansAsync(string token)
        {

            try
            {
                var azure = new AzureAppOfferApi(token);
                var plan = await azure.getProductsPlansAsync().ConfigureAwait(true);
                foreach (var item in plan)
                {
                    this.SavePlan(item);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        

    }
}
