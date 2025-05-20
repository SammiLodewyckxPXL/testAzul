using System.Security.Claims;
using AutoMapper;
using Azul.Api.Controllers;
using Azul.Api.Models.Output;
using Azul.Core.TableAggregate;
using Azul.Core.TableAggregate.Contracts;
using Azul.Core.Tests.Builders;
using Azul.Core.UserAggregate;
using Guts.Client.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace Azul.Api.Tests;

[ProjectComponentTestFixture("1TINProject", "Azul", "TablesCtlr",
    @"Azul.Api\Controllers\TablesController.cs")]
public class TablesControllerTests
{
    private TablesController _controller = null!;
    private Mock<ITableManager> _tableManagerMock = null!;
    private Mock<ITableRepository> _tableRepositoryMock = null!;
    private Mock<UserManager<User>> _userManagerMock = null!;
    private Mock<IMapper> _mapperMock = null!;
    private User _loggedInUser = null!;
    private TablePreferences _tablePreferences = null!;

    [SetUp]
    public void Setup()
    {
        _tableManagerMock = new Mock<ITableManager>();
        _tableRepositoryMock = new Mock<ITableRepository>();
        _mapperMock = new Mock<IMapper>();

        var userStoreMock = new Mock<IUserStore<User>>();
        var passwordHasherMock = new Mock<IPasswordHasher<User>>();
        var lookupNormalizerMock = new Mock<ILookupNormalizer>();
        var errorsMock = new Mock<IdentityErrorDescriber>();
        var loggerMock = new Mock<ILogger<UserManager<User>>>();
        _userManagerMock = new Mock<UserManager<User>>(
            userStoreMock.Object,
            null,
            passwordHasherMock.Object,
            null,
            null,
            lookupNormalizerMock.Object,
            errorsMock.Object,
            null,
            loggerMock.Object);

        _controller = new TablesController(_tableManagerMock.Object, _tableRepositoryMock.Object, _mapperMock.Object, _userManagerMock.Object);

        _loggedInUser = new UserBuilder().Build();
        var userClaimsPrincipal = new ClaimsPrincipal(
            new ClaimsIdentity(new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, _loggedInUser.Id.ToString())
            })
        );
        var context = new ControllerContext { HttpContext = new DefaultHttpContext() };
        context.HttpContext.User = userClaimsPrincipal;
        _controller.ControllerContext = context;
        _userManagerMock.Setup(manager => manager.GetUserAsync(userClaimsPrincipal))
            .ReturnsAsync(_loggedInUser);

        _tablePreferences = new TablePreferencesBuilder().Build();
    }

    [MonitoredTest]
    public void GetTableById_ShouldRetrieveTableUsingRepositoryAndReturnAModelOfIt()
    {
        //Arrange
        ITable table = new TableMockBuilder().Mock.Object;
        _tableRepositoryMock.Setup(repository => repository.Get(table.Id)).Returns(table);

        var tableModel = new TableModel();
        _mapperMock.Setup(mapper => mapper.Map<TableModel>(It.IsAny<ITable>())).Returns(tableModel);

        //Act
        var result = _controller.GetTableById(table.Id) as OkObjectResult;

        //Assert
        Assert.That(result, Is.Not.Null, "An instance of 'OkObjectResult' should be returned.");
        _mapperMock.Verify(mapper => mapper.Map<TableModel>(table), Times.Once,
            "The table is not correctly mapped to a table model");
        Assert.That(result!.Value, Is.SameAs(tableModel), "The mapped table model is not in the OkObjectResult");
    }

    [MonitoredTest]
    public void JoinOrCreate_NoMatchingTableExists_ShouldUseTheTableManagerToCreateANewTableSeatedByTheLoggedInUser()
    {
        //Arrange
        ITable createdTable = new TableMockBuilder().WithSeatedUsers([_loggedInUser]).Object;
        _tableManagerMock.Setup(manager => manager.JoinOrCreateTable(It.IsAny<User>(), It.IsAny<TablePreferences>()))
            .Returns(createdTable);

        var tableModel = new TableModel();
        _mapperMock.Setup(mapper => mapper.Map<TableModel>(It.IsAny<ITable>())).Returns(tableModel);

        //Act
        var result = _controller.JoinOrCreate(_tablePreferences).Result as OkObjectResult;

        //Assert
        Assert.That(result, Is.Not.Null, "An instance of 'OkObjectResult' should be returned.");

        _userManagerMock.Verify(manager => manager.GetUserAsync(It.IsAny<ClaimsPrincipal>()), Times.Once,
            "The 'GetUserAsync' of the UserManager is not called");

        _tableManagerMock.Verify(manager => manager.JoinOrCreateTable(_loggedInUser, _tablePreferences), Times.Once,
            "The 'JoinOrCreateTable' method of the table manager is not called correctly");

        _tableManagerMock.Verify(manager => manager.StartGameForTable(It.IsAny<Guid>()), Times.Never,
            "The 'StartGameForTable' method of the table manager should not be called. The table is not full yet.");

        _mapperMock.Verify(mapper => mapper.Map<TableModel>(createdTable), Times.Once,
            "The table is not correctly mapped to a table model");

        Assert.That(result!.Value, Is.SameAs(tableModel), "The mapped table model is not in the OkObjectResult");
    }

    [MonitoredTest]
    public void JoinOrCreate_MatchingTableWithOneAvailableSeatExists_ShouldUseTheTableManagerToJoinTheMatchingTable()
    {
        //Arrange
        User otherUser = new UserBuilder().Build();
        ITable createdTable = new TableMockBuilder().WithSeatedUsers([otherUser, _loggedInUser]).Object;
        _tableManagerMock.Setup(manager => manager.JoinOrCreateTable(It.IsAny<User>(), It.IsAny<TablePreferences>()))
            .Returns(createdTable);

        var tableModel = new TableModel();
        _mapperMock.Setup(mapper => mapper.Map<TableModel>(It.IsAny<ITable>())).Returns(tableModel);

        //Act
        var result = _controller.JoinOrCreate(_tablePreferences).Result as OkObjectResult;

        //Assert
        Assert.That(result, Is.Not.Null, "An instance of 'OkObjectResult' should be returned.");

        _userManagerMock.Verify(manager => manager.GetUserAsync(It.IsAny<ClaimsPrincipal>()), Times.Once,
            "The 'GetUserAsync' of the UserManager is not called");

        _tableManagerMock.Verify(manager => manager.JoinOrCreateTable(_loggedInUser, _tablePreferences), Times.Once,
            "The 'JoinOrCreateTable' method of the table manager is not called correctly");

        _tableManagerMock.Verify(manager => manager.StartGameForTable(createdTable.Id), Times.Once,
            "The 'StartGameForTable' method of the table manager is not called correctly. The table is not full yet.");

        _mapperMock.Verify(mapper => mapper.Map<TableModel>(createdTable), Times.Once,
            "The table is not correctly mapped to a table model");

        Assert.That(result!.Value, Is.SameAs(tableModel), "The mapped table model is not in the OkObjectResult");
    }

    [MonitoredTest]
    public void Leave_ShouldUseTheTableManagerToRemoveTheLoggedInUserFromTheTable()
    {
        //Arrange
        ITable existingTable = new TableMockBuilder().Object;

        //Act
        var result = _controller.Leave(existingTable.Id).Result as OkResult;

        //Assert
        Assert.That(result, Is.Not.Null, "An instance of 'OkResult' should be returned.");

        _userManagerMock.Verify(manager => manager.GetUserAsync(It.IsAny<ClaimsPrincipal>()), Times.Once,
            "The 'GetUserAsync' of the UserManager is not called");

        _tableManagerMock.Verify(manager => manager.LeaveTable(existingTable.Id, _loggedInUser), Times.Once,
            "The 'LeaveTable' method of the table manager is not called correctly");
    }
}

