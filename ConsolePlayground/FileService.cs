namespace ConsolePlayground;

public class FileService
{
    private const string Alphabet = "abcdefghigklmnopqrstuvwxyz";
    
    public async Task GenerateTextFile(CancellationToken token, IProgress<int> progress, string path, int lengthMb)
    {
        try
        {
            await using var writer = new StreamWriter(path);

            var buffer = new char[1024]; //128 buffer will spam main threads with even more events via Progress<int>
            var bufferCount = lengthMb * 1024 * 1024 / buffer.Length;
            
            progress.Report(0);
            
            for (int i = 0; i < bufferCount; i++)
            {
                FillRandomTextToBuffer(buffer); // AsSpan() is not necessary since span has implicit conversion operator
                await writer.WriteAsync(buffer, token);
                
                progress.Report((i + 1) * 100 / bufferCount);
            }
        }
        catch (OperationCanceledException)
        {
            File.Delete(path);
            throw;
        }
    }

    private void FillRandomTextToBuffer(Span<char> buffer) => Random.Shared.GetItems(Alphabet.AsSpan(), buffer);
}