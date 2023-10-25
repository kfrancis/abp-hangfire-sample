using Acme.BookStore.Reports;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.AspNetCore.Mvc.UI.RazorPages;
using Acme.BookStore.Books;

namespace Acme.BookStore.Web.Pages.Books
{
    public class IndexModel : BookStorePageModel
    {
        private readonly IBackgroundJobManager _backgroundJobManager;
        private readonly IBookAppService _bookAppService;

        [BindProperty(SupportsGet = true)]
        public string? ReportContent { get; set; }

        public IndexModel(IBackgroundJobManager backgroundJobManager, IBookAppService bookAppService)
        {
            _backgroundJobManager = backgroundJobManager;
            _bookAppService = bookAppService;
        }

        public void OnGet()
        {
        }

        public async Task OnPostAsync()
        {
            await _bookAppService.RunReportAsync(ReportContent ?? string.Empty);

            Alerts.Success("Job is queued!");
        }
    }
}
