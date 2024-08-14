using ConsolePlayground;

var cts = new CancellationTokenSource();
var backgroundCts = new CancellationTokenSource();

var path = Path.GetTempFileName();
var appStatus = string.Empty;
var sizeInMb = 100;

Console.WriteLine("Starting application...");
Console.WriteLine($"Temp file is {path} ");

Task keyboardListenerTask = KeyboardListener(backgroundCts.Token, new Progress<ConsoleKey>(CancelOnCKeyPressed));;
Task spinner = new ProgressSpinner().Run(backgroundCts.Token, new Progress<char>(EmptySpinner));

try
{
    await new FileService().GenerateTextFile(cts.Token, new Progress<int>(DisplayPercentage), path, sizeInMb); // known bug here, text generating task spamming main thread with events with buffer size of 128, same with bigger 1024 but not that critical
    appStatus = "Success";
}
catch (OperationCanceledException)
{
    appStatus = "Canceled";
}
finally
{
    backgroundCts.Cancel();
    await keyboardListenerTask;
    await spinner;
    
    File.Delete(path);
    Console.WriteLine("\nFile deleted");
    Console.WriteLine(appStatus);
}

void DisplayPercentage(int percent) => Console.Write($"\r{percent}%");

void EmptySpinner(char c) { }

void CancelOnCKeyPressed(ConsoleKey keyPressed)
{
    if(keyPressed == ConsoleKey.C) cts.Cancel();
}

async Task KeyboardListener(CancellationToken cancellationToken, IProgress<ConsoleKey> progress)
{
    using var periodicTimer = new PeriodicTimer(TimeSpan.FromMilliseconds(50));
    while (!cancellationToken.IsCancellationRequested &&
           await periodicTimer.WaitForNextTickAsync(CancellationToken.None))
    {
        if (Console.KeyAvailable) progress.Report(Console.ReadKey(intercept: true).Key);
    }
}