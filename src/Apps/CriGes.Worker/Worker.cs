namespace CriGes.Worker;

public partial class Worker(ILogger<Worker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                WorkerRunning(logger, DateTimeOffset.Now);
            }
            await Task.Delay(1000, stoppingToken);
        }
    }

    [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "Worker running at: {CurrentTime}")]
    private static partial void WorkerRunning(ILogger logger, DateTimeOffset currentTime);
}
