namespace OrderFlow.Infrastructure.Configurations;

public class OrderCreatedDlqOptions
{
    public string Url { get; set; } = default!;
    public string OrderCreatedQueueName { get; set; } = default!;
}