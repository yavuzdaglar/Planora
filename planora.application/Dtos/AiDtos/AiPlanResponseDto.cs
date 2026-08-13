namespace Planora.Application.Dtos.AiDtos;

public class AiProposedBlockDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public double DurationMinutes { get; set; }
    public string Color { get; set; } = string.Empty;
    public int Priority { get; set; }
    public bool IsAiCreated { get; set; } = true;
}

public class AiConflictDto
{
    public string Message { get; set; } = string.Empty;
    public string NewBlockTitle { get; set; } = string.Empty;
    public string ExistingBlockTitle { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public List<string> Suggestions { get; set; } = new();
}

public class AiPlanResponseDto
{
    public bool Success { get; set; } = true;
    public string Message { get; set; } = string.Empty;
    public List<AiProposedBlockDto> ProposedBlocks { get; set; } = new();
    public List<AiConflictDto> Conflicts { get; set; } = new();
    public Dictionary<string, int> Summary { get; set; } = new();
}

public class AiApplyRequestDto
{
    public int UserId { get; set; }
    public List<AiProposedBlockDto> ProposedBlocks { get; set; } = new();
}