using System;
using System.Text.Json;
using System.Threading.Tasks;
using Acme.BookStore.Reports;
using Microsoft.AspNetCore.SignalR;
using Volo.Abp.AspNetCore.SignalR;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EventBus;

namespace Acme.BookStore.Web.Hubs
{
    public class BackgroundJobEventHub : AbpHub, ITransientDependency, ILocalEventHandler<ReportCompleteEvent>
    {
        private readonly IHubContext<BackgroundJobEventHub> _hubContext;

        public BackgroundJobEventHub(IHubContext<BackgroundJobEventHub> hubContext)
        {
            _hubContext = hubContext;
        }

        private async Task SendMessage(object eventData, string eventName, Guid userId, Guid? tenantId)
        {
            // Use this, so that the events don't need to be triggered client side. They are triggered in the
            if (_hubContext.Clients != null)
            {
                await _hubContext.Clients.All.SendAsync("backgroundJobMessage", $"{userId};{tenantId};{eventName};{JsonSerializer.Serialize(eventData)}");
            }
        }

        public async Task HandleEventAsync(ReportCompleteEvent eventData)
        {
            // get the class name of the eventData
            var className = eventData.GetType().Name;
            if (className.Length > 0)
            {
                // remove the "EventData" suffix and set first characted to lower case
                var eventName = $"app.{char.ToLowerInvariant(className[0])}{className.Substring(1)}".Replace("Event", string.Empty);
                // and send the event
                await SendMessage(eventData, eventName, eventData.UserId, eventData.TenantId);
            }
        }
    }
}
