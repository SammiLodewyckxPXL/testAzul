using System.Drawing;
using Azul.Core.BoardAggregate;
using Azul.Core.BoardAggregate.Contracts;
using Azul.Core.PlayerAggregate.Contracts;
using Azul.Core.TileFactoryAggregate.Contracts;

namespace Azul.Core.PlayerAggregate;

/// <inheritdoc cref="IPlayer"/>
internal class PlayerBase: IPlayer
{
    public Guid Id { get; set; }

    public string Name { get; set; }

    public DateOnly? LastVisitToPortugal { get; set; }

    public IBoard Board { get; set; }

    public bool HasStartingTile { get; set; }

    public List<TileType> TilesToPlace { get; set; }
    protected PlayerBase(Guid id, string name, DateOnly? lastVisitToPortugal)
    {
        Id = id;
        Name = name;
        LastVisitToPortugal = lastVisitToPortugal;
        Board = new Board();  //pas als board is geimplementeerd
        HasStartingTile = false;
        TilesToPlace = new List<TileType>();
    }
}