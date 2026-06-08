using System.Text.Json;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Options;
using OrderFlow.Application.Failures;
using OrderFlow.Infrastructure.Configurations;

namespace OrderFlow.Infrastructure.Failures;

public class S3FailureStore(
    IAmazonS3 s3,
    IOptions<S3Options> options) : IFailureStore
{
    private readonly S3Options _options = options.Value;
    private readonly JsonSerializerOptions _jsonSerializerOptions = new() { WriteIndented = true };

    public async Task SaveFailedMessageAsync(FailedMessage message, CancellationToken cancellationToken = default)
    {
        var key = $"{_options.Prefix}/{message.QueueName}/{message.MessageId}.json";
        
        await s3.PutObjectAsync(new PutObjectRequest
        {
            BucketName = _options.BucketName,
            Key = key,
            ContentBody = JsonSerializer.Serialize(message, _jsonSerializerOptions)
        }, cancellationToken);
    }

    public Task<List<FailedMessage>> GetAllAsync(string queueName, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task RemoveAsync(string queueName, string messageId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}