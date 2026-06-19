using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrderFlow.Application.Failures;
using OrderFlow.Infrastructure.Configurations;

namespace OrderFlow.Infrastructure.Failures;

public class S3FailureStore(
    IAmazonS3 s3,
    IOptions<S3FailureStoreOptions> options,
    ILogger<S3FailureStore> logger) : IFailureStore
{
    private readonly S3FailureStoreOptions _options = options.Value;
    private readonly JsonSerializerOptions _jsonSerializerOptions = new() { WriteIndented = true };

    public async Task SaveFailedMessageAsync(FailedMessage message, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message, nameof(message));
        
        var key = $"{_options.Prefix}/{message.QueueIdentifier}/{message.MessageId}.json";
        
        await s3.PutObjectAsync(new PutObjectRequest
        {
            BucketName = _options.BucketName,
            Key = key,
            ContentBody = JsonSerializer.Serialize(message, _jsonSerializerOptions)
        }, cancellationToken);
    }
    
    public async Task<FailedMessagePage> GetPageAsync(
        int pageSize,
        string? continuationToken,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(pageSize, nameof(pageSize));
        ArgumentOutOfRangeException.ThrowIfGreaterThan(pageSize, _options.MaxPageSize, nameof(pageSize));
        
        var request = new ListObjectsV2Request
        {
            BucketName = _options.BucketName,
            Prefix = _options.Prefix,
            MaxKeys = pageSize,
            ContinuationToken = continuationToken
        };

        var listObjectsResponse = await s3.ListObjectsV2Async(request, cancellationToken);
        var failedMessagePageItems = await GetParallelFailedMessagePageItemsAsync(
            listObjectsResponse.S3Objects,
            cancellationToken);
        
        return new FailedMessagePage(
            failedMessagePageItems,
            listObjectsResponse.IsTruncated,
            listObjectsResponse.NextContinuationToken);
    }

    [ExcludeFromCodeCoverage] // TODO: Remove after implementation
    public Task RemoveAsync(string queueName, string messageId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    private async Task<IReadOnlyList<FailedMessagePageItem>> GetParallelFailedMessagePageItemsAsync(
        IEnumerable<S3Object> s3Objects,
        CancellationToken cancellationToken)
    {
        var messages = new ConcurrentBag<FailedMessagePageItem>();

        await Parallel.ForEachAsync(
            s3Objects,
            new ParallelOptions
            {
                MaxDegreeOfParallelism = _options.MaxDegreeOfParallelism,
                CancellationToken = cancellationToken
            },
            async (s3Object, ct) =>
            {
                var message = await GetFailedMessagePageItemAsync(s3Object, ct);

                if (message is not null)
                {
                    messages.Add(message);
                }
            });

        return messages
            .OrderByDescending(x => x.FailedAt)
            .ToList();
    }

    private async Task<FailedMessagePageItem?> GetFailedMessagePageItemAsync(
        S3Object s3Object,
        CancellationToken cancellationToken)
    {
        try
        {
            var content = await GetFileContentAsTextAsync(
                s3Object.Key,
                cancellationToken);

            var message = JsonSerializer.Deserialize<FailedMessage>(
                content,
                _jsonSerializerOptions);

            return message?.ToPageItem();
        }
        catch (JsonException ex)
        {
            logger.LogWarning(
                ex,
                "Failed to deserialize DLQ message {Key}",
                s3Object.Key);

            return null;
        }
    }
    
    private async Task<string> GetFileContentAsTextAsync( 
        string key,
        CancellationToken cancellationToken)
    {
        var request = new GetObjectRequest
        {
            BucketName = _options.BucketName,
            Key = key
        };

        using var response = await s3.GetObjectAsync(request, cancellationToken);
        using var reader = new StreamReader(response.ResponseStream);
    
        return await reader.ReadToEndAsync(cancellationToken);
    }
}