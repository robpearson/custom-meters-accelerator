using Azure.Identity;
using ManagedApplicationScheduler.DataAccess.Contracts;
using ManagedApplicationScheduler.Services.Configurations;
using ManagedApplicationScheduler.Services.Models;
using ManagedApplicationScheduler.Services.Services;
using ManagedApplicationScheduler.Services.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace ManagedApplicationScheduler.AdminSite.Controllers.Webhook
{
    [ApiController]
    [AllowAnonymous]
    [Route("api/container")]
    public class ContainerController : ControllerBase
    {

        private readonly ManagedAppClientConfiguration config;
        private readonly SubscriptionService subscriptionService;
        private readonly ApplicationLogService applicationLogService;
        private readonly PaymentService paymentService;
        private readonly PlanService planService;
        private readonly SchedulerService schedulerService;
        private UsageResultService usageResultService;

        public ContainerController(ManagedAppClientConfiguration config, ISubscriptionsRepository subscriptionsRepository, IApplicationLogRepository applicationLogRepository,IPaymentRepository paymentRepository, IScheduledTasksRepository scheduledTasksRepository, IUsageResultRepository usageResultRepository,IPlanRepository planRepository )
        {
            subscriptionService = new SubscriptionService(subscriptionsRepository);
            this.config = config;
            this.applicationLogService = new ApplicationLogService(applicationLogRepository);
            this.paymentService = new PaymentService(paymentRepository, scheduledTasksRepository);
            this.schedulerService = new SchedulerService(scheduledTasksRepository,null,null);
            this.usageResultService = new UsageResultService(usageResultRepository);
            this.planService = new PlanService(planRepository);
        }
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> GetAsync(NotificationDefinitionModel notificationDefinition, string sig)
        {
            // create new notification
            //Sending subscription Managed Idenity
            //Check if it exist
            // Save if it new
            // Return any scheduled task for this managed Identity

            if(notificationDefinition == null)
            {
                throw new ArgumentNullException(nameof(notificationDefinition));
            }
            this.applicationLogService.AddApplicationLog($"Notification recieved {JsonSerializer.Serialize(notificationDefinition)} ");

            if (config.Signature == sig)
            {
               // var creds = new ClientSecretCredential(config.PC_TenantId, config.PC_ClientID, config.PC_ClientSecret);
               // var result = await creds.GetTokenAsync(new Azure.Core.TokenRequestContext(new string[] { config.PC_Scope }), System.Threading.CancellationToken.None).ConfigureAwait(false);
               // var token = result.Token;
                // If provisioning of a marketplace application instance is successful, we persist a Meter entry to be picked up by the chron metric emitting job
                if (notificationDefinition.EventType == "PUT" && notificationDefinition.ProvisioningState == "Succeeded")
                {
                    var sub = this.subscriptionService.GetSubscriptionByKey(notificationDefinition.Plan.Product,notificationDefinition.Plan.Name,notificationDefinition.SubscriptionKey);
                    if (sub is null)
                    {
                        var subscription = new SubscriptionModel
                        {
                            // CosmosDB does not support forward slashes in the id.
                            ResourceUri = notificationDefinition.ApplicationId,
                            PlanId = notificationDefinition.Plan.Name,
                            Product = notificationDefinition.Plan.Product,
                            ProvisionState = notificationDefinition.ProvisioningState,
                            ProvisionTime = DateTime.UtcNow,
                            SubscriptionStatus = "Subscribed",
                            SubscriptionKey = notificationDefinition.SubscriptionKey,
                            id = Guid.NewGuid().ToString()
                        };
                        try
                        {
                            this.applicationLogService.AddApplicationLog($"Get dims list for Product {subscription.Product} with plan {subscription.PlanId}");
                            //var azureOfferApi = new AzureAppOfferApi(token, "AzureContainer");
                            //subscription.Dimension = await azureOfferApi.getProductDims(subscription.Product, subscription.PlanId).ConfigureAwait(false);

                            subscription.Dimension = this.planService.GetDimListByOfferIDByPlanID(subscription.Product, subscription.PlanId);
                            this.applicationLogService.AddApplicationLog($"Found dims : {subscription.Dimension}");
                        }
                        catch (Exception ex)
                        {

                            Console.WriteLine($"Error during getting Product Dims.  {ex.Message}");
                            this.applicationLogService.AddApplicationLog($"Error during getting Product Dims.  {ex.Message}");
                            throw;
                        }

                        subscriptionService.SaveSubscription(subscription);
                        this.applicationLogService.AddApplicationLog($"Successful Subscription added {notificationDefinition.ApplicationId} ");

                        paymentService.SaveMilestonePayment(subscription);
                        this.applicationLogService.AddApplicationLog($"Successful ScheduleTasks added {notificationDefinition.ApplicationId} ");

                        Console.WriteLine($"Successfully inserted the entry in CosmosDB for the application {notificationDefinition.ApplicationId}");
                        this.applicationLogService.AddApplicationLog($"Successfully inserted the entry in CosmosDB for the application {notificationDefinition.ApplicationId}");
                        sub = subscription;
                    }
                    else
                    {
                        if (sub.ResourceUri != notificationDefinition.ApplicationId)
                        {
                            this.subscriptionService.UpdateSubscriptionResourceUri(sub.id, notificationDefinition.ApplicationId);
                            this.schedulerService.UpdateSchedulerResourceUri(sub.ResourceUri, notificationDefinition.ApplicationId);
                            sub.ResourceUri = notificationDefinition.ApplicationId;
                        }
                    }

                    return GetScheduledTask(sub);

                }

                return BadRequest();
            }
            else { return Forbid(); }


        }
        [AllowAnonymous]
        [HttpGet]
        private IActionResult GetScheduledTask(SubscriptionModel subscription)
        {
            try
            {
                List<ScheduledTasksModel> task = new ();
                var scheduledItems = this.schedulerService.GetEnabledSchedulersTasksBySubscription(subscription.ResourceUri);
                //GetCurrentUTC time
                DateTime _currentUTCTime = DateTime.UtcNow;
                TimeSpan ts = new TimeSpan(DateTime.UtcNow.Hour, 0, 0);
                _currentUTCTime = _currentUTCTime.Date + ts;
                foreach (var scheduledItem in scheduledItems)
                {
                    //Always pickup the NextRuntime, durnig firstRun or OneTime then pickup StartDate, as the NextRunTime will be null
                    DateTime? _nextRunTime = scheduledItem.NextRunTime ?? scheduledItem.StartDate;
                    int timeDifferentInHours = (int)_currentUTCTime.Subtract(_nextRunTime.Value).TotalHours;

                    if (timeDifferentInHours >= 0)
                    {
                        var msg = $"Scheduled Item Id: {scheduledItem.ScheduledTaskName} will not run as {_nextRunTime} has passed. Please check audit logs if its has run previously.";
                        this.applicationLogService.AddApplicationLog(msg);
                        Console.WriteLine(msg);
                        task.Add(scheduledItem);
                    }
                }
                return Ok(task);

            }
            catch
            {
                return BadRequest();
            }
        }

        [AllowAnonymous]
        [HttpPost]
        public IActionResult Post(List<MeteredUsageResultModel> meteredUsageResults, string sig)
        {
            if (meteredUsageResults == null)
            {
                throw new ArgumentNullException(nameof(meteredUsageResults));
            }
            this.applicationLogService.AddApplicationLog($"Notification recieved {JsonSerializer.Serialize(meteredUsageResults)} ");

            if (config.Signature == sig)
            {
                foreach(var item in meteredUsageResults)
                {
                    var result = this.schedulerService.ProcessContainerMeterUsageResult(item);
                    if (result != "OK")
                    {
                        this.applicationLogService.AddApplicationLog($"Updated Container scheduler task {item.ScheduledTaskName} successfully");
                    }
                    else
                    {
                        this.applicationLogService.AddApplicationLog($"Failed to Updated Container scheduler task {item.ScheduledTaskName} with error {result}");
                    }

                    this.usageResultService.SaveUsageResult(item);
                    this.applicationLogService.AddApplicationLog($"Scheduler Task {item.ScheduledTaskName} has response {item.Message}");
                }

                return Ok();
            }
            else { return Forbid(); }


        }


    }
}

