using Azul.Core.PlayerAggregate.Contracts;
using Azul.Core.TableAggregate;
using Azul.Core.TableAggregate.Contracts;
using Azul.Core.UserAggregate;
using Moq;

namespace Azul.Core.Tests.Builders;

public class TableMockBuilder : MockBuilder<ITable>
{
    private ITablePreferences _tablePreferences = new TablePreferencesBuilder().Build();

    public TableMockBuilder()
    {
        Mock.SetupGet(t => t.Id).Returns(Guid.NewGuid());
        Mock.SetupGet(t => t.SeatedPlayers).Returns([]);
        Mock.SetupGet(t => t.HasAvailableSeat).Returns(true);
        Mock.SetupGet(t => t.GameId).Returns(Guid.Empty);
        Mock.SetupGet(t => t.Preferences).Returns(() => _tablePreferences);
    }

    public TableMockBuilder WithPreferences(ITablePreferences tablePreferences)
    {
        _tablePreferences = tablePreferences;
        return this;
    }

    public TableMockBuilder WithSeatedUsers(User[] users)
    {
        IPlayer[] players = new IPlayer[users.Length];
        for (int i = 0; i < users.Length; i++)
        {
            IPlayer player = new PlayerMockBuilder()
                .BasedOnUser(users[i])
                .Object;
            players[i] = player;
        }
        Mock.SetupGet(t => t.SeatedPlayers).Returns(() => players);
        Mock.Setup(table => table.Leave(It.IsAny<Guid>())).Callback((Guid playerId) =>
        {
            players = players.Where(p => p.Id != playerId).ToArray();
        });
        Mock.SetupGet(t => t.HasAvailableSeat).Returns(() => players.Length < _tablePreferences.NumberOfPlayers);

        return this;
    }
}