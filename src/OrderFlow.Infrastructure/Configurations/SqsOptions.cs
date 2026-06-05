namespace OrderFlow.Infrastructure.Configurations;

public class SqsOptions
{
    public int MaxNumberOfMessages { get; set; } = default!;
    public int WaitTimeSeconds { get; set; } = default!;
    public int VisibilityTimeout { get; set; } = default!;
}