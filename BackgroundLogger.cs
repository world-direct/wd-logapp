using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

public class BackgroundLogger : IHostedService, IDisposable
{
    private readonly ILogger<BackgroundLogger> logger;
    private Timer timer;
    private int number;

    public BackgroundLogger(ILogger<BackgroundLogger> logger)
    {
        this.logger = logger;
    }

    void WriteTestLogEntries(ILogger logger){
        Interlocked.Increment(ref number);

        using(logger.BeginScope("root-scope")){
            logger.LogInformation($"Logging Number {number} with a template string");
            logger.LogInformation("Logging Number {number} with an arg", number);
            using(logger.BeginScope(new {Task="DemoTask", ID=42})){
                logger.LogInformation("Logging String from nested scope", number);
            }
        }
    }

    public void Dispose()
    {
        timer?.Dispose();
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        timer = new Timer(o => WriteTestLogEntries(logger), null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(5));
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}