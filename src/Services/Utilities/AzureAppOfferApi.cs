using ManagedApplicationScheduler.Services.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;

namespace ManagedApplicationScheduler.Services.Utilities
{
    public class AzureAppOfferApi
    {
        private string apiProduct = "https://api.partner.microsoft.com/v1.0/ingestion/products?$filter=resourceType eq 'AzureApplication' and ExternalIDs/Any(i:i/Type eq 'AzureOfferId' and i/Value eq '%OFFERID%')";
        private string apiAllProducts = "https://api.partner.microsoft.com/v1.0/ingestion/products?$filter=resourceType eq 'AzureApplication'";
        private string apiProductVariants = "https://api.partner.microsoft.com/v1.0/ingestion/products/%PRODUCTID%/variants/";
        private string apiProductBranches = "https://api.partner.microsoft.com/v1.0/ingestion/products/%PRODUCTID%/branches/getByModule(module=availability)";
        private string apiProductFeatures = "https://api.partner.microsoft.com/v1.0/ingestion/products/%PRODUCTID%/featureAvailabilities/getByInstanceID(instanceID=%INSTANCEID%)";
        private string token;
        private HttpClient httpClient = new HttpClient();
        public AzureAppOfferApi(string token)
        {
            this.token = token;
            this.httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
        }

        public async Task<string> getProductDims(string offerId, string planId)
        {
            string dims = "";
            string productId = await getProductIdAsync(offerId.Replace("-preview", ""));
            if (productId != null)
            {
                string variantsId = await getVariantsId(productId, planId);
                if (variantsId != null)
                {
                    string instanceId = await getInstanceId(productId, variantsId);
                    if (instanceId != null)
                    {
                        string dimlist = await getProductFeature(productId, instanceId);
                        if (dimlist != null)
                        {
                            dims = dimlist;
                        }
                    }
                }
            }



            return dims;
        }

        private async Task<string> getProductIdAsync(string offerId)
        {
            var url = this.apiProduct.Replace("%OFFERID%", offerId);
            var response = await httpClient.GetAsync(url).ConfigureAwait(continueOnCapturedContext: false);

            var responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(continueOnCapturedContext: false);

            var products = JsonSerializer.Deserialize<ProductModel>(responseBody);
            if (products != null)
            {
                return products.value[0].id;
            }
            return "";
        }

        private async Task<string> getVariantsId(string productId, string planId)
        {


            var url = this.apiProductVariants.Replace("%PRODUCTID%", productId);
            var response = await httpClient.GetAsync(url).ConfigureAwait(continueOnCapturedContext: false);

            var responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(continueOnCapturedContext: false);

            var items = JsonSerializer.Deserialize<ProductVariantModel>(responseBody);

            foreach (var item in items.value)
            {
                if (item.externalID == planId)
                {
                    return item.id;
                }
            }

            return "";
        }

        private async Task<List<VariantModel>> getVariantsList(string productId)
        {
            var variantlist = new List<VariantModel>();
            var url = this.apiProductVariants.Replace("%PRODUCTID%", productId);

            while(true)
            {

                var response = await httpClient.GetAsync(url).ConfigureAwait(continueOnCapturedContext: false);

                var responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(continueOnCapturedContext: false);

                var items = JsonSerializer.Deserialize<ProductVariantModel>(responseBody);

                variantlist.AddRange(items.value);


                Console.WriteLine("Total Variant count " + variantlist.Count.ToString());
                if (String.IsNullOrEmpty(items.nextLink))
                {
                    Console.WriteLine("No more next link, breaking out now...");
                    break;
                }
                else
                {
                    url = "https://api.partner.microsoft.com/" + items.nextLink;
                    Console.WriteLine("Current next link " + url);
                }
            }

            return variantlist;


        }

        private async Task<string> getInstanceId(string productId, string variantsId)
        {
            while (true)
            {

                var url = this.apiProductBranches.Replace("%PRODUCTID%", productId);
                var response = await httpClient.GetAsync(url).ConfigureAwait(continueOnCapturedContext: false);

                var responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(continueOnCapturedContext: false);

                var items = JsonSerializer.Deserialize<ProductBranchModel>(responseBody);
                foreach (var item in items.value)
                {
                    if (item.variantID == variantsId)
                    {
                        return item.currentDraftInstanceID;
                    }
                }

                Console.WriteLine("Instance was not found yet. check for nextLink ");
                if (String.IsNullOrEmpty(items.nextLink))
                {
                    Console.WriteLine("No more next link, breaking out now...");
                    break;
                }
                else
                {
                    url = "https://api.partner.microsoft.com/" + items.nextLink;
                    Console.WriteLine("Current next link " + url);
                }
            }
            return "";
        }

        private async Task<string> getProductFeature(string productId, string instanceId)
        {
            var dimsList = new List<string>();
            var url = this.apiProductFeatures.Replace("%PRODUCTID%", productId).Replace("%INSTANCEID%", instanceId);
            var response = await httpClient.GetAsync(url).ConfigureAwait(continueOnCapturedContext: false);

            var responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(continueOnCapturedContext: false);

            var items = JsonSerializer.Deserialize<ProductFeatureModel>(responseBody);
            foreach (var item in items.value)
            {
                if (item.id == instanceId)
                {
                    foreach (var dim in item.customMeters)
                    {
                        if (dim.isEnabled)
                        {
                            dimsList.Add(dim.id);
                        }
                    }
                }
            }

            if (dimsList.Count > 0)
            {
                return string.Join<string>("|", dimsList);
            }

            return "";
        }



        private async Task<List<ProductValue>> getProductAllAsync()
        {
            var productlist = new List<ProductValue>();
            string url = this.apiAllProducts;
            try
            {
               
                while (true)
                {
                    var response = await httpClient.GetAsync(url).ConfigureAwait(continueOnCapturedContext: false);

                    var responseBody = await response.Content.ReadAsStringAsync();

                    var products = JsonSerializer.Deserialize<ProductModel>(responseBody);
                    productlist.AddRange(products.value);
                    Console.WriteLine("Total Products count "+ productlist.Count.ToString()) ;
                    if (String.IsNullOrEmpty(products.nextLink))
                    {
                        Console.WriteLine("No more next link, breaking out now...");
                        break;
                    }
                    else
                    {
                        url = "https://api.partner.microsoft.com/" + products.nextLink;
                        Console.WriteLine("Current next link "+url);
                    }
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return productlist;
        }


        public async Task<List<PlanModel>> getProductsPlansAsync()
        {
            var productList = await getProductAllAsync();
            var planList = new List<PlanModel>();
            foreach (var item in productList)
            {
                var offerPlans = await getVariantsList(item.id);
                foreach (var offerPlan in offerPlans)
                {
                    try
                    {
                        if (offerPlan.id != "testdrive")
                        {
                            var plan = new PlanModel();
                            plan.ProductName = item.name;
                            plan.Product = item.externalIDs[0].value;
                            plan.Name = offerPlan.externalID; //planID
                            plan.PlanName = offerPlan.friendlyName;
                            string instanceId = await getInstanceId(item.id, offerPlan.id);
                            if (instanceId != null)
                            {
                                string dimlist = await getProductFeature(item.id, instanceId);
                                if (dimlist != "")
                                {
                                    plan.Dimension = dimlist;
                                    planList.Add(plan); // Add Only Product with Plan and Dim
                                }
                            }
                        }
                    } catch (Exception ex) { throw; }

                }

            }
            

            return planList;
        }
    }
}
