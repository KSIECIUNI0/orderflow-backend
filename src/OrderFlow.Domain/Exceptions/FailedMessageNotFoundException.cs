using OrderFlow.Domain.Abstractions;

namespace OrderFlow.Domain.Exceptions;

public class FailedMessageNotFoundException(string messageId) 
    : DomainException($"Failed message {messageId} was not found")
{
    public override string ErrorTitle
        => "Failed message not found";
}