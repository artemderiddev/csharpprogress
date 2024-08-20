using System.Collections.Immutable;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace ConsolePlayground;

public class ProgressBarWithSpinner(ILogger<ProgressBarWithSpinner>? logger = default, int maxSteps = 10, TimeSpan? updateTimeSpan = default)
{
    private readonly ILogger<ProgressBarWithSpinner> _logger = logger ?? NullLogger<ProgressBarWithSpinner>.Instance;
    private readonly TimeSpan _updateTimeSpan = updateTimeSpan ?? TimeSpan.FromMilliseconds(200);
    
    private static readonly ImmutableArray<char> PossibleSpinnerStates = ['|', '/', '─', '\\'];

    private const char Filled = '█';
    private const char Empty = ' ';

    private volatile int _currentStep;
    
    private readonly char[] _progressValues = Enumerable.Range(0, maxSteps).Select(_ => Empty).ToArray();
    
    public async Task RunAsync(IProgress<string> progress, Func<int> getPercent, CancellationToken cancellationToken = default)
    {
        _logger.LogTrace("Starting progress bar with spinner");
        using var progressBarCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        // cancellationToken.Register(() => cts.Cancel());
        
        using var timer = new PeriodicTimer(_updateTimeSpan);
        
        try
        {
            // TODO: debug and see where you missed to catch cancellation exception part 1
            var spinTask = RunSpinner(new Progress<char>(currentSpinnerValue => _progressValues[_currentStep] = currentSpinnerValue), progressBarCts.Token);
            
            // TODO: debug and see where you missed to catch cancellation exception part 2
            while (!progressBarCts.IsCancellationRequested && await timer.WaitForNextTickAsync(default))
            {
                _logger.LogTrace("Updating progress bar");
                var currentPercentage = getPercent();
                _logger.LogTrace("Current percentage: {Percentage}", currentPercentage);
                // if(currentPercentage == 0) continue;

                var currentIndex = currentPercentage * maxSteps / 100;
                _logger.LogTrace("Current index: {CurrentIndex}", currentIndex);

                var progressString = string.Join("", _progressValues);

                _logger.LogTrace("Progress: {ProgressString}", progressString);
                progress.Report(progressString);

                if (currentIndex == _currentStep) continue;

                _logger.LogTrace("Updating current index: {CurrentIndex}", _currentStep);
                _progressValues[_currentStep] = Filled;

                _logger.LogTrace("Filled index: {FilledIndex}", _currentStep);
                _currentStep = currentIndex;
            }

            _logger.LogTrace("Finished progress bar");
            // await cts.CancelAsync();
            await spinTask;
        }
        catch (OperationCanceledException canceledException)
        {
            _logger.LogError(canceledException, "Operation cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception inside Progress Bar With Spinner");
            throw;
        }
    }

    // TODO: move updated logic to Spinner class, you can just utilize it inside task that you will control
    private async Task RunSpinner(IProgress<char> spinnerState, CancellationToken cancellationToken = default)
    {
        using var spinnerCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        using var spinnerTimer = new PeriodicTimer(_updateTimeSpan);
        try
        {
            var (currentSpinnerIndex, currentSpinnerSymbol) = (0, PossibleSpinnerStates[0]);
            spinnerState.Report(currentSpinnerSymbol);

            while (!spinnerCts.IsCancellationRequested && await spinnerTimer.WaitForNextTickAsync(default))
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
            _logger.LogError(canceledException, "Operation cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception inside ProgressSpinner");
        }
    }
}