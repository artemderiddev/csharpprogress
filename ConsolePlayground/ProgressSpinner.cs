namespace ConsolePlayground;

public class ProgressSpinner
{
    private static char[] _possibleStates = ['|', '/', '─', '\\'];
    
    public async Task Run(CancellationToken cancellationToken, IProgress<char> progress)
    {
        var (stateCount, currentSymbol) = (0, _possibleStates[0]);
        using var periodicTimer = new PeriodicTimer(TimeSpan.FromMilliseconds(100));
        while (!cancellationToken.IsCancellationRequested && await periodicTimer.WaitForNextTickAsync(CancellationToken.None))
        {
            progress.Report(currentSymbol);
            (stateCount, currentSymbol) = ((stateCount + 1) % _possibleStates.Length, _possibleStates[stateCount]);
        }
    }
}