using Planora.Application.Dtos.AiDtos;

namespace Planora.Application.Interfaces;

public interface IAiCommandService
{
    AiCommandResponseDto Execute(AiCommandRequestDto request);
}