using Microsoft.AspNetCore.Mvc;

namespace Planora.UI.ViewComponents;

public class _CalendarSidebarViewComponent : ViewComponent
{
    public IViewComponentResult Invoke(DateTime selectedDate)
    {
        ViewBag.SelectedDate = selectedDate;
        return View();
    }
}