namespace OrderFlow.Domain.Abstractions;

public abstract class DomainException : Exception
{
    protected DomainException(string message) : base(message) {}
}