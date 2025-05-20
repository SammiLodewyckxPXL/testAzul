using Azul.Core.TableAggregate;

namespace Azul.Core.Tests.Builders;

public class TablePreferencesBuilder
{
    private readonly TablePreferences _preferences;

    public TablePreferencesBuilder()
    {
        _preferences = new TablePreferences
        {
            NumberOfPlayers = 2,
            NumberOfArtificialPlayers = 0
        };
    }

    public TablePreferencesBuilder WithNumberOfPlayers(int numberOfPlayers)
    {
        _preferences.NumberOfPlayers = numberOfPlayers;
        return this;
    }

    public TablePreferences Build()
    {
        return _preferences;
    }

   
}
