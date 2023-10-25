using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.DependencyInjection;

namespace Acme.BookStore.Reports
{
    [Serializable]
    public class MyReportJobArgs
    {
        public string? Content { get; set; }
    }

    public class MyReportJob : AsyncBackgroundJob<MyReportJobArgs>, ITransientDependency
    {
        public override Task ExecuteAsync(MyReportJobArgs args)
        {
            Logger.LogInformation("Executing MyReportJob with args: {0}", args.Content);
            return Task.CompletedTask;
        }
    }
}
