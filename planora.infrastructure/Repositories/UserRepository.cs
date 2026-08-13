using Microsoft.EntityFrameworkCore;
using Planora.Domain.Interfaces;
using Planora.Entities;
using Planora.Infrastructure.Context;

namespace Planora.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly PlanoraDbContext _context;

    public UserRepository(PlanoraDbContext context)
    {
        _context = context;
    }

    public List<User> GetAll()
    {
        return _context.Users.ToList();
    }

    public User? GetById(int id)
    {
        return _context.Users.Find(id);
    }

    public void Add(User user)
    {
        _context.Users.Add(user);
        _context.SaveChanges();
    }

    public void Update(User user)
    {
        var tracked = _context.Users.Local.FirstOrDefault(u => u.Id == user.Id);
        if (tracked != null)
        {
            _context.Entry(tracked).CurrentValues.SetValues(user);
            _context.Entry(tracked).State = EntityState.Modified;
        }
        else
        {
            _context.Users.Attach(user);
            _context.Entry(user).State = EntityState.Modified;
        }
        _context.SaveChanges();
    }

    public void Delete(int id)
    {
        var user = _context.Users.Find(id);
        if (user == null) return;

        var hasBlocks = _context.Blocks.Any(b => b.UserId == id);
        if (hasBlocks)
            throw new InvalidOperationException("Bu kullanıcıya bağlı bloklar var. Kullanıcıyı silmek için önce bağlı blokları silmelisiniz.");

        _context.Users.Remove(user);
        _context.SaveChanges();
    }
}