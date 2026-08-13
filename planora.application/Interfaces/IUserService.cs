using Planora.Application.Dtos.UserDtos;

namespace Planora.Application.Interfaces;

public interface IUserService
{
    List<UserGetDto> GetAll();
    UserGetDto? GetById(int id);
    void Add(UserAddDto userAddDto);
    void Update(UserUpdateDto userUpdateDto);
    void Delete(int id);
}