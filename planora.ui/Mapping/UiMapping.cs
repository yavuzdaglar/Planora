using AutoMapper;
using AppBlock = Planora.Application.Dtos.BlockDtos;
using UIBlock = Planora.UI.Dtos.BlockDtos;

namespace Planora.UI.Mapping;

public class UiMapping : Profile
{
    public UiMapping()
    {
        // Blok
        CreateMap<AppBlock.BlockGetDto, UIBlock.BlockGetDto>().ReverseMap();
        CreateMap<AppBlock.BlockAddDto, UIBlock.BlockAddDto>().ReverseMap();
        CreateMap<AppBlock.BlockUpdateDto, UIBlock.BlockUpdateDto>().ReverseMap();
    }
}