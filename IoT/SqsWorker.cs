using Amazon.SQS;
using Amazon.SQS.Model;
using System.Text.Json;
namespace LaundryOaxWebAPI.IoT;
// The IoT rule constructs TenantId and DeviceId from the authorized MQTT topic,
// not from device-supplied JSON. Delete only after the durable inbox commit.
public sealed class SqsWorker(IConfiguration config, EventPipeline pipeline, ILogger<SqsWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        var url = config["Aws:QueueUrl"];
        if (string.IsNullOrWhiteSpace(url)) return;
        using var sqs = new AmazonSQSClient(); // SDK credential chain / ECS task role; no embedded keys.
        while (!ct.IsCancellationRequested)
        {
            try {
                var batch = await sqs.ReceiveMessageAsync(new ReceiveMessageRequest {
                    QueueUrl = url, MaxNumberOfMessages = 10, WaitTimeSeconds = 20, VisibilityTimeout = 60
                }, ct);
                foreach (var message in batch.Messages) {
                    try {
                        var e = JsonSerializer.Deserialize<Telemetry>(message.Body, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                            ?? throw new ArgumentException("Empty message");
                        await pipeline.Accept(e, ct);
                        await sqs.DeleteMessageAsync(url, message.ReceiptHandle, ct);
                    } catch (Exception ex) when (ex is not OperationCanceledException) {
                        logger.LogWarning(ex, "SQS message {MessageId} retained for retry / DLQ", message.MessageId);
                    }
                }
            } catch (OperationCanceledException) when (ct.IsCancellationRequested) { break; }
            catch (Exception ex) { logger.LogError(ex, "SQS unavailable"); await Task.Delay(TimeSpan.FromSeconds(5), ct); }
        }
    }
}
