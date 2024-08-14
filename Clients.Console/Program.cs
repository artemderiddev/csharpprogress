using Domain;

var cts = new CancellationTokenSource();

var path = Path.GetTempFileName();
var appStatus = string.Empty;
var sizeInMb = 200;

Console.WriteLine("Starting application...");
Console.WriteLine($"Temp file is {path} ");

Task keyPressTask = CancelOnButtonPress(cts, ConsoleKey.C);

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
    File.Delete(path);
    Console.WriteLine("File deleted");
    Console.WriteLine(appStatus);
}

async Task CancelOnButtonPress(CancellationTokenSource cancellationTokenSource, ConsoleKey key)
{
    var periodicTimer = new PeriodicTimer(TimeSpan.FromMilliseconds(50));
    while (await periodicTimer.WaitForNextTickAsync(cancellationTokenSource.Token))
    {
        if(Console.KeyAvailable && Console.ReadKey(true).Key == key) cancellationTokenSource.Cancel();
    }
}