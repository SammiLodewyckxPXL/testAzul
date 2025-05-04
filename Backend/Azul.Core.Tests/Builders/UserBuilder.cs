using Azul.Core.UserAggregate;

namespace Azul.Core.Tests.Builders;

public class UserBuilder
{
    private readonly User _user = new()
    {
        Id = Guid.NewGuid(),
        Email = Guid.NewGuid().ToString(),
        LastVisitToPortugal = null,
        UserName = Guid.NewGuid().ToString(),
        PasswordHash = Guid.NewGuid().ToString()
    };

    public UserBuilder AsCloneOf(User user)
    {
        _user.Id = user.Id;
        _user.Email = user.Email;
        _user.LastVisitToPortugal = user.LastVisitToPortugal;
        _user.UserName = user.UserName;
        _user.PasswordHash = user.PasswordHash;
        return this;
    }

    public UserBuilder WithUserName(string userName)
    {
        _user.UserName = userName;
        return this;
    }

    public UserBuilder WithLastVisitToPortugal(DateOnly? lastVisitToPortugal)
    {
        _user.LastVisitToPortugal = lastVisitToPortugal;
        return this;
    }

    public User Build()
    {
        return _user;
    }
}