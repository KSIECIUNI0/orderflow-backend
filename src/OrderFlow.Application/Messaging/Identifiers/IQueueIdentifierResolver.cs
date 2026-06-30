namespace OrderFlow.Application.Messaging.Identifiers;

public interface IQueueIdentifierResolver
{
    string ResolveQueueUrl(string queueIdentifier);
}