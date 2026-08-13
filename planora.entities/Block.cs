namespace Planora.Entities;

public class Block
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

    // Kullanıcı
    public int UserId { get; set; }
    public User User { get; set; } = null!;
}