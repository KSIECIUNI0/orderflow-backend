namespace OrderFlow.Domain.Abstractions;

public abstract class DomainException(string message) : Exception(message)
{
    public virtual string ErrorTitle
        => "Business rule violation";
}