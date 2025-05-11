using AutoMapper;
using Azul.Core.TileFactoryAggregate.Contracts;

namespace Azul.Api.Models.Output;

public class TileFactoryModel
{
    public List<FactoryDisplayModel> Displays { get; set; }
    public TableCenterModel TableCenter { get; set; }
    public bool IsEmpty { get; set; }

    private class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<ITileFactory, TileFactoryModel>();
        }
    }
}