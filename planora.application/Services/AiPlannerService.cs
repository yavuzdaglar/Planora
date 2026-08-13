using Planora.Application.Dtos.AiDtos;
using Planora.Application.Interfaces;
using Planora.Domain.Interfaces;
using Planora.Entities;

namespace Planora.Application.Services;

public class AiPlannerService : IAiPlannerService
{
    private readonly IBlockRepository _blockRepository;

    private static readonly TimeSpan WorkDayStart = new(8, 0, 0);
    private static readonly TimeSpan WorkDayEnd = new(21, 0, 0);

    public AiPlannerService(IBlockRepository blockRepository)
    {
        _blockRepository = blockRepository;
    }

    public AiPlanResponseDto GeneratePlan(AiPlanRequestDto request)
    {
        var startDate = request.StartDate.Date;
        var endDate = startDate.AddDays(request.NumberOfDays - 1);

        var existingBlocks = _blockRepository.GetByDateRange(startDate, endDate);

        var proposed = new List<AiProposedBlockDto>();
        var conflicts = new List<AiConflictDto>();

        // Güne göre mevcut bloklar (çakışma kontrolü için)
        var occupied = BuildOccupancy(existingBlocks);

        // 1) Sabit bloklar (örneğin spor: Pzt-Çar-Cuma 18:00)
        PlaceFixedBlocks(request, occupied, proposed, conflicts);

        // 2) Görevler (örneğin her hafta içi 2 saat çalışma)
        PlaceTasks(request, startDate, endDate, occupied, proposed, conflicts);

        var response = new AiPlanResponseDto
        {
            Success = true,
            Message = $"Planora {proposed.Count} blok oluşturdu.",
            ProposedBlocks = proposed,
            Conflicts = conflicts,
            Summary = BuildSummary(proposed)
        };

        return response;
    }

    public int ApplyPlan(AiApplyRequestDto request)
    {
        var count = 0;
        foreach (var p in request.ProposedBlocks)
        {
            // Çakışan blok uygulanmasın (bloklar üst üste gelemez)
            var existing = _blockRepository.GetByDate(p.Date.Date);
            var overlaps = existing.Any(b =>
                b.UserId == request.UserId &&
                p.StartTime < b.EndTime && b.StartTime < p.EndTime);
            if (overlaps) continue;

            var block = new Block
            {
                Title = p.Title,
                Description = p.Description,
                Notes = string.Empty,
                Date = p.Date.Date,
                StartTime = p.StartTime,
                EndTime = p.EndTime,
                DurationMinutes = p.DurationMinutes > 0 ? p.DurationMinutes : (p.EndTime - p.StartTime).TotalMinutes,
                Priority = (Priority)p.Priority,
                Repeat = RepeatType.None,
                Status = BlockStatus.Pending,
                Color = p.Color,
                IsAiCreated = true,
                UserId = request.UserId
            };
            _blockRepository.Add(block);
            count++;
        }
        return count;
    }

    // ----- yardımcılar -----

    private static Dictionary<DateTime, List<(TimeSpan Start, TimeSpan End)>> BuildOccupancy(List<Block> blocks)
    {
        var occupied = new Dictionary<DateTime, List<(TimeSpan, TimeSpan)>>();
        foreach (var b in blocks)
        {
            var key = b.Date.Date;
            if (!occupied.ContainsKey(key)) occupied[key] = new List<(TimeSpan, TimeSpan)>();
            occupied[key].Add((b.StartTime, b.EndTime));
        }
        return occupied;
    }

    private void PlaceFixedBlocks(
        AiPlanRequestDto request,
        Dictionary<DateTime, List<(TimeSpan Start, TimeSpan End)>> occupied,
        List<AiProposedBlockDto> proposed,
        List<AiConflictDto> conflicts)
    {
        var startDate = request.StartDate.Date;

        foreach (var fb in request.FixedBlocks)
        {
            for (var i = 0; i < request.NumberOfDays; i++)
            {
                var date = startDate.AddDays(i);

                if (!fb.Days.Contains(date.DayOfWeek)) continue;
                if (request.FreeDays.Contains(date.DayOfWeek)) continue;

                if (OverlapsAny(occupied, date, fb.StartTime, fb.EndTime, out var existingTitle))
                {
                    conflicts.Add(new AiConflictDto
                    {
                        Message = $"{date:dddd} günü {fb.StartTime:hh\\:mm}-{fb.EndTime:hh\\:mm} saatinde {existingTitle} bloku var.",
                        NewBlockTitle = fb.Title,
                        ExistingBlockTitle = existingTitle,
                        Date = date,
                        StartTime = fb.StartTime,
                        EndTime = fb.EndTime,
                        Suggestions = new List<string>
                        {
                            $"{existingTitle} blokunu taşı",
                            $"{fb.Title} blokunu başka saate taşı",
                            "İkisini de koru"
                        }
                    });
                    continue;
                }

                var categoryColor = fb.Color;
                proposed.Add(new AiProposedBlockDto
                {
                    Title = fb.Title,
                    Description = fb.Description,
                    Date = date,
                    StartTime = fb.StartTime,
                    EndTime = fb.EndTime,
                    DurationMinutes = (fb.EndTime - fb.StartTime).TotalMinutes,
                    Color = string.IsNullOrEmpty(categoryColor) ? "#8b5cf6" : categoryColor,
                    Priority = (int)fb.Priority,
                    IsAiCreated = true
                });

                AddOccupied(occupied, date, fb.StartTime, fb.EndTime);
            }
        }
    }

    private void PlaceTasks(
        AiPlanRequestDto request,
        DateTime startDate,
        DateTime endDate,
        Dictionary<DateTime, List<(TimeSpan Start, TimeSpan End)>> occupied,
        List<AiProposedBlockDto> proposed,
        List<AiConflictDto> conflicts)
    {
        foreach (var task in request.Tasks)
        {
            for (var date = startDate; date <= endDate; date = date.AddDays(1))
            {
                if (!task.Days.Contains(date.DayOfWeek)) continue;
                if (request.FreeDays.Contains(date.DayOfWeek)) continue;
                if (task.Deadline.HasValue && date > task.Deadline.Value.Date) continue;

                var slot = FindFreeSlot(occupied, date, task.DurationMinutes, task.PreferredStartTime);
                if (slot == null)
                {
                    conflicts.Add(new AiConflictDto
                    {
                        Message = $"{date:dddd} {date:dd.MM} günü {task.Title} için uygun boş zaman bulunamadı.",
                        NewBlockTitle = task.Title,
                        Date = date
                    });
                    continue;
                }

                proposed.Add(new AiProposedBlockDto
                {
                    Title = task.Title,
                    Description = task.Description,
                    Date = date,
                    StartTime = slot.Value.Start,
                    EndTime = slot.Value.End,
                    DurationMinutes = task.DurationMinutes,
                    Color = task.Color,
                    Priority = (int)task.Priority,
                    IsAiCreated = true
                });

                AddOccupied(occupied, date, slot.Value.Start, slot.Value.End);
            }
        }
    }

    private static (TimeSpan Start, TimeSpan End)? FindFreeSlot(
        Dictionary<DateTime, List<(TimeSpan Start, TimeSpan End)>> occupied,
        DateTime date,
        int durationMinutes,
        TimeSpan? preferredStart = null)
    {
        var candidates = new List<TimeSpan>();

        // Önce tercih edilen saatten başla
        if (preferredStart.HasValue) candidates.Add(preferredStart.Value);

        // Ardından 30 dk adımlarla çalışma aralığında tara
        for (var t = WorkDayStart; t.Add(TimeSpan.FromMinutes(durationMinutes)) <= WorkDayEnd; t = t.Add(TimeSpan.FromMinutes(30)))
        {
            candidates.Add(t);
        }

        foreach (var start in candidates)
        {
            var end = start.Add(TimeSpan.FromMinutes(durationMinutes));
            if (end > WorkDayEnd) continue;
            if (!OverlapsAny(occupied, date, start, end, out _)) return (start, end);
        }

        return null;
    }

    private static bool OverlapsAny(
        Dictionary<DateTime, List<(TimeSpan Start, TimeSpan End)>> occupied,
        DateTime date,
        TimeSpan start,
        TimeSpan end,
        out string existingTitle)
    {
        existingTitle = string.Empty;
        if (occupied.TryGetValue(date.Date, out var slots))
        {
            foreach (var (s, e) in slots)
            {
                if (start < e && s < end)
                {
                    existingTitle = "Bilinmeyen blok";
                    return true;
                }
            }
        }
        return false;
    }

    private static void AddOccupied(
        Dictionary<DateTime, List<(TimeSpan Start, TimeSpan End)>> occupied,
        DateTime date,
        TimeSpan start,
        TimeSpan end)
    {
        if (!occupied.ContainsKey(date.Date)) occupied[date.Date] = new List<(TimeSpan, TimeSpan)>();
        occupied[date.Date].Add((start, end));
    }

    private static Dictionary<string, int> BuildSummary(List<AiProposedBlockDto> proposed)
    {
        var summary = new Dictionary<string, int>();
        foreach (var p in proposed)
        {
            var name = p.Title;
            summary[name] = summary.GetValueOrDefault(name) + 1;
        }
        return summary;
    }
}