using System;
using System.Threading.Tasks;
using Acme.BookStore.Reports;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Users;

namespace Acme.BookStore.Books
{
    public class BookAppService :
    CrudAppService<Book, BookDto, Guid, PagedAndSortedResultRequestDto, CreateUpdateBookDto>,
    IBookAppService
    {
        private readonly IBackgroundJobManager _backgroundJobManager;
        private readonly ICurrentUser _currentUser;

        public BookAppService(IRepository<Book, Guid> repository, IBackgroundJobManager backgroundJobManager, ICurrentUser currentUser)
            : base(repository)
        {
            _backgroundJobManager = backgroundJobManager;
            _currentUser = currentUser;
        }

        [Authorize]
        public async Task RunReportAsync(string content)
        {
            await _backgroundJobManager.EnqueueAsync(new MyReportJobArgs
            {
                Content = content,
                UserId = _currentUser.Id,
                TenantId = _currentUser.TenantId
            });
        }
    }
}
