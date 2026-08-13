using Planora.Entities;

namespace Planora.Application.Dtos.AiDtos;

public class AiTaskInputDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int DurationMinutes { get; set; } = 60;
    public Priority Priority { get; set; } = Priority.Medium;
    public string Color { get; set; } = "#3b82f6";

    // Hangi günler: Monday..Sunday
    public List<DayOfWeek> Days { get; set; } = new();
    public TimeSpan? PreferredStartTime { get; set; }
    public DateTime? Deadline { get; set; }
}

public class AiFixedBlockInputDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public Priority Priority { get; set; } = Priority.Medium;
    public string Color { get; set; } = string.Empty;
    public List<DayOfWeek> Days { get; set; } = new();
}

public class AiFreeDayInputDto
{
    public DayOfWeek Day { get; set; }
}

public class AiPlanRequestDto
{
    public int UserId { get; set; }
    public DateTime StartDate { get; set; }
    public int NumberOfDays { get; set; } = 7;
    public List<AiTaskInputDto> Tasks { get; set; } = new();
    public List<AiFixedBlockInputDto> FixedBlocks { get; set; } = new();
    public List<DayOfWeek> FreeDays { get; set; } = new();
}