using Azul.Api.Controllers;
using Guts.Client.Core;

namespace Azul.Api.Tests
{
    [ProjectComponentTestFixture("1TINProject", "Azul", "TablesIntegration",
        @"Azul.Api\Controllers\TablesController.cs;
Azul.Core\TableAggregate\TableManager.cs;
Azul.Core\TableAggregate\TableFactory.cs;
Azul.Core\TableAggregate\TablePreferences.cs;
Azul.Core\TableAggregate\Table.cs;
Azul.Infrastructure\InMemoryTableRepository.cs;")]
    public class TablesControllerIntegrationTests : ControllerIntegrationTestsBase<TablesController>
    {
        [MonitoredTest]
        public void HappyFlow_UserAStartsATableWith2Seats_UserBJoins_AGameIsStartedAutomatically()
        {
            StartANewGameForANewTable();
        }
    }
}