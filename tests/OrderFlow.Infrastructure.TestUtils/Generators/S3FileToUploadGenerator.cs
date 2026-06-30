using System.Text.Json;
using OrderFlow.Application.TestUtils.Builders.Failures;
using OrderFlow.Infrastructure.TestUtils.Fixtures;
using OrderFlow.TestCore.Drivers;

namespace OrderFlow.Infrastructure.TestUtils.Generators;

public static class S3FileToUploadGenerator
{
    public static string BuildKey(
        string messageId,
        string queueIdentifier) 
            => $"{StoreFixture.Prefix}/{queueIdentifier}/{messageId}.json";
    
    public static IEnumerable<S3Driver.FileToUpload> GenerateValidFilesToUpload(
        int numberOfFiles,
        string queueId)
    {
        return GenerateFilesToUpload(numberOfFiles, queueId, GetValidContentBody);

        string GetValidContentBody(int number) =>
            JsonSerializer.Serialize(new FailedMessageBuilder()
                .WithMessageId($"MessageId-{number}")
                .WithCorrelationId($"CorrelationId-{number}")
                .WithQueueIdentifier(queueId)
                .Build());
    }
    
    public static IEnumerable<S3Driver.FileToUpload> GenerateInvalidFilesToUpload(
        int numberOfFiles,
        string queueId)
    {
        return GenerateFilesToUpload(numberOfFiles, queueId, GetInvalidContentBody);

        string GetInvalidContentBody(int number) => $"invalid content body-{number}";
    }
    
    public static IEnumerable<S3Driver.FileToUpload> GenerateEmptyFilesToUpload(
        int numberOfFiles,
        string queueId)
    {
        return GenerateFilesToUpload(numberOfFiles, queueId, GetInvalidContentBody);

        string GetInvalidContentBody(int number) => "null";
    }

    private static IEnumerable<S3Driver.FileToUpload> GenerateFilesToUpload(
        int numberOfFiles,
        string queueId,
        Func<int, string> getContentBody) =>
        Enumerable
            .Range(1, numberOfFiles)
            .Select(number => new S3Driver.FileToUpload(
                BuildKey($"message-{Guid.NewGuid()}", queueId),
                getContentBody(number)));
}