using Azul.Core.TableAggregate.Contracts;
using Azul.Core.UserAggregate;

namespace Azul.Core.TableAggregate;

/// <inheritdoc cref="ITableFactory"/>
internal class TableFactory : ITableFactory
{
    public ITable CreateNewForUser(User user, ITablePreferences preferences)
    {
        ITable table = new Table(Guid.NewGuid(), preferences);
        table.Join(user);
        return table;
    }
}