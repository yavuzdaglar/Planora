using Planora.Application.Dtos.AiDtos;

namespace Planora.Application.Interfaces;

public interface IAiPlannerService
{
    AiPlanResponseDto GeneratePlan(AiPlanRequestDto request);
    int ApplyPlan(AiApplyRequestDto request);
}