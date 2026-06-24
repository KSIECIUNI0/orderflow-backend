using OrderFlow.Domain.Abstractions;

namespace OrderFlow.Api.TestUtils.Fakes;

public class FakeDomainException(string message) : DomainException(message)
{
}