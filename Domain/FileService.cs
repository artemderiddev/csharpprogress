namespace Domain;

public class FileService
{
    private const string Alphabet = "abcdefghigklmnopqrstuvwxyz";
    
    public async Task GenerateTextFile(CancellationToken token, string path, int lengthMb)
    {
        try
        {
            await using var writer = new StreamWriter(path);

            var buffer = new char[128];
            var bufferCount = lengthMb * 1024 * 1024 / buffer.Length;

            for (int i = 0; i < bufferCount; i++)
            {
                FillRandomText(buffer);
                await writer.WriteAsync(buffer, token);
            }
        }
        catch (OperationCanceledException)
        {
            File.Delete(path);
            throw;
        }
    }

    private void FillRandomText(Span<char> buffer) => Random.Shared.GetItems(Alphabet.AsSpan(), buffer);
}