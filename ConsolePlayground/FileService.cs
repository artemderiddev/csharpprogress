using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace ConsolePlayground;

public class FileService(ILogger<FileService>? logger = default)
{
    private readonly ILogger<FileService> _logger = logger ?? new NullLogger<FileService>();
    
    private const string Alphabet = "abcdefghigklmnopqrstuvwxyz";
    
    // Mark variable volatile to ensure no compiler optimization and warnings about using it in separate task.
    // Even if dotnet will try to sync memory between task and try to get us latest version of the value this still does not guarantee we will have the latest value 
    // If we want to make sure we need to use some locking mechanism, but since this is read only variable for another task and only for reporting with a lot of changes
    // It will be just more optimal to ignore some differences for better performance.
    private volatile int _progressStatus; 

    public async Task GenerateTextFile(CancellationToken token, IProgress<int> progress, string path, int lengthMb)
    {
        await GenerateTextFile(token, progress, path, lengthMb, 1024);
    }

    public async Task GenerateTextFile(CancellationToken token, IProgress<int> progress, string path, int lengthMb, int bufferSize)
    {
        _progressStatus = 0;
        
        using var progressTaskCts = new CancellationTokenSource();
        var progressTask = ReportProgressPeriodicallyAsync(progressTaskCts.Token, progress, () => _progressStatus);

        try
        {
            await using var writer = new StreamWriter(path);
            var buffer = new char[bufferSize];
            var totalOperations = lengthMb * 1024 * 1024 / buffer.Length;

            for (var i = 0; i < totalOperations; i++)
            {
                FillRandomTextToBuffer(
                    buffer); // AsSpan() is not necessary since span has implicit conversion operator
                await writer.WriteAsync(buffer, token);
                _progressStatus = SmoothProgressPercentCalculation(i, totalOperations);
            }

            progress.Report(100);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("File generation was cancelled.");
            File.Delete(path);
            throw;
        }
        finally
        {
            try
            {
                await progressTaskCts.CancelAsync();
                await progressTask;
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Progress task was cancelled.");
            }
        }
    }

    private int SmoothProgressPercentCalculation(int tick, int totalSteps) => (tick + 1) * 100 / totalSteps;

    private async Task ReportProgressPeriodicallyAsync(CancellationToken token, IProgress<int> progress,
        Func<int> getCurrentProgress)
    {
        var progressStatus = 0;
        using var periodicTimer = new PeriodicTimer(TimeSpan.FromMilliseconds(500));
        while (!token.IsCancellationRequested && await periodicTimer.WaitForNextTickAsync(token))
        {
            var newProgress = getCurrentProgress();
            if (newProgress == progressStatus) continue;
                
            progressStatus = newProgress;
            progress.Report(progressStatus);
            
        }
    }

    private void FillRandomTextToBuffer(Span<char> buffer) => Random.Shared.GetItems(Alphabet.AsSpan(), buffer);
}