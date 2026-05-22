using System.Text.Json;
using Amazon.S3;
using Amazon.S3.Model;

namespace OrderFlow.Worker.Shared.FailureStore;

public class S3FailureStore : IFailureStore
{
    private readonly IAmazonS3 _s3;
    private readonly string _bucketName;
    private readonly string _prefix;

    public S3FailureStore(
        IAmazonS3 s3,
        IConfiguration configuration)
    {
        _s3 = s3;

        var s3Config = configuration.GetSection("S3");
        _bucketName = s3Config.GetValue<string>("FailureStore:BucketName")
                      ?? throw new ArgumentNullException(paramName: nameof(configuration), message: "S3:FailureStore:BucketName not configured");
        _prefix = s3Config.GetValue<string>("FailureStore:Prefix")
                      ?? throw new ArgumentNullException(paramName: nameof(configuration), message: "S3:FailureStore:Prefix not configured");
    }
        
    public async Task SaveFailedMessageAsync(FailedMessage message)
    {
        var key = $"{_prefix}/{message.QueueName}/{message.MessageId}.json";
        
        await _s3.PutObjectAsync(new PutObjectRequest
        {
            BucketName = _bucketName,
            Key = key,
            ContentBody = JsonSerializer.Serialize(message, new JsonSerializerOptions{ WriteIndented = true })
        });
    }

    public async Task<List<FailedMessage>> GetAllAsync(string queueName)
    {
        throw new NotImplementedException();
    }

    public async Task RemoveAsync(string queueName, string messageId)
    {
        throw new NotImplementedException();
    }
}