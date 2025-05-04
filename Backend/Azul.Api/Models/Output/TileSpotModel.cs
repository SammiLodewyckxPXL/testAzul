using AutoMapper;
using Azul.Core.BoardAggregate;
using Azul.Core.TileFactoryAggregate.Contracts;

namespace Azul.Api.Models.Output;

public class TileSpotModel
{
    public TileType? Type { get; set; }
    public bool HasTile { get; set; }

    private class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<TileSpot, TileSpotModel>();
        }
    }
}