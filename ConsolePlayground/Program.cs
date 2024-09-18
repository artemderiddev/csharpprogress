using ConsolePlayground;

CancellationTokenSource heavyTaskCts = new ();
CancellationTokenSource backgroundCts = new ();

var path = Path.GetTempFileName();
var appStatus = string.Empty;
const int sizeInMb = 1000;

Console.WriteLine("Starting application...");
Console.WriteLine($"Temp file path: {path}");

var keyboardListener = new KeyboardListener(backgroundCts.Token, new Progress<ConsoleKey>(CancelOnCKeyPressed));
var progressBarWithSpinner = new ProgressBarWithSpinner();
var fileService = new FileService();

Task progressTask = default!;
Task keyboardListenerTask = keyboardListener.Run();
try
{
    int currentPercentage = 0;

    progressTask = progressBarWithSpinner.RunAsync(new Progress<string>(DisplayProgressSpinner), () => currentPercentage, backgroundCts.Token);
    
    // tricky part here, text generating task spamming main thread with events with buffer size of 128, same with bigger 1024 but not that critical
    await fileService.GenerateTextFile(heavyTaskCts.Token, new Progress<int>(value => currentPercentage = value), path, sizeInMb); 
    
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
    await progressTask;

    Console.WriteLine(appStatus);

    File.Delete(path);
    Console.WriteLine("File deleted");
}

return;

void CancelOnCKeyPressed(ConsoleKey keyPressed)
{
    if (keyPressed == ConsoleKey.C) heavyTaskCts.Cancel();
}

void DisplayProgressSpinner(string state)
{
    var returns = string.Join(string.Empty, Enumerable.Range(0, state.Length).Select(x => "\r")); 
    Console.Write($"{returns}{state}");
}

