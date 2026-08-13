using Microsoft.EntityFrameworkCore;
using Planora.Domain.Interfaces;
using Planora.Entities;
using Planora.Infrastructure.Context;

namespace Planora.Infrastructure.Repositories;

public class BlockRepository : IBlockRepository
{
    private readonly PlanoraDbContext _context;

    public BlockRepository(PlanoraDbContext context)
    {
        _context = context;
    }

    public List<Block> GetAll()
    {
        return _context.Blocks.ToList();
    }

    public List<Block> GetByDate(DateTime date)
    {
        return _context.Blocks
            .Where(b => b.Date.Date == date.Date)
            .ToList();
    }

    public List<Block> GetByDateRange(DateTime startDate, DateTime endDate)
    {
        var start = startDate.Date;
        var end = endDate.Date;

        return _context.Blocks
            .Where(b => b.Date.Date >= start && b.Date.Date <= end)
            .ToList();
    }

    public Block? GetById(int id)
    {
        return _context.Blocks
            .FirstOrDefault(b => b.Id == id);
    }

    public void Add(Block block)
    {
        _context.Blocks.Add(block);
        _context.SaveChanges();
    }

    public void Update(Block block)
    {
        // Aynı anahtarla zaten takip edilen bir örnek varsa onun üzerine yaz (identity çakışması olmasın)
        var tracked = _context.Blocks.Local.FirstOrDefault(b => b.Id == block.Id);
        if (tracked != null)
        {
            _context.Entry(tracked).CurrentValues.SetValues(block);
            _context.Entry(tracked).State = EntityState.Modified;
        }
        else
        {
            _context.Blocks.Attach(block);
            _context.Entry(block).State = EntityState.Modified;
        }
        _context.SaveChanges();
    }

    public void Delete(int id)
    {
        var block = _context.Blocks.Find(id);
        if (block != null)
        {
            _context.Blocks.Remove(block);
            _context.SaveChanges();
        }
    }
}