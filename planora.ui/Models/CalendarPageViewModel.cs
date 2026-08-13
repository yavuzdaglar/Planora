using Planora.UI.Dtos.BlockDtos;

namespace Planora.UI.Models;

public class CalendarPageViewModel
{
    public string View { get; set; } = "month"; // month | week | day
    public DateTime SelectedDate { get; set; } = DateTime.Today;
    public DateTime WeekStart { get; set; }
    public DateTime WeekEnd { get; set; }
    public List<BlockGetDto> Blocks { get; set; } = new();
    public List<DayBlocksViewModel> DayBlocks { get; set; } = new();
    public string MonthName { get; set; } = string.Empty;
    public int CurrentUserId { get; set; } = 5;
}

public class DayBlocksViewModel
{
    public DateTime Date { get; set; }
    public List<BlockGetDto> Blocks { get; set; } = new();
}

public class TodayDashboardViewModel
{
    public int CompletedCount { get; set; }
    public int TotalCount { get; set; }
    public double FocusMinutes { get; set; }
    public List<BlockGetDto> ComingUp { get; set; } = new();
}