using System.Text.Json;

namespace BookCatalog;

public class JsonFileService
{
    // WriteIndented – красивый JSON с отступами
    // UnsafeRelaxedJsonEscaping – кирилица пишется буквами, а не \u0412...
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public async Task<List<Book>> LoadAsync(string path)
    {
        string json = await File.ReadAllTextAsync(path);
        return Deserialize(json);
    }

    public async Task SaveAsync(string path, List<Book> books)
    {
        string json = Serialize(books);
        await File.WriteAllTextAsync(path, json);
    }

    private string Serialize(List<Book> books)
    {
        return JsonSerializer.Serialize(books, Options);
    }

    private List<Book> Deserialize(string json)
    {
        return JsonSerializer.Deserialize<List<Book>>(json, Options) ?? new List<Book>();
    }
}