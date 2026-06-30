using OrderFlow.TestCore.Drivers;
using OrderFlow.TestCore.Fixtures;
using WireMock.Logging;

namespace OrderFlow.Infrastructure.TestUtils.Fixtures;

public class WireMockDriverFixture(WireMockFixture fixture)
{
    public WireMockDriver Driver => new(fixture.Server);
    public IReadOnlyList<ILogEntry> LogEntries => fixture.Server.LogEntries;

    public ValueTask ResetLogEntries() => Driver.ResetLogEntries();
}