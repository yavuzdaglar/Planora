using Microsoft.AspNetCore.Mvc;
using Planora.Application.Dtos.AiDtos;
using Planora.Application.Interfaces;

namespace Planora.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AiController : ControllerBase
{
    private readonly IAiPlannerService _aiPlannerService;
    private readonly IAiCommandService _aiCommandService;

    public AiController(IAiPlannerService aiPlannerService, IAiCommandService aiCommandService)
    {
        _aiPlannerService = aiPlannerService;
        _aiCommandService = aiCommandService;
    }

    [HttpPost("plan")]
    public IActionResult GeneratePlan(AiPlanRequestDto request)
    {
        var response = _aiPlannerService.GeneratePlan(request);
        return Ok(response);
    }

    [HttpPost("apply")]
    public IActionResult ApplyPlan(AiApplyRequestDto request)
    {
        var applied = _aiPlannerService.ApplyPlan(request);
        return Ok(new { Message = $"{applied} blok eklendi.", AppliedCount = applied });
    }

    [HttpPost("command")]
    public IActionResult ExecuteCommand(AiCommandRequestDto request)
    {
        var response = _aiCommandService.Execute(request);
        return Ok(response);
    }
}