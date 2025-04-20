using AutoMapper;
using Azul.Core.TileFactoryAggregate.Contracts;

namespace Azul.Api.Models.Output;

public class TableCenterModel : FactoryDisplayModel
{
    private class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<ITableCenter, TableCenterModel>();
        }
    }
}