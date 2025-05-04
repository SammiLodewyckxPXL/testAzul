using System.Drawing;
using Azul.Core.GameAggregate.Contracts;
using Azul.Core.PlayerAggregate.Contracts;

namespace Azul.Core.PlayerAggregate;

/// <inheritdoc cref="IPlayer"/>
internal class ComputerPlayer : PlayerBase
{
    private readonly IGamePlayStrategy _strategy;

    public ComputerPlayer(IGamePlayStrategy strategy) : base(Guid.NewGuid(), "Computer", null)
    {
        _strategy = strategy;
    }
}