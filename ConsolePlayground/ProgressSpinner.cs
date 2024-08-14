namespace ConsolePlayground;

public class ProgressSpinner
{
    private static readonly char[] PossibleStates = ['|', '/', '─', '\\'];
    
    public async Task Run(CancellationToken cancellationToken, IProgress<char> progress)
    {
        var (stateCount, currentSymbol) = (0, PossibleStates[0]);
        using var periodicTimer = new PeriodicTimer(TimeSpan.FromMilliseconds(100));
        while (!cancellationToken.IsCancellationRequested && await periodicTimer.WaitForNextTickAsync(CancellationToken.None))
        {
            progress.Report(currentSymbol);
            (stateCount, currentSymbol) = ((stateCount + 1) % PossibleStates.Length, PossibleStates[stateCount]);
        }
    }
}