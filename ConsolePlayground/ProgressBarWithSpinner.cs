using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace ConsolePlayground;

public class ProgressBarWithSpinner(int progressBarLength = 10, TimeSpan? updateTimeSpan = default)
{
    private readonly ILogger<ProgressBarWithSpinner> _logger = LoggerServiceLocator.CreateLogger<ProgressBarWithSpinner>() ?? NullLogger<ProgressBarWithSpinner>.Instance;
    private readonly TimeSpan _updateTimeSpan = updateTimeSpan ?? TimeSpan.FromMilliseconds(200);
    
    private const char Filled = '█';
    private const char Empty = ' ';

    private volatile int _currentStep;
    
    private readonly char[] _progressValues = Enumerable.Range(0, progressBarLength).Select(_ => Empty).ToArray();
    
    public async Task RunAsync(IProgress<string> progress, Func<int> getPercent, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogTrace("Starting progress bar with spinner");

            using var timer = new PeriodicTimer(_updateTimeSpan);
            var spinner = new ProgressSpinner(_updateTimeSpan);

            _logger.LogTrace("Starting spinner task");
            var spinnerTask = spinner.RunSpinner(new Progress<char>(currentSpinnerValue => _progressValues[_currentStep] = currentSpinnerValue), cancellationToken);

            while (await timer.WaitForNextTickAsync(cancellationToken))
            {
                _logger.LogTrace("Updating progress bar");
                var currentPercentage = getPercent();

                _logger.LogTrace("Current percentage: {Percentage}", currentPercentage);
                var currentIndex = currentPercentage * progressBarLength / 100;

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
            await spinnerTask; 
        }
        catch (OperationCanceledException canceledException)
        {
            _logger.LogInformation(canceledException, $"{nameof(ProgressBarWithSpinner)} operation cancelled");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"{nameof(ProgressBarWithSpinner)} unhandled exception");
            throw;
        }
    }
}