using AutoMapper;
using Planora.Application.Dtos.UserDtos;
using Planora.Application.Interfaces;
using Planora.Domain.Interfaces;
using Planora.Entities;

namespace Planora.Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;

    public UserService(IUserRepository userRepository, IMapper mapper)
    {
        _userRepository = userRepository;
        _mapper = mapper;
    }

    public List<UserGetDto> GetAll()
    {
        var users = _userRepository.GetAll();
        return _mapper.Map<List<UserGetDto>>(users);
    }

    public UserGetDto? GetById(int id)
    {
        var user = _userRepository.GetById(id);
        if (user == null) return null;
        return _mapper.Map<UserGetDto>(user);
    }

    public void Add(UserAddDto userAddDto)
    {
        var user = _mapper.Map<User>(userAddDto);
        _userRepository.Add(user);
    }

    public void Update(UserUpdateDto userUpdateDto)
    {
        var user = _mapper.Map<User>(userUpdateDto);
        _userRepository.Update(user);
    }

    public void Delete(int id)
    {
        _userRepository.Delete(id);
    }
}