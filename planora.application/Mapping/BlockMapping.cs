using AutoMapper;
using Planora.Application.Dtos.BlockDtos;
using Planora.Entities;

namespace Planora.Application.Mapping;

public class BlockMapping : Profile
{
    public BlockMapping()
    {
        CreateMap<Block, BlockGetDto>();
        CreateMap<BlockAddDto, Block>();
        CreateMap<BlockUpdateDto, Block>();
    }
}