using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging;

namespace ConsolePlayground;

public class KeyboardListener(CancellationToken cancellationToken, IProgress<ConsoleKey> keyboardKeyPressedReporter)
{
    private readonly ILogger<KeyboardListener> _logger = LoggerServiceLocator.CreateLogger<KeyboardListener>() ?? NullLogger<KeyboardListener>.Instance;

    public async Task Run()
    {
        _logger.LogTrace("Running keyboard listener");
        try
        {
            using var periodicTimer = new PeriodicTimer(TimeSpan.FromMilliseconds(200));
            while (!cancellationToken.IsCancellationRequested && await periodicTimer.WaitForNextTickAsync(cancellationToken))
            {
                if (Console.KeyAvailable) keyboardKeyPressedReporter.Report(Console.ReadKey(intercept: true).Key);
            }

        }
        catch (OperationCanceledException canceledException)
        {
            _logger.LogInformation(canceledException, $"{nameof(KeyboardListener)} operation cancelled");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"{nameof(KeyboardListener)} unhandled exception");
        }
    }
}