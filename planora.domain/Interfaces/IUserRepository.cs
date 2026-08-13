using Planora.Entities;

namespace Planora.Domain.Interfaces;

public interface IUserRepository
{
    List<User> GetAll();
    User? GetById(int id);
    void Add(User user);
    void Update(User user);
    void Delete(int id);
}