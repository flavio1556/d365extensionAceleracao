using Microsoft.Xrm.Sdk;
using System;

namespace d365extensionAceleracao.Account
{
    public class FollowUpAccount : IPlugin
    {

        /// <summary>
        /// Create an asynchronous plug-in, registered on the create massage of the account table (i.e entity here).
        /// This plug-in will create task activity that will remind the creator (owner)
        /// of the account to follow up one week later
        /// </summary>
        public void Execute(IServiceProvider serviceProvider)
        {
            // obtain the tracing service 
            ITracingService tracingService =
            (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            //obtain the execution context from service provider.
            IPluginExecutionContext context = (IPluginExecutionContext)
                serviceProvider.GetService(typeof(IPluginExecutionContext));

            // The inputParameters collection contains all the data passed in the message resquet.
            if (context.InputParameters.Contains("Target") &&
               context.InputParameters["Target"] is Entity account)
            {
                // Obtain the organization service reference whice you will need for 
                // web service calls.
                IOrganizationServiceFactory serviceFactory =
                    (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);
                try
                {
                    tracingService.Trace(account.Id.ToString());
                    Entity followUP = new Entity("task");
                    followUP["subject"] = "Send e-mail to the new customer";
                    followUP["description"] =
                        "Follow up with the customer. Check if there are any new issues that need resolution.";
                    followUP["scheduledstart"] = DateTime.Now.AddDays(7);
                    followUP["scheduledend"] = DateTime.Now.AddDays(7);
                    followUP["category"] = context.PrimaryEntityName;
                    //refer to the account in the task activity
                    if (context.OutputParameters.Contains("id"))
                    {
                        Guid regardingobjectid = new Guid(context.OutputParameters["id"].ToString());
                        string regardingobjectidType = "account";
                        followUP["regardingobjectid"] =                                  
                            new EntityReference(regardingobjectidType, regardingobjectid);

                        // create the task in Microsoft Dynamics Crm
                        tracingService.Trace($"{this.GetType().Name} : Creating the task activity.");
                        service.Create(followUP);
                    }
                }
                catch (Exception ex)
                {
                    tracingService.Trace($"{this.GetType().Name} error {0}", ex.ToString());
                    throw;
                }
            }

        }
    }
}
