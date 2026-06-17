namespace OrderFlow.Domain.Abstractions;

public abstract class DomainException(string message) : Exception(message);