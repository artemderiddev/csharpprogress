using ConsolePlayground;

var cts = new CancellationTokenSource();
var backgroundCts = new CancellationTokenSource();

var path = Path.GetTempFileName();
var appStatus = string.Empty;
var sizeInMb = 200;

Console.WriteLine("Starting application...");
Console.WriteLine($"Temp file is {path} ");

Task keyboardListenerTask = KeyboardListener(backgroundCts.Token, new Progress<ConsoleKey>(CancelOnCKeyPressed));;
Task spinner = new ProgressSpinner().Run(backgroundCts.Token, new Progress<char>(c => Console.Write($"\r{c} "))); 

try
{
    FileService fileService = new();
    await fileService.GenerateTextFile(cts.Token, path, sizeInMb);
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
    Console.WriteLine("File deleted");
    Console.WriteLine(appStatus);
}

void CancelOnCKeyPressed(ConsoleKey keyPressed)
{
    if(keyPressed == ConsoleKey.C) cts.Cancel();
}

async Task KeyboardListener(CancellationToken cancellationToken, IProgress<ConsoleKey> progress)
{
    using var periodicTimer = new PeriodicTimer(TimeSpan.FromMilliseconds(50));
    while (!cancellationToken.IsCancellationRequested &&
           await periodicTimer.WaitForNextTickAsync())
    {
        if (Console.KeyAvailable) progress.Report(Console.ReadKey(intercept: true).Key);
    }
}