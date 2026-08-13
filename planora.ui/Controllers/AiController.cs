using Microsoft.AspNetCore.Mvc;
using Planora.UI.Dtos.AiDtos;
using System.Net.Http.Json;

namespace Planora.UI.Controllers;

public class AiController : Controller
{
    private readonly HttpClient _httpClient;
    private const string BaseUrl = "http://localhost:5210/";

    public AiController(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri(BaseUrl);
    }

    public async Task<IActionResult> Index()
    {
        return RedirectToAction("Index", "Calendar", new { view = "month" });
    }

    [HttpPost]
    public async Task<IActionResult> Plan([FromBody] AiPlanRequestDto request)
    {
        var response = await _httpClient.PostAsJsonAsync("api/ai/plan", request);
        var result = await response.Content.ReadFromJsonAsync<AiPlanResponseDto>();
        return Json(result);
    }

    [HttpPost]
    public async Task<IActionResult> Command([FromBody] AiCommandRequestDto request)
    {
        var response = await _httpClient.PostAsJsonAsync("api/ai/command", request);
        var result = await response.Content.ReadFromJsonAsync<AiCommandResponseDto>();
        return Json(result);
    }

    [HttpPost]
    public async Task<IActionResult> Apply([FromBody] AiApplyRequestDto request)
    {
        var response = await _httpClient.PostAsJsonAsync("api/ai/apply", request);
        var result = await response.Content.ReadFromJsonAsync<object>();
        return Json(result);
    }
}