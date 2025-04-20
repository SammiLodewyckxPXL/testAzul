using Azul.Core.TileFactoryAggregate.Contracts;

namespace Azul.Api.Models.Input;

public class TakeTilesModel
{
    public Guid DisplayId { get; set; }
    public TileType TileType { get; set; }
}