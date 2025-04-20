namespace Azul.Core.GameAggregate.Contracts;

/// <summary>
/// Represents a repository for games
/// </summary>
public interface IGameRepository
{
    void Add(IGame newGame);
    IGame GetById(Guid id);
}