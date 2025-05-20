using Azul.Core.Util;

namespace Azul.Core.TableAggregate.Contracts
{
    public interface ITableRepository
    {
        /// <summary>
        /// Adds a table to storage.
        /// </summary>
        void Add(ITable table);

        /// <summary>
        /// Gets a table to storage.
        /// </summary>
        /// <exception cref="DataNotFoundException">When no table is found</exception>
        ITable Get(Guid tableId);

        /// <summary>
        /// Removes a table from storage.
        /// </summary>
        /// <param name="tableId">The identifier of the table</param>
        /// <exception cref="DataNotFoundException">When no table is found</exception>
        void Remove(Guid tableId);

        /// <summary>
        /// Find the tables in storage that have a seat available.
        /// Only tables that match the given <paramref name="preferences"/> are returned.
        /// </summary>
        IList<ITable> FindTablesWithAvailableSeats(ITablePreferences preferences);
    }
}
