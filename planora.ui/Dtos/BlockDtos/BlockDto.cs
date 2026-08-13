namespace Planora.UI.Dtos.BlockDtos;

public class BlockGetDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public double DurationMinutes { get; set; }
    public int Priority { get; set; }
    public int Repeat { get; set; }
    public int Status { get; set; }
    public string Color { get; set; } = string.Empty;
    public bool IsAiCreated { get; set; }
    public int? ReminderMinutes { get; set; }
    public int UserId { get; set; }
}

public class BlockAddDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public int Priority { get; set; }
    public int Repeat { get; set; }
    public int Status { get; set; }
    public string Color { get; set; } = string.Empty;
    public bool IsAiCreated { get; set; }
    public int? ReminderMinutes { get; set; }
    public int UserId { get; set; }
}

public class BlockUpdateDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public int Priority { get; set; }
    public int Repeat { get; set; }
    public int Status { get; set; }
    public string Color { get; set; } = string.Empty;
    public bool IsAiCreated { get; set; }
    public int? ReminderMinutes { get; set; }
    public int UserId { get; set; }
}