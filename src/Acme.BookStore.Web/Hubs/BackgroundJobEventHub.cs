using System;
using System.Collections;
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
        private static readonly Hashtable s_connectionIds = new(20);
        private readonly IHubContext<BackgroundJobEventHub> _hubContext;

        public BackgroundJobEventHub(IHubContext<BackgroundJobEventHub> hubContext)
        {
            _hubContext = hubContext;
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

        /// <summary>
        /// Record a connectionId along with the UserId so we can send targeted messages back
        /// </summary>
        /// <param name="userId"></param>
        [HubMethodName("RegisterConnectionId")]
        public void RegisterConnectionId(string userId)
        {
            if (s_connectionIds.ContainsKey(userId))
                s_connectionIds[userId] = Context.ConnectionId;
            else
                s_connectionIds.Add(userId, Context.ConnectionId);
        }

        private async Task SendMessage(object eventData, string eventName, Guid userId, Guid? tenantId)
        {
            // Use this, so that the events don't need to be triggered client side. They are triggered in the
            if (_hubContext.Clients != null)
            {
                // If we can, let's send the message only to the client who's expecting to recieve it.
                if (s_connectionIds.ContainsKey(userId.ToString()))
                {
                    var connectionId = s_connectionIds[userId.ToString()].ToString();
                    await _hubContext.Clients.Client(connectionId).SendAsync("backgroundJobMessage", $"{userId};{tenantId};{eventName};{JsonSerializer.Serialize(eventData)}");
                }
                else
                {
                    await _hubContext.Clients.All.SendAsync("backgroundJobMessage", $"{userId};{tenantId};{eventName};{JsonSerializer.Serialize(eventData)}");
                }
            }
        }
    }
}
