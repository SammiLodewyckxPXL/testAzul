using AutoMapper;
using Azul.Core.BoardAggregate.Contracts;
using Azul.Core.TileFactoryAggregate.Contracts;

namespace Azul.Api.Models.Output;

public class PatternLineModel
{
    public int Length { get; set; }
    public TileType? TileType { get; set; }
    public int NumberOfTiles { get; set; }
    public bool IsComplete { get; set; }

    private class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<IPatternLine, PatternLineModel>();
        }
    }
}