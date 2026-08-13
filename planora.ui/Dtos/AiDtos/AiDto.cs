namespace Planora.UI.Dtos.AiDtos;

public class AiPlanRequestDto
{
    public int UserId { get; set; }
    public DateTime StartDate { get; set; }
    public int NumberOfDays { get; set; } = 7;
    public List<AiTaskInputDto> Tasks { get; set; } = new();
    public List<AiFixedBlockInputDto> FixedBlocks { get; set; } = new();
    public List<int> FreeDays { get; set; } = new();
}

public class AiTaskInputDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int DurationMinutes { get; set; } = 60;
    public int Priority { get; set; } = 1;
    public string Color { get; set; } = "#3b82f6";
    public List<int> Days { get; set; } = new();
    public string? PreferredStartTime { get; set; }
}

public class AiFixedBlockInputDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string StartTime { get; set; } = "18:00:00";
    public string EndTime { get; set; } = "19:00:00";
    public int Priority { get; set; } = 2;
    public string Color { get; set; } = string.Empty;
    public List<int> Days { get; set; } = new();
}

public class AiProposedBlockDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string StartTime { get; set; } = string.Empty;
    public string EndTime { get; set; } = string.Empty;
    public double DurationMinutes { get; set; }
    public string Color { get; set; } = string.Empty;
    public int Priority { get; set; }
    public bool IsAiCreated { get; set; } = true;
}

public class AiPlanResponseDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<AiProposedBlockDto> ProposedBlocks { get; set; } = new();
    public List<AiConflictDto> Conflicts { get; set; } = new();
    public Dictionary<string, int> Summary { get; set; } = new();
}

public class AiConflictDto
{
    public string Message { get; set; } = string.Empty;
    public string NewBlockTitle { get; set; } = string.Empty;
    public string ExistingBlockTitle { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string StartTime { get; set; } = string.Empty;
    public string EndTime { get; set; } = string.Empty;
    public List<string> Suggestions { get; set; } = new();
}

public class AiCommandRequestDto
{
    public int UserId { get; set; }
    public string Command { get; set; } = string.Empty;
    public DateTime? StartDate { get; set; }
}

public class AiCommandResponseDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<AiProposedBlockDto> SuggestedBlocks { get; set; } = new();
    public List<AiConflictDto> Conflicts { get; set; } = new();
    public int AffectedCount { get; set; }
}

public class AiApplyRequestDto
{
    public int UserId { get; set; }
    public List<AiProposedBlockDto> ProposedBlocks { get; set; } = new();
}