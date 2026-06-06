using AutoFixture;
using AutoFixture.AutoNSubstitute;

namespace OrderFlow.Testing.Builders;

public abstract class BaseServiceBuilder<TSut> where TSut : class
{
    private readonly IFixture _fixture = new Fixture()
        .Customize(new AutoNSubstituteCustomization());

    protected TDependency Freeze<TDependency>() where TDependency : class
    {
        return _fixture.Freeze<TDependency>();
    }

    public virtual TSut Build()
    {
        return _fixture.Create<TSut>();
    }
}