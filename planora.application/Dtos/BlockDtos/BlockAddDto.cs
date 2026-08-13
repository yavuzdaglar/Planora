using Planora.Entities;

namespace Planora.Application.Dtos.BlockDtos;

public class BlockAddDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;

    public DateTime Date { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }

    public Priority Priority { get; set; }
    public RepeatType Repeat { get; set; }
    public BlockStatus Status { get; set; }

    public string Color { get; set; } = string.Empty;
    public bool IsAiCreated { get; set; }

    public int? ReminderMinutes { get; set; }

    public int UserId { get; set; }
}

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

    public Priority Priority { get; set; }
    public RepeatType Repeat { get; set; }
    public BlockStatus Status { get; set; }

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

    public Priority Priority { get; set; }
    public RepeatType Repeat { get; set; }
    public BlockStatus Status { get; set; }

    public string Color { get; set; } = string.Empty;
    public bool IsAiCreated { get; set; }

    public int? ReminderMinutes { get; set; }

    public int UserId { get; set; }
}