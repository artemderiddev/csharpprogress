using Domain;

var cancellationTokenSource = new CancellationTokenSource();

var path = Path.GetTempFileName();
var appStatus = string.Empty;
var sizeInMb = 200;

try
{
    FileService fileService = new();
    await fileService.GenerateTextFile(cancellationTokenSource.Token, path, sizeInMb);
    appStatus = "Success";
}
catch (OperationCanceledException)
{
    appStatus = "Canceled";
}
finally
{
    File.Delete(path);
    Console.WriteLine(appStatus);
}