namespace BookCatalog;

public class BookEditForm : Form
{
    public Book Book { get; private set; }

    private readonly Func<string, string, bool>? isDuplicate;

    private readonly TextBox tbTitle = new();
    private readonly TextBox tbAuthor = new();
    private readonly TextBox tbGenre = new();
    private readonly NumericUpDown nudYear = new();
    private readonly NumericUpDown nudPrice = new();

    public BookEditForm(Book book, Func<string, string, bool>? isDuplicate = null)
    {
        Book = book;
        this.isDuplicate = isDuplicate;

        Text = book.Id == 0 ? "Додати книгу" : "Редагувати книгу";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new Size(360, 260);

        nudYear.Minimum = 1;
        nudYear.Maximum = 2100;
        nudYear.Value = Math.Clamp(book.Year, 1, 2100);

        nudPrice.DecimalPlaces = 2;
        nudPrice.Maximum = 1000000;
        nudPrice.Value = Math.Clamp(book.Price, 0, 1000000);

        tbTitle.Text = book.Title;
        tbAuthor.Text = book.Author;
        tbGenre.Text = book.Genre;

        AddRow("Назва:", tbTitle, 15);
        AddRow("Автор:", tbAuthor, 50);
        AddRow("Рік видання:", nudYear, 85);
        AddRow("Жанр:", tbGenre, 120);
        AddRow("Ціна:", nudPrice, 155);

        var btnSave = new Button { Text = "Зберегти", Left = 120, Top = 205, Width = 100 };
        btnSave.Click += OnSave;

        var btnCancel = new Button
        {
            Text = "Скасувати",
            Left = 230,
            Top = 205,
            Width = 100,
            DialogResult = DialogResult.Cancel
        };

        Controls.Add(btnSave);
        Controls.Add(btnCancel);
        AcceptButton = btnSave;
        CancelButton = btnCancel;
    }

    private void AddRow(string caption, Control input, int y)
    {
        Controls.Add(new Label { Text = caption, Left = 15, Top = y + 4, Width = 100 });
        input.Left = 120;
        input.Top = y;
        input.Width = 210;
        Controls.Add(input);
    }

    private void OnSave(object? sender, EventArgs e)
    {
        string title = tbTitle.Text.Trim();
        string author = tbAuthor.Text.Trim();
        string genre = tbGenre.Text.Trim();
        int currentYear = DateTime.Now.Year;

        if (title.Length == 0)
        {
            Warn("Введіть назву книги.", tbTitle);
            return;
        }
        if (author.Length == 0)
        {
            Warn("Введіть автора.", tbAuthor);
            return;
        }
        if (genre.Length == 0)
        {
            Warn("Введіть жанр.", tbGenre);
            return;
        }
        if (nudYear.Value < 1000 || nudYear.Value > currentYear)
        {
            Warn($"Рік видання має бути від 1000 до {currentYear}.", nudYear);
            return;
        }
        if (nudPrice.Value <= 0)
        {
            Warn("Ціна має бути більшою за 0.", nudPrice);
            return;
        }
        if (isDuplicate?.Invoke(title, author) == true)
        {
            Warn("Книга з такою назвою й автором уже є в списку.", tbTitle);
            return;
        }

        Book.Title = title;
        Book.Author = author;
        Book.Genre = genre;
        Book.Year = (int)nudYear.Value;
        Book.Price = nudPrice.Value;

        DialogResult = DialogResult.OK;
    }

    private void Warn(string message, Control field)
    {
        MessageBox.Show(message, "Некоректні дані",
            MessageBoxButtons.OK, MessageBoxIcon.Warning);
        field.Focus();
    }
}