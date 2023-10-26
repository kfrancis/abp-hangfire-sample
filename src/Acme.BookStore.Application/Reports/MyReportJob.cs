using System;
using System.Threading.Tasks;
using Acme.BookStore.Books;
using Microsoft.Extensions.Logging;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.EventBus.Local;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Threading;

namespace Acme.BookStore.Reports
{
    [BackgroundJobName("something")]
    [Serializable]
    public class MyReportJobArgs : IMultiTenant
    {
        public string? Content { get; set; }
        public Guid? UserId { get; set; }
        public Guid? TenantId { get; set; }
    }

    public class MyReportJob : AsyncBackgroundJob<MyReportJobArgs>, ITransientDependency
    {
        private readonly IRepository<Book, Guid> _repository;
        private readonly ICancellationTokenProvider _cancellationTokenProvider;
        private readonly ILocalEventBus _localEventBus;

        public MyReportJob(IRepository<Book, Guid> repository,
            ICancellationTokenProvider cancellationTokenProvider,
            ICurrentTenant currentTenant,
            ILocalEventBus localEventBus)
        {
            _repository = repository;
            _cancellationTokenProvider = cancellationTokenProvider;
            CurrentTenant = currentTenant;
            _localEventBus = localEventBus;
        }

        public ICurrentTenant CurrentTenant { get; }

        public override async Task ExecuteAsync(MyReportJobArgs args)
        {
            if (args.TenantId != null)
            {
                using (CurrentTenant.Change(args.TenantId.Value))
                {
                    await DoWorkAsync(args);
                }
            }
            else
            {
                await DoWorkAsync(args);
            }
        }

        private async Task DoWorkAsync(MyReportJobArgs args)
        {
            _cancellationTokenProvider.Token.ThrowIfCancellationRequested();
            if (Guid.TryParse(args.Content, out var bookId))
            {
                var book = await _repository.GetAsync(bookId, cancellationToken: _cancellationTokenProvider.Token);
                if (book != null)
                {
                    Logger.LogInformation("Executing MyReportJob AND Found book '{0}'", book.Name);
                    await _localEventBus.PublishAsync(
                        new ReportCompleteEvent
                        {
                            Id = book.Id,
                            UserId = args.UserId ?? Guid.Empty,
                            TenantId = args.TenantId,
                            Message = $"Report for book '{book.Name}'"
                        }
                    );
                }
            }
            else
            {
                Logger.LogInformation("Executing MyReportJob with args: {0}", args.Content);
            }
        }
    }
}
