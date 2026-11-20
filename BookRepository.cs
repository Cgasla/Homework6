using System.ComponentModel;

namespace BookCatalog;

public class BookRepository
{
    private readonly BindingList<Book> books = new();

    public BindingList<Book> GetAll() => books;

    public void Init(IEnumerable<Book> items)
    {
        books.Clear();
        foreach (var b in items) books.Add(b);
    }

    public void Add(Book book)
    {
        book.Id = books.Count == 0 ? 1 : books.Max(b => b.Id) + 1;
        books.Add(book);
    }

    public void Update(Book book)
    {
        int index = books.ToList().FindIndex(b => b.Id == book.Id);
        if (index < 0) return;

        var existing = books[index];
        existing.Title = book.Title;
        existing.Author = book.Author;
        existing.Year = book.Year;
        existing.Genre = book.Genre;
        existing.Price = book.Price;
        books.ResetItem(index); // оновити рядок у таблиці
    }

    public void Delete(int id)
    {
        var book = books.FirstOrDefault(b => b.Id == id);
        if (book == null) return;

        books.Remove(book);
        RenumberIds(); 
    }

    private void RenumberIds()
    {
        for (int i = 0; i < books.Count; i++)
            books[i].Id = i + 1;

        books.ResetBindings(); 
    }

    public bool Exists(string title, string author, int excludeId = 0) =>
        books.Any(b => b.Id != excludeId &&
                       b.Title.Equals(title, StringComparison.OrdinalIgnoreCase) &&
                       b.Author.Equals(author, StringComparison.OrdinalIgnoreCase));

    // ===== 3 критерії пошуку через LINQ =====

    public IEnumerable<Book> SearchByTitle(string text) =>
        books.Where(b => b.Title.Contains(text, StringComparison.OrdinalIgnoreCase));

    public IEnumerable<Book> SearchByAuthor(string author) =>
        books.Where(b => b.Author.Contains(author, StringComparison.OrdinalIgnoreCase));

    public IEnumerable<Book> FilterByYear(int from, int to) =>
        books.Where(b => b.Year >= from && b.Year <= to);
}