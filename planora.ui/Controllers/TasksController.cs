using Microsoft.AspNetCore.Mvc;
using Planora.UI.Dtos.BlockDtos;
using Planora.UI.Models;
using System.Net.Http.Json;

namespace Planora.UI.Controllers;

public class TasksController : Controller
{
    private readonly HttpClient _httpClient;
    private const string BaseUrl = "http://localhost:5210/";

    public TasksController(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri(BaseUrl);
    }

    public async Task<IActionResult> Index()
    {
        var model = new TodayDashboardViewModel();
        try
        {
            var blocks = await _httpClient.GetFromJsonAsync<List<BlockGetDto>>($"api/blocks/date/{DateTime.Today:yyyy-MM-dd}");
            var todayBlocks = blocks ?? new List<BlockGetDto>();

            model.TotalCount = todayBlocks.Count;
            model.CompletedCount = todayBlocks.Count(b => b.Status == 2);
            model.FocusMinutes = todayBlocks.Where(b => b.Status != 2).Sum(b => b.DurationMinutes);
            model.ComingUp = todayBlocks.Where(b => b.Status != 2).OrderBy(b => b.StartTime).Take(5).ToList();
        }
        catch { }

        return View(model);
    }
}