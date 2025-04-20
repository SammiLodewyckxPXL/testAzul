using Moq;

namespace Azul.Core.Tests.Builders;

public abstract class MockBuilder<T> where T : class
{
    public Mock<T> Mock { get; private set; } = new();

    public T Object => Mock.Object;
}