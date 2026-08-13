using AutoMapper;
using Planora.Application.Dtos.BlockDtos;
using Planora.Application.Interfaces;
using Planora.Domain.Interfaces;
using Planora.Entities;

namespace Planora.Application.Services;

public class BlockService : IBlockService
{
    private readonly IBlockRepository _blockRepository;
    private readonly IMapper _mapper;

    public BlockService(IBlockRepository blockRepository, IMapper mapper)
    {
        _blockRepository = blockRepository;
        _mapper = mapper;
    }

    public List<BlockGetDto> GetAll()
    {
        var blocks = _blockRepository.GetAll();
        return _mapper.Map<List<BlockGetDto>>(blocks);
    }

    public List<BlockGetDto> GetByDate(DateTime date)
    {
        var blocks = _blockRepository.GetByDate(date);
        return _mapper.Map<List<BlockGetDto>>(blocks);
    }

    public List<BlockGetDto> GetByDateRange(DateTime startDate, DateTime endDate)
    {
        var blocks = _blockRepository.GetByDateRange(startDate, endDate);
        return _mapper.Map<List<BlockGetDto>>(blocks);
    }

    public BlockGetDto? GetById(int id)
    {
        var block = _blockRepository.GetById(id);
        if (block == null) return null;
        return _mapper.Map<BlockGetDto>(block);
    }

    public void Add(BlockAddDto blockAddDto)
    {
        ValidateTimes(blockAddDto.StartTime, blockAddDto.EndTime);
        EnsureNoOverlap(blockAddDto.Date, blockAddDto.UserId, 0, blockAddDto.StartTime, blockAddDto.EndTime);
        var block = _mapper.Map<Block>(blockAddDto);
        block.DurationMinutes = (block.EndTime - block.StartTime).TotalMinutes;
        _blockRepository.Add(block);
    }

    public void Update(BlockUpdateDto blockUpdateDto)
    {
        ValidateTimes(blockUpdateDto.StartTime, blockUpdateDto.EndTime);
        EnsureNoOverlap(blockUpdateDto.Date, blockUpdateDto.UserId, blockUpdateDto.Id, blockUpdateDto.StartTime, blockUpdateDto.EndTime);
        var block = _mapper.Map<Block>(blockUpdateDto);
        block.DurationMinutes = (block.EndTime - block.StartTime).TotalMinutes;
        _blockRepository.Update(block);
    }

    public void UpdateStatus(int id, BlockStatus status)
    {
        var block = _blockRepository.GetById(id);
        if (block == null) throw new ArgumentException("Blok bulunamadı.");
        block.Status = status;
        _blockRepository.Update(block);
    }

    public void Delete(int id)
    {
        _blockRepository.Delete(id);
    }

    private static void ValidateTimes(TimeSpan startTime, TimeSpan endTime)
    {
        if (endTime <= startTime)
            throw new ArgumentException("Bitiş saati başlangıç saatinden sonra olmalıdır.");
    }

    private void EnsureNoOverlap(DateTime date, int userId, int excludeId, TimeSpan startTime, TimeSpan endTime)
    {
        foreach (var b in _blockRepository.GetByDate(date))
        {
            if (b.UserId != userId) continue;
            if (b.Id == excludeId) continue;
            if (startTime < b.EndTime && b.StartTime < endTime)
                throw new ArgumentException(
                    $"Bu saat diliminde başka bir blok var: {b.StartTime:hh\\:mm} – {b.EndTime:hh\\:mm} ({b.Title}). Bloklar üst üste gelemez.");
        }
    }
}