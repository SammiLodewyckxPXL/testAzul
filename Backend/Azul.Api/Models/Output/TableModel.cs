using AutoMapper;
using Azul.Core.TableAggregate;
using Azul.Core.TableAggregate.Contracts;

namespace Azul.Api.Models.Output
{
    public class TableModel
    {
        public Guid Id { get; set; }
        public ITablePreferences Preferences { get; set; } = new TablePreferences();
        public List<PlayerModel> SeatedPlayers { get; set; } = new List<PlayerModel>();
        public bool HasAvailableSeat { get; set; }
        public Guid GameId { get; set; }

        private class MappingProfile : Profile
        {
            public MappingProfile()
            {
                CreateMap<ITable, TableModel>();
            }
        }
    }
}
