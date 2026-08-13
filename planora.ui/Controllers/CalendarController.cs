using Microsoft.AspNetCore.Mvc;
using Planora.UI.Dtos.BlockDtos;
using Planora.UI.Models;
using System.Globalization;
using System.Net.Http.Json;

namespace Planora.UI.Controllers;

public class CalendarController : Controller
{
    private readonly HttpClient _httpClient;
    private const string BaseUrl = "http://localhost:5210/";

    public CalendarController(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri(BaseUrl);
    }

    public async Task<IActionResult> Index(string view = "month", DateTime? date = null)
    {
        var selectedDate = date ?? DateTime.Today;
        ViewBag.SelectedDate = selectedDate;
        var model = new CalendarPageViewModel
        {
            View = string.IsNullOrEmpty(view) ? "month" : view,
            SelectedDate = selectedDate,
            MonthName = CultureInfo.GetCultureInfo("tr-TR").DateTimeFormat.GetMonthName(selectedDate.Month),
            WeekStart = selectedDate.AddDays(-((int)selectedDate.DayOfWeek + 6) % 7),
            WeekEnd = selectedDate.AddDays(6 - ((int)selectedDate.DayOfWeek + 6) % 7)
        };

        try
        {
            var start = model.View switch
            {
                "month" => new DateTime(selectedDate.Year, selectedDate.Month, 1).AddDays(-7),
                "week" => model.WeekStart,
                _ => selectedDate
            };
            var end = model.View switch
            {
                "month" => new DateTime(selectedDate.Year, selectedDate.Month, 1).AddMonths(1).AddDays(14),
                "week" => model.WeekEnd,
                _ => selectedDate
            };

            var blocks = await _httpClient.GetFromJsonAsync<List<BlockGetDto>>($"api/blocks/range?startDate={start:yyyy-MM-dd}&endDate={end:yyyy-MM-dd}");
            model.Blocks = blocks ?? new List<BlockGetDto>();

            // Gün bazlı grupla
            if (model.View == "week" || model.View == "month")
            {
                for (var d = model.WeekStart; d <= model.WeekEnd; d = d.AddDays(1))
                {
                    model.DayBlocks.Add(new DayBlocksViewModel
                    {
                        Date = d,
                        Blocks = model.Blocks.Where(b => b.Date.Date == d.Date).OrderBy(b => b.StartTime).ToList()
                    });
                }
            }
        }
        catch
        {
            // API yoksa boş model ile sayfa göster
        }

        return View(model);
    }

    public async Task<IActionResult> Day(DateTime? date)
    {
        var selectedDate = date ?? DateTime.Today;
        ViewBag.SelectedDate = selectedDate;
        var model = new CalendarPageViewModel
        {
            View = "day",
            SelectedDate = selectedDate,
            MonthName = CultureInfo.GetCultureInfo("tr-TR").DateTimeFormat.GetMonthName(selectedDate.Month),
            WeekStart = selectedDate.AddDays(-((int)selectedDate.DayOfWeek + 6) % 7),
            WeekEnd = selectedDate.AddDays(6 - ((int)selectedDate.DayOfWeek + 6) % 7)
        };

        try
        {
            var blocks = await _httpClient.GetFromJsonAsync<List<BlockGetDto>>($"api/blocks/date/{selectedDate:yyyy-MM-dd}");
            model.Blocks = blocks ?? new List<BlockGetDto>();
        }
        catch { }

        return View(model);
    }
}