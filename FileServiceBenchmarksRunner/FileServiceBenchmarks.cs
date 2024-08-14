using BenchmarkDotNet.Attributes;
using ConsolePlayground;

namespace FileServiceBenchmarksRunner;

public class FileServiceBenchmarks
{
    private readonly FileService _fileService = new FileService();

    private int _sizeInMb;
    private int _bufferSize;
    
    
    [Params(100, 256, 1000)] public int SizeInMb;

    [Params(100, 256, 1024)] public int BufferSize;

    [GlobalSetup]
    public void Setup()
    {
        _sizeInMb = SizeInMb;
        _bufferSize = BufferSize;
    }
    
    [Benchmark]
    public async Task RunWithCurrentParams()
    {
        using var cts = new CancellationTokenSource();
        await RunWithSettings(cts.Token, _sizeInMb, _bufferSize);
        await cts.CancelAsync();
    }

    private async Task RunWithSettings(CancellationToken token, int sizeInMb, int bufferSize) {
        var tempFilePath = Path.GetTempFileName();
        try
        {
            await _fileService.GenerateTextFile(token, new Progress<int>(IgnoreProgressHandler), tempFilePath, sizeInMb, bufferSize);
        }
        finally
        {
            File.Delete(tempFilePath);
        }
    }
        
    
    private void IgnoreProgressHandler(int newProgressValue) { }
}