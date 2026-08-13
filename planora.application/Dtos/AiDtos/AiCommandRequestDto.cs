namespace Planora.Application.Dtos.AiDtos;

public class AiCommandRequestDto
{
    public int UserId { get; set; }
    public string Command { get; set; } = string.Empty;
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

public class AiCommandResponseDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<AiProposedBlockDto> SuggestedBlocks { get; set; } = new();
    public List<AiConflictDto> Conflicts { get; set; } = new();
    public int AffectedCount { get; set; }
}