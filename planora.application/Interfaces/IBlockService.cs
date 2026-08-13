using Planora.Application.Dtos.BlockDtos;
using Planora.Entities;

namespace Planora.Application.Interfaces;

public interface IBlockService
{
    List<BlockGetDto> GetAll();
    List<BlockGetDto> GetByDate(DateTime date);
    List<BlockGetDto> GetByDateRange(DateTime startDate, DateTime endDate);
    BlockGetDto? GetById(int id);
    void Add(BlockAddDto blockAddDto);
    void Update(BlockUpdateDto blockUpdateDto);
    void UpdateStatus(int id, BlockStatus status);
    void Delete(int id);
}