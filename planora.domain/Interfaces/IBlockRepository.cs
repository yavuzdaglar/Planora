using Planora.Entities;

namespace Planora.Domain.Interfaces;

public interface IBlockRepository
{
    List<Block> GetAll();
    List<Block> GetByDate(DateTime date);
    List<Block> GetByDateRange(DateTime startDate, DateTime endDate);
    Block? GetById(int id);
    void Add(Block block);
    void Update(Block block);
    void Delete(int id);
}