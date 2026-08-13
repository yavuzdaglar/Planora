using AutoMapper;
using Planora.Application.Dtos.UserDtos;
using Planora.Entities;

namespace Planora.Application.Mapping;

public class UserMapping : Profile
{
    public UserMapping()
    {
        CreateMap<User, UserGetDto>();
        CreateMap<UserAddDto, User>();
        CreateMap<UserUpdateDto, User>();
    }
}