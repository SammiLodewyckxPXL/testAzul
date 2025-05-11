using Azul.Core.GameAggregate.Contracts;

namespace Azul.Core.Tests.Builders;

public class GameMockBuilder : MockBuilder<IGame>
{
    public GameMockBuilder()
    {
        Mock.SetupGet(t => t.Id).Returns(Guid.NewGuid());
    }
}