using System;
using System.Collections.Generic;
using System.Text;
using Volo.Abp.MultiTenancy;

namespace Acme.BookStore.Reports
{
    [Serializable]
    public class BackgroundJobEvent : IMultiTenant
    {
        public Guid UserId { get; set; }
        public Guid? TenantId { get; set; }
    }

    [Serializable]
    public class ReportCompleteEvent : BackgroundJobEvent
    {
        public string Message { get; set; }
        public Guid? Id { get; set; }
    }
}
