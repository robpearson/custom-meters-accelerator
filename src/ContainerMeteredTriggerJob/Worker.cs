using System.Text.Json;
using ManagedApplicationScheduler.Services.Models;

namespace MeteredSheduler
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly string MARKETPLACE_API = "https://marketplaceapi.microsoft.com/api/usageEvent?api-version=2018-08-31";

        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var token = await getMsiToken();
                if (token == null || token == "")
                {
                    _logger.LogInformation("Can not get token");
                }
                else
                {
                    try
                    {
                        var options = new JsonSerializerOptions{PropertyNameCaseInsensitive = true};
                        _logger.LogInformation($"Worker running at: {DateTimeOffset.Now.ToString()}" );
                        string productId = Environment.GetEnvironmentVariable("PRODUCT_ID");
                        string planId = Environment.GetEnvironmentVariable("PLAN_ID");
                        string applicationId = Environment.GetEnvironmentVariable("EXTENSION_RESOURCE_ID");
                        string webookUri = Environment.GetEnvironmentVariable("WEBHOOK_URI");
                        string subscriptionKey = Environment.GetEnvironmentVariable("SUBSCRIPTION_KEY");

                        Console.WriteLine($"productId is {productId}" );
                        Console.WriteLine($"planId is {planId}" );
                        Console.WriteLine($"subscriptionId is {applicationId}" );


                        if (string.IsNullOrEmpty(webookUri) ||
                            string.IsNullOrEmpty(productId) ||
                            string.IsNullOrEmpty(planId) ||
                            string.IsNullOrEmpty(applicationId))
                        {
                            Console.WriteLine("engine will not run due to missing configuration");
                        }
                        else
                        {
                            // create notification body
                            var notification = new NotificationDefinitionModel();
                            var plan = new PlanModel();
                            plan.Name = planId;
                            plan.Product = productId;
                            notification.ApplicationId = applicationId;
                            notification.Plan = plan;
                            notification.EventType = "PUT";
                            notification.ProvisioningState = "Succeeded";
                            notification.SubscriptionKey = subscriptionKey;
                            HttpClient client = new HttpClient();
                            using StringContent jsonContent = new( JsonSerializer.Serialize(notification),null,"application/json");
                            var request = new HttpRequestMessage(HttpMethod.Get, webookUri);
                            request.Content = jsonContent;
                            var response = await client.SendAsync(request);

                            response.EnsureSuccessStatusCode();

                            var jsonResponse = await response.Content.ReadAsStringAsync();
                            _logger.LogInformation($"Subscription usage submitted and response was {jsonResponse}\n");

                            var meterUsageResultList = new List<MeteredUsageResultModel>();
                            if (response.IsSuccessStatusCode)
                            {
                                var schedulerList = JsonSerializer.Deserialize<List<ScheduledTasksModel>>(jsonResponse,options);
                                foreach (var task in schedulerList)
                                {
                                    var meteringUsageResult = new MeteredUsageResultModel();
                                    Console.WriteLine($"current task is {JsonSerializer.Serialize(task)}");
                                    var meterUsage = new MeteredUsageRequestModel {
                                        Dimension = task.Dimension,
                                        Quantity = task.Quantity,
                                        EffectiveStartTime = DateTime.Now.ToUniversalTime(),
                                        PlanId = task.PlanId,
                                        ResourceUri = task.ResourceUri,
                                    };

                                    using StringContent jsontask = new(JsonSerializer.Serialize(meterUsage),null,"application/json");

                                    client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
                                    using HttpResponseMessage taskResponse = await client.PostAsync(MARKETPLACE_API,jsontask);

                                    
                                    if (taskResponse.IsSuccessStatusCode)
                                    {
                                        
                                        var jsonTaskResponse = await taskResponse.Content.ReadAsStringAsync();
                                        _logger.LogInformation($"Metered usage submitted and response was successfull \n");
                                        meteringUsageResult = JsonSerializer.Deserialize<MeteredUsageResultModel>(jsonTaskResponse,options);
                                        

                                    }
                                    else
                                    {
                                        var jsonTaskResponseError = await taskResponse.Content.ReadAsStringAsync();
                                        _logger.LogInformation($"Metered usage submitted and response was failure \n");
                                        if (jsonTaskResponseError.Contains("additionalInfo"))
                                        {
                                            _logger.LogInformation($"Metered usage submitted and response has error \n");
                                            var errorResult = JsonSerializer.Deserialize<MeteredUsageErrorResultModel>(jsonTaskResponseError,options);
                                            meteringUsageResult = errorResult.additionalInfo.acceptedMessage;
                                            meteringUsageResult.Message = errorResult.Message;
                                        }
                                    }

                                    meteringUsageResult.ScheduledTaskId = task.id;
                                    meteringUsageResult.ScheduledTaskName = task.ScheduledTaskName;
                                    meterUsageResultList.Add( meteringUsageResult );
                                }
                            
                                if(meterUsageResultList.Count > 0)
                                {
                                    // send result back to webhook
                                    using StringContent content = new(JsonSerializer.Serialize(meterUsageResultList), null,"application/json");
                                    request = new HttpRequestMessage(HttpMethod.Post, webookUri);
                                    request.Content = content;
                                    response = await client.SendAsync(request);

                                    if (response.IsSuccessStatusCode)
                                    {
                                        _logger.LogInformation($"Successfully send metered usage result \n");
                                    }
                                    else
                                    {
                                        _logger.LogInformation($"Failed to send metered usage result \n");
                                    }



                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                }

                System.Environment.Exit(0);
        }
    
        public async Task<string> getMsiToken()
        {
            Console.WriteLine("Getting Token from MSI");
            var resource = "20e940b3-4c77-4b0b-9a53-9e16a1b010a7";
            var clientId = Environment.GetEnvironmentVariable("CLIENT_ID");

            Console.WriteLine($"Current clientId: {clientId}");
            var token = "";
            try
            {
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Add("Metadata", "true");
                var url = $"http://169.254.169.254/metadata/identity/oauth2/token?api-version=2018-02-01&client_id={clientId}&resource={resource}";
                Console.WriteLine(url);
                using HttpResponseMessage response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var jsonResponse = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                token = JsonSerializer.Deserialize<TokenDefinition>(jsonResponse,options).Access_token;
            }
            catch(Exception e)
            {
                Console.WriteLine($"Error during calling MSI point {e.Message}");
                
            }
            return token;
        } 

    }
}
