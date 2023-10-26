using System.Threading.Tasks;
using Acme.BookStore.Localization;
using Acme.BookStore.Reports;
using Volo.Abp.AspNetCore.Mvc.UI.RazorPages;
using Volo.Abp.EventBus;

namespace Acme.BookStore.Web.Pages;

public abstract class BookStorePageModel : AbpPageModel,
    ILocalEventHandler<ReportCompleteEvent>
{
    protected BookStorePageModel()
    {
        LocalizationResourceType = typeof(BookStoreResource);
    }

    public Task HandleEventAsync(ReportCompleteEvent eventData)
    {
        Alerts.Success("Job is completed!");

        return Task.CompletedTask;
    }
}
