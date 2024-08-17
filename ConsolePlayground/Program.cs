using ConsolePlayground;
using Microsoft.Extensions.Logging;

var cts = new CancellationTokenSource();
var backgroundCts = new CancellationTokenSource();

var path = Path.GetTempFileName();
var appStatus = string.Empty;
const int sizeInMb = 2000;

Console.WriteLine("Starting application...");
Console.WriteLine($"Temp file is {path} ");

Task keyboardListenerTask = KeyboardListener(backgroundCts.Token, new Progress<ConsoleKey>(CancelOnCKeyPressed));;

var factory = LoggerFactory.Create(x => x.AddConsole().SetMinimumLevel(LogLevel.Warning));
var progressBarLogger = factory.CreateLogger<ProgressBarWithSpinner>();
var fileServiceLogger = factory.CreateLogger<FileService>();

int percentage = 0;
Task progressSpinner = new ProgressBarWithSpinner(progressBarLogger).RunAsync(new Progress<string>(DisplayProgressSpinner), () => percentage, backgroundCts.Token);

try
{
    await new FileService(fileServiceLogger).GenerateTextFile(cts.Token, new Progress<int>(value => percentage = value), path, sizeInMb); // known bug here, text generating task spamming main thread with events with buffer size of 128, same with bigger 1024 but not that critical
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
    await progressSpinner;
    
    File.Delete(path);
    Console.WriteLine();
    Console.WriteLine("File deleted");
    Console.WriteLine(appStatus);
}

return;

// TODO: move this to static method to progress bar class itself, at least give opportunity to register handler
void DisplayProgressSpinner(string state)
{
    var returns = string.Join(string.Empty, Enumerable.Range(0, state.Length).Select(x => "\r")); 
    Console.Write($"{returns}{state}");
}

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