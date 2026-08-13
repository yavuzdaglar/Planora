using Planora.Application.Dtos.AiDtos;
using Planora.Application.Interfaces;
using Planora.Domain.Interfaces;
using Planora.Entities;

namespace Planora.Application.Services;

public class AiCommandService : IAiCommandService
{
    private readonly IBlockRepository _blockRepository;

    public AiCommandService(IBlockRepository blockRepository)
    {
        _blockRepository = blockRepository;
    }

    public AiCommandResponseDto Execute(AiCommandRequestDto request)
    {
        var command = request.Command.Trim();
        var lower = command.ToLowerInvariant();

        if (IsMoveCommand(lower)) return MoveBlocks(request);
        if (IsFreeCommand(lower)) return FreeDay(request, lower);
        if (IsReduceCommand(lower)) return ReduceDay(request);
        if (IsAddCommand(lower)) return AddTime(request);

        return new AiCommandResponseDto
        {
            Success = false,
            Message = "Komutu anlayamadım. Taşı, boşalt, azalt ya da ekle gibi bir komut kullanabilirsin.",
            Conflicts = new List<AiConflictDto>
            {
                new() { Message = "Örnek: \"Wednesday study'yi Thursday'a taşı\", \"Cuma akşamını boşalt\", \"bugünün yükünü azalt\", \"yarın .NET'e 2 saat ver\"" }
            }
        };
    }

    // ----- komut algılama -----

    private static bool IsMoveCommand(string lower)
    {
        return lower.Contains("taşı") || lower.Contains("tası") || lower.Contains(" move ") ||
               lower.StartsWith("move ") || lower.Contains(" yatay") || lower.Contains(" kaydır");
    }

    private static bool IsFreeCommand(string lower)
    {
        return lower.Contains("boşalt") || lower.Contains("bosalt") || lower.Contains("boş bırak") ||
               lower.Contains("bos birak") || lower.Contains("temizle") || lower.Contains(" serbest") ||
               lower.Contains(" free") || lower.Contains("clear ");
    }

    private static bool IsReduceCommand(string lower)
    {
        return lower.Contains("azalt") || lower.Contains(" yükünü") || lower.Contains(" reduce ") ||
               lower.Contains(" hafiflet") || lower.Contains(" less busy");
    }

    private static bool IsAddCommand(string lower)
    {
        return lower.Contains(" ver") || lower.StartsWith("ver ") || lower.Contains(" ekle") ||
               lower.Contains(" give") || lower.StartsWith("give ") ||
               lower.Contains(" add") || lower.Contains(" yer aç") || lower.Contains(" ayır ");
    }

    // ----- MOVE -----

    private AiCommandResponseDto MoveBlocks(AiCommandRequestDto request)
    {
        var command = request.Command;
        var (fromDay, toDay) = ExtractDays(command);
        if (fromDay == null || toDay == null)
        {
            return new AiCommandResponseDto
            {
                Success = false,
                Message = "Taşıma için iki gün bulamadım. Örnek: \"Monday'deki study'yi Thursday'a taşı\"",
                Conflicts = new List<AiConflictDto>()
            };
        }

        var now = request.StartDate ?? DateTime.Today;
        var referenceDate = now.Date;
        var fromDate = GetDayDateInWeek(referenceDate, fromDay.Value);
        var toDate = GetDayDateInWeek(referenceDate, toDay.Value);

        var (startRange, endRange) = GetRange(request, fromDate, toDate);
        var fromBlocks = _blockRepository
            .GetByDateRange(startRange, endRange)
            .Where(b => b.Date.Date == fromDate && b.UserId == request.UserId);

        var titleHint = ExtractTitle(command);
        var matched = string.IsNullOrEmpty(titleHint)
            ? fromBlocks.ToList()
            : fromBlocks.Where(b => MatchesTitle(b, titleHint)).ToList();

        if (matched.Count == 0)
        {
            return new AiCommandResponseDto
            {
                Success = false,
                Message = $"'{titleHint}' için {fromDay} ({fromDate:dd.MM}) gününde blok bulamadım.",
                Conflicts = new List<AiConflictDto>()
            };
        }

        var conflicts = new List<AiConflictDto>();
        var moved = 0;

        foreach (var block in matched)
        {
            var targetDate = toDate;
            var existing = _blockRepository.GetByDate(targetDate)
                .Where(b => b.Id != block.Id && b.UserId == request.UserId)
                .FirstOrDefault(b => Overlaps(block, b));

            if (existing != null)
            {
                conflicts.Add(new AiConflictDto
                {
                    Message = $"{toDay} ({targetDate:dd.MM}) günü {existing.StartTime:hh\\:mm}-{existing.EndTime:hh\\:mm} saatinde '{existing.Title}' var. Taşımak istediğin '{block.Title}' ile çakışıyor. Önce mevcut blok taşınmalı ya da süre değiştirilmeli.",
                    NewBlockTitle = block.Title,
                    ExistingBlockTitle = existing.Title,
                    Date = targetDate,
                    StartTime = block.StartTime,
                    EndTime = block.EndTime
                });
                continue;
            }

            block.Date = targetDate;
            _blockRepository.Update(block);
            moved++;
        }

        return new AiCommandResponseDto
        {
            Success = conflicts.Count == 0,
            Message = moved > 0
                ? $"'{matched.FirstOrDefault()?.Title}' {fromDay} ({fromDate:dd.MM}) → {toDay} ({toDate:dd.MM}) taşındı. ({moved} blok)"
                : "Hiçbir blok taşınamadı.",
            AffectedCount = moved,
            Conflicts = conflicts
        };
    }

    // ----- FREE -----

    private AiCommandResponseDto FreeDay(AiCommandRequestDto request, string lower)
    {
        var command = request.Command;
        var evening = lower.Contains("akşam") || lower.Contains("aksam") || lower.Contains("evening") || lower.Contains("night");
        var day = ExtractDays(command).ToDay ?? ExtractFirstDay(command);

        var referenceDate = (request.StartDate ?? DateTime.Today).Date;
        var targetDate = day.HasValue ? GetDayDateInWeek(referenceDate, day.Value) : referenceDate;

        var blocks = _blockRepository
            .GetByDateRange(targetDate, targetDate)
            .Where(b => b.UserId == request.UserId)
            .ToList();

        var toRemove = evening
            ? blocks.Where(b => b.StartTime >= new TimeSpan(18, 0, 0)).ToList()
            : blocks;

        if (toRemove.Count == 0)
        {
            return new AiCommandResponseDto
            {
                Success = true,
                Message = evening
                    ? $"{targetDate:dddd} ({targetDate:dd.MM}) akşamı zaten boş."
                    : $"{targetDate:dddd} ({targetDate:dd.MM}) günü zaten boş.",
                Conflicts = new List<AiConflictDto>()
            };
        }

        foreach (var b in toRemove) _blockRepository.Delete(b.Id);

        return new AiCommandResponseDto
        {
            Success = true,
            Message = $"{targetDate:dddd} ({targetDate:dd.MM}) {(evening ? "akşamı" : "günü")} boşaltıldı. ({toRemove.Count} blok silindi)",
            AffectedCount = toRemove.Count,
            Conflicts = new List<AiConflictDto>()
        };
    }

    // ----- REDUCE -----

    private AiCommandResponseDto ReduceDay(AiCommandRequestDto request)
    {
        var referenceDate = (request.StartDate ?? DateTime.Today).Date;

        var blocks = _blockRepository
            .GetByDateRange(referenceDate, referenceDate)
            .Where(b => b.UserId == request.UserId && b.Status != BlockStatus.Completed)
            .ToList();

        // Düşük ve orta öncelikli işleri kaldır, yüksek öncelikleri koru
        var toRemove = blocks
            .Where(b => b.Priority == Priority.Low || b.Priority == Priority.Medium)
            .ToList();

        if (toRemove.Count == 0)
        {
            return new AiCommandResponseDto
            {
                Success = true,
                Message = $"{referenceDate:dddd} ({referenceDate:dd.MM}) için azaltılabilecek düşük öncelikli iş bulamadım.",
                Conflicts = new List<AiConflictDto>()
            };
        }

        foreach (var b in toRemove) _blockRepository.Delete(b.Id);

        var kept = blocks.Count - toRemove.Count;
        return new AiCommandResponseDto
        {
            Success = true,
            Message = $"{referenceDate:dddd} ({referenceDate:dd.MM}) yükü azaltıldı. {toRemove.Count} düşük öncelikli iş silindi, {kept} yüksek öncelikli iş korundu.",
            AffectedCount = toRemove.Count,
            Conflicts = new List<AiConflictDto>()
        };
    }

    // ----- ADD -----

    private AiCommandResponseDto AddTime(AiCommandRequestDto request)
    {
        var lower = request.Command.ToLowerInvariant();
        var (hours, minutes) = ExtractDuration(request.Command);

        var day = ExtractDays(request.Command).ToDay ?? ExtractFirstDay(request.Command);
        var referenceDate = (request.StartDate ?? DateTime.Today).Date;
        var targetDate = day.HasValue
            ? GetDayDateInWeek(referenceDate, day.Value)
            : lower.Contains("yarın") || lower.Contains("yarin") || lower.Contains("tomorrow")
                ? referenceDate.AddDays(1)
                : referenceDate;

        var titleHint = ExtractTitle(request.Command);

        var slot = FindFreeSlot(request.UserId, targetDate, minutes);
        if (slot == null)
        {
            return new AiCommandResponseDto
            {
                Success = false,
                Message = $"{targetDate:dddd} ({targetDate:dd.MM}) gününde {hours} saat için uygun boş zaman bulamadım. Daha kısa bir süre ya da farklı bir gün deneyebilirsin.",
                Conflicts = new List<AiConflictDto>()
            };
        }

        var startTime = slot.Value;
        var endTime = startTime.Add(TimeSpan.FromMinutes(minutes));
        var suggested = new AiProposedBlockDto
        {
            Title = string.IsNullOrEmpty(titleHint) ? "Yeni Görev" : titleHint,
            Date = targetDate,
            StartTime = startTime,
            EndTime = endTime,
            DurationMinutes = minutes,
            Color = "#3b82f6",
            Priority = 1,
            IsAiCreated = true
        };

        return new AiCommandResponseDto
        {
            Success = true,
            Message = $"{targetDate:dddd} ({targetDate:dd.MM}) için {FormatDuration(minutes)} ({slot:hh\\:mm}-{endTime:hh\\:mm}) uygun bulundu.",
            SuggestedBlocks = new List<AiProposedBlockDto> { suggested },
            AffectedCount = 1,
            Conflicts = new List<AiConflictDto>()
        };
    }

    private static string FormatDuration(int minutes)
    {
        if (minutes % 60 == 0) return $"{minutes / 60} saat";
        if (minutes < 60) return $"{minutes} dakika";
        return $"{minutes / 60}.{minutes % 60} saat";
    }

    // ----- yardımcılar -----

    private TimeSpan? FindFreeSlot(int userId, DateTime date, int minutes)
    {
        var existing = _blockRepository
            .GetByDateRange(date, date)
            .Where(b => b.UserId == userId)
            .ToList();

        for (var time = new TimeSpan(8, 0, 0); time.Add(TimeSpan.FromMinutes(minutes)) <= new TimeSpan(21, 0, 0); time = time.Add(TimeSpan.FromMinutes(30)))
        {
            var end = time.Add(TimeSpan.FromMinutes(minutes));
            var clashes = existing.Any(b => b.StartTime < end && time < b.EndTime);
            if (!clashes) return time;
        }
        return null;
    }

    private static bool Overlaps(Block a, Block b)
    {
        return a.StartTime < b.EndTime && b.StartTime < a.EndTime;
    }

    private static bool MatchesTitle(Block block, string hint)
    {
        var keys = hint.Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Where(k => k.Length > 2)
            .ToArray();
        if (keys.Length == 0) return true;
        return keys.Any(k => block.Title.Contains(k, StringComparison.OrdinalIgnoreCase) ||
                             block.Description.Contains(k, StringComparison.OrdinalIgnoreCase));
    }

    private static (DateTime Start, DateTime End) GetRange(AiCommandRequestDto request, DateTime a, DateTime b)
    {
        if (request.StartDate.HasValue && request.EndDate.HasValue)
            return (request.StartDate.Value.Date, request.EndDate.Value.Date);
        return a < b ? (a, b) : (b, a);
    }

    private static DateTime GetDayDateInWeek(DateTime reference, DayOfWeek day)
    {
        var weekStart = reference.Date.AddDays(-(((int)reference.DayOfWeek + 6) % 7));
        return weekStart.AddDays(((int)day + 6) % 7);
    }

    private static (DayOfWeek? FromDay, DayOfWeek? ToDay) ExtractDays(string command)
    {
        var days = new List<string>();
        foreach (var weekDay in new[] { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday, DayOfWeek.Saturday, DayOfWeek.Sunday })
        {
            var names = DayNames(weekDay);
            if (names.Any(n => command.Contains(n, StringComparison.OrdinalIgnoreCase)))
                days.Add(weekDay.ToString());
        }

        if (days.Count >= 2)
        {
            var first = ParseDay(days[0]);
            var last = ParseDay(days[days.Count - 1]);
            return (first, last);
        }

        return (null, null);
    }

    private static DayOfWeek? ExtractFirstDay(string command)
    {
        foreach (var weekDay in new[] { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday, DayOfWeek.Saturday, DayOfWeek.Sunday })
        {
            if (DayNames(weekDay).Any(n => command.Contains(n, StringComparison.OrdinalIgnoreCase)))
                return weekDay;
        }
        return null;
    }

    private static DayOfWeek? ParseDay(string name)
    {
        foreach (var weekDay in new[] { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday, DayOfWeek.Saturday, DayOfWeek.Sunday })
        {
            if (DayNames(weekDay).Any(n => name.Equals(n, StringComparison.OrdinalIgnoreCase)))
                return weekDay;
        }
        return null;
    }

    private static string ExtractTitle(string command)
    {
        var lower = command.ToLowerInvariant();

        var markers = new[] { "study", ".net", "gym", "spor", "proje", "to do", "kanban", "ders", "okul", "toplantı", "toplanti", "meeting", "analiz", "analysis" };
        foreach (var m in markers)
        {
            if (lower.Contains(m))
            {
                var idx = command.IndexOf(m, StringComparison.OrdinalIgnoreCase);
                var sub = command.Substring(idx);
                var parts = sub.Split(new[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length > 0) return parts[0];
            }
        }
        return string.Empty;
    }

    private static (int Hours, int Minutes) ExtractDuration(string command)
    {
        var lower = command.ToLowerInvariant();
        var minutes = 60;

        // Genel desen: "<sayı> saat", "<sayı> dk/dakika" (ör. "2 saat", "90 dk", "45 dakika")
        var hoursMatch = System.Text.RegularExpressions.Regex.Match(lower, @"(\d+)\s*(?:saat|hour|hours|hrs?)");
        if (hoursMatch.Success)
        {
            minutes = int.Parse(hoursMatch.Groups[1].Value) * 60;
        }
        else
        {
            var minsMatch = System.Text.RegularExpressions.Regex.Match(lower, @"(\d+)\s*(?:dk|dakika|min|mins?)");
            if (minsMatch.Success)
                minutes = int.Parse(minsMatch.Groups[1].Value);
        }

        if (minutes <= 0) minutes = 60;
        return (minutes / 60, minutes);
    }

    private static string[] DayNames(DayOfWeek day)
    {
        return day switch
        {
            DayOfWeek.Monday => new[] { "monday", "mon", "pazartesi", "pzt" },
            DayOfWeek.Tuesday => new[] { "tuesday", "tue", "salı", "sali", "sal" },
            DayOfWeek.Wednesday => new[] { "wednesday", "wed", "çarşamba", "carsamba", "çar", "car" },
            DayOfWeek.Thursday => new[] { "thursday", "thu", "perşembe", "persembe", "per" },
            DayOfWeek.Friday => new[] { "friday", "fri", "cuma", "cum" },
            DayOfWeek.Saturday => new[] { "saturday", "sat", "cumartesi", "cmt" },
            _ => new[] { "sunday", "sun", "pazar", "paz" }
        };
    }
}