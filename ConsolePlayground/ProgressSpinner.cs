using Microsoft.Extensions.Logging;
using System.Collections.Immutable;

namespace ConsolePlayground;

public class ProgressSpinner(TimeSpan? updateTimeSpan = default)
{
    private static readonly ImmutableArray<char> PossibleSpinnerStates = ['|', '/', '─', '\\'];

    private readonly ILogger<ProgressSpinner> _logger = LoggerServiceLocator.CreateLogger<ProgressSpinner>();
    private readonly TimeSpan _updateTimeSpan = updateTimeSpan ?? TimeSpan.FromMilliseconds(200);
    public async Task RunSpinner(IProgress<char> spinnerState, CancellationToken cancellationToken = default)
    {
        using var spinnerCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        using var spinnerTimer = new PeriodicTimer(_updateTimeSpan);
        try
        {
            var (currentSpinnerIndex, currentSpinnerSymbol) = (0, PossibleSpinnerStates[0]);
            spinnerState.Report(currentSpinnerSymbol);

            while (await spinnerTimer.WaitForNextTickAsync(spinnerCts.Token))
            {
                currentSpinnerIndex = currentSpinnerIndex switch
                {
                    _ when currentSpinnerIndex + 1 == PossibleSpinnerStates.Length => 0,
                    _ => currentSpinnerIndex + 1
                };
                currentSpinnerSymbol = PossibleSpinnerStates[currentSpinnerIndex];
                spinnerState.Report(currentSpinnerSymbol);
            }

        }
        catch (OperationCanceledException canceledException)
        {
            _logger.LogInformation(canceledException, $"{nameof(ProgressSpinner)} operation cancelled");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"{nameof(ProgressSpinner)} unhandled exception");
            throw;
        }
    }
}