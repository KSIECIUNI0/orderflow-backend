using Amazon.S3;
using Amazon.S3.Model;

namespace OrderFlow.TestCore.Drivers;

public class S3Driver(IAmazonS3 s3)
{
    public IAmazonS3 S3Client => s3;
    
    public async Task CreateBucket(string bucketName, CancellationToken cancellationToken = default)
    {
        await s3.PutBucketAsync(bucketName, cancellationToken);
    }

    public async Task DeleteBucket(
        string bucketName,
        string prefix,
        CancellationToken cancellationToken = default)
    {
        var keys = await s3.GetAllObjectKeysAsync(bucketName, prefix, null);
        await s3.DeletesAsync(bucketName, keys, null, cancellationToken);
        await s3.DeleteBucketAsync(bucketName, cancellationToken);
    }
    
    public async Task<bool> FileExistsAsync(
        string bucketName,
        string key,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await s3.GetObjectMetadataAsync(bucketName, key, cancellationToken);
            return true;
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return false;
        }
    }
    
    public async Task<string> GetFileContentAsTextAsync(
        string bucketName, 
        string key,
        CancellationToken cancellationToken = default)
    {
        var request = new GetObjectRequest
        {
            BucketName = bucketName,
            Key = key
        };

        using var response = await s3.GetObjectAsync(request, cancellationToken);
        using var reader = new StreamReader(response.ResponseStream);
    
        return await reader.ReadToEndAsync(cancellationToken);
    }
}