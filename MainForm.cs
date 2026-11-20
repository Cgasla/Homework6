using System.ComponentModel;
using System.Text.Json;

namespace BookCatalog;

public class MainForm : Form
{
    private readonly BookRepository repo = new();
    private readonly JsonFileService jsonService = new();
    private string? currentPath;

    private readonly DataGridView dataGrid = new();
    private readonly ComboBox cbCriterion = new();
    private readonly TextBox tbValue = new();
    private readonly TextBox tbTo = new();
    private readonly Label lblTo = new();

    public MainForm()
    {
        Text = "Каталог книг (JSON)";
        Width = 950;
        Height = 600;
        StartPosition = FormStartPosition.CenterScreen;

        BuildUi();
        dataGrid.DataSource = repo.GetAll();
    }

    private void BuildUi()
    {
        dataGrid.Dock = DockStyle.Fill;
        dataGrid.AutoGenerateColumns = false;
        dataGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        dataGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        dataGrid.MultiSelect = false;
        dataGrid.ReadOnly = true;
        dataGrid.AllowUserToAddRows = false;
        dataGrid.AllowUserToDeleteRows = false;
        dataGrid.RowHeadersVisible = false;

        dataGrid.Columns.Add(new DataGridViewTextBoxColumn
        { DataPropertyName = "Id", HeaderText = "ID", FillWeight = 8 });
        dataGrid.Columns.Add(new DataGridViewTextBoxColumn
        { DataPropertyName = "Title", HeaderText = "Назва", FillWeight = 35 });
        dataGrid.Columns.Add(new DataGridViewTextBoxColumn
        { DataPropertyName = "Author", HeaderText = "Автор", FillWeight = 25 });
        dataGrid.Columns.Add(new DataGridViewTextBoxColumn
        { DataPropertyName = "Year", HeaderText = "Рік", FillWeight = 10 });
        dataGrid.Columns.Add(new DataGridViewTextBoxColumn
        { DataPropertyName = "Genre", HeaderText = "Жанр", FillWeight = 15 });
        var priceCol = new DataGridViewTextBoxColumn
        { DataPropertyName = "Price", HeaderText = "Ціна", FillWeight = 12 };
        priceCol.DefaultCellStyle.Format = "N2";
        dataGrid.Columns.Add(priceCol);

        dataGrid.CellDoubleClick += (s, e) => { if (e.RowIndex >= 0) OnEdit(); };

        var buttonPanel = new FlowLayoutPanel
        { Dock = DockStyle.Top, AutoSize = true, Padding = new Padding(5) };

        var addBtn = new Button { Text = "Додати", AutoSize = true };
        addBtn.Click += (s, e) => OnAdd();
        var editBtn = new Button { Text = "Редагувати", AutoSize = true };
        editBtn.Click += (s, e) => OnEdit();
        var delBtn = new Button { Text = "Видалити", AutoSize = true };
        delBtn.Click += (s, e) => OnDelete();
        var helpBtn = new Button { Text = "Довідка", AutoSize = true };
        helpBtn.Click += (s, e) => ShowHelp();

        buttonPanel.Controls.AddRange(new Control[] { addBtn, editBtn, delBtn, helpBtn });
        // ----- Панель пошуку -----
        var searchPanel = new FlowLayoutPanel
        { Dock = DockStyle.Top, AutoSize = true, Padding = new Padding(5) };

        cbCriterion.DropDownStyle = ComboBoxStyle.DropDownList;
        cbCriterion.Items.AddRange(new object[]
            { "За назвою", "За автором", "За роком видання (від – до)" });
        cbCriterion.Width = 200;
        cbCriterion.SelectedIndex = 0;
        cbCriterion.SelectedIndexChanged += (s, e) =>
        {
            bool isYear = cbCriterion.SelectedIndex == 2;
            lblTo.Visible = isYear;
            tbTo.Visible = isYear;
        };

        tbValue.Width = 180;

        lblTo.Text = "до:";
        lblTo.AutoSize = true;
        lblTo.Margin = new Padding(6, 6, 0, 0);
        lblTo.Visible = false;

        tbTo.Width = 80;
        tbTo.Visible = false;

        var searchBtn = new Button { Text = "Знайти", AutoSize = true };
        searchBtn.Click += (s, e) => OnSearch();
        var resetBtn = new Button { Text = "Скинути", AutoSize = true };
        resetBtn.Click += (s, e) => ResetSearch();

        searchPanel.Controls.AddRange(new Control[]
            { cbCriterion, tbValue, lblTo, tbTo, searchBtn, resetBtn });

        var menu = new MenuStrip();
        var fileMenu = new ToolStripMenuItem("Файл");
        fileMenu.DropDownItems.AddRange(new ToolStripItem[]
        {
            new ToolStripMenuItem("Відкрити...", null, async (s, e) => await OnOpenFile()),
            new ToolStripMenuItem("Зберегти", null, async (s, e) => await OnSaveFile(false)),
            new ToolStripMenuItem("Зберегти як...", null, async (s, e) => await OnSaveFile(true)),
            new ToolStripSeparator(),
            new ToolStripMenuItem("Вихід", null, (s, e) => Close())
        });
        var aboutItem = new ToolStripMenuItem("Про програму", null, (s, e) => ShowAbout());
        menu.Items.AddRange(new ToolStripItem[] { fileMenu, aboutItem });

        // Порядок додавання важливий для Dock: Fill першим, меню останнім
        Controls.Add(dataGrid);
        Controls.Add(buttonPanel);
        Controls.Add(searchPanel);
        Controls.Add(menu);
        MainMenuStrip = menu;
    }

    private async Task OnOpenFile()
    {
        using var dlg = new OpenFileDialog
        {
            Filter = "JSON файли (*.json)|*.json|Усі файли (*.*)|*.*",
            Title = "Відкрити JSON-файл"
        };
        if (dlg.ShowDialog() != DialogResult.OK) return;

        try
        {
            var books = await jsonService.LoadAsync(dlg.FileName);
            repo.Init(books);
            currentPath = dlg.FileName;
            ResetSearch();
            Text = "Каталог книг (JSON) – " + Path.GetFileName(currentPath);
        }
        catch (JsonException)
        {
            MessageBox.Show("Файл має неправильний формат JSON.", "Помилка",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        catch (Exception ex)
        {
            MessageBox.Show("Не вдалося відкрити файл:\n" + ex.Message, "Помилка",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async Task OnSaveFile(bool saveAs)
    {
        if (repo.GetAll().Count == 0)
        {
            var ask = MessageBox.Show(
                "Список порожній. Файл буде перезаписано порожнім списком. Продовжити?",
                "Увага", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (ask != DialogResult.Yes) return;
        }
        string? path = currentPath;

        if (saveAs || path == null)
        {
            using var dlg = new SaveFileDialog
            {
                Filter = "JSON файли (*.json)|*.json",
                DefaultExt = "json",
                FileName = "books.json",
                Title = "Зберегти як"
            };
            if (dlg.ShowDialog() != DialogResult.OK) return;
            path = dlg.FileName;
        }

        try
        {
            await jsonService.SaveAsync(path, repo.GetAll().ToList());
            currentPath = path;
            Text = "Каталог книг (JSON) – " + Path.GetFileName(path);
            MessageBox.Show("Дані збережено.", "Готово",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show("Не вдалося зберегти файл:\n" + ex.Message, "Помилка",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private Book? GetSelectedBook() => dataGrid.CurrentRow?.DataBoundItem as Book;
    private void OnAdd()
    {
        using var form = new BookEditForm(
            new Book { Year = DateTime.Now.Year },
            (title, author) => repo.Exists(title, author));

        if (form.ShowDialog(this) == DialogResult.OK)
        {
            repo.Add(form.Book);
            ResetSearch();
        }
    }

    private void OnEdit()
    {
        var selected = GetSelectedBook();
        if (selected == null)
        {
            MessageBox.Show("Спочатку виберіть запис у таблиці.", "Редагування",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var copy = new Book
        {
            Id = selected.Id,
            Title = selected.Title,
            Author = selected.Author,
            Year = selected.Year,
            Genre = selected.Genre,
            Price = selected.Price
        };

        using var form = new BookEditForm(
            copy,
            (title, author) => repo.Exists(title, author, copy.Id));

        if (form.ShowDialog(this) == DialogResult.OK)
        {
            repo.Update(form.Book);
            ResetSearch();
        }
    }

    private void OnDelete()
    {
        var selected = GetSelectedBook();
        if (selected == null)
        {
            MessageBox.Show("Спочатку виберіть запис у таблиці.");
            return;
        }

        var answer = MessageBox.Show($"Видалити книгу «{selected.Title}»?", "Підтвердження",
            MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        if (answer == DialogResult.Yes)
        {
            repo.Delete(selected.Id);
            ResetSearch();
        }
    }

    private void OnSearch()
    {
        if (repo.GetAll().Count == 0)
        {
            MessageBox.Show("Список порожній. Спочатку відкрийте JSON-файл або додайте книгу.",
                "Пошук", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        IEnumerable<Book> result;
        string text = tbValue.Text.Trim();

        switch (cbCriterion.SelectedIndex)
        {
            case 0:
            case 1:
                if (text.Length == 0)
                {
                    MessageBox.Show("Введіть текст для пошуку.", "Пошук",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    tbValue.Focus();
                    return;
                }
                result = cbCriterion.SelectedIndex == 0
                    ? repo.SearchByTitle(text)
                    : repo.SearchByAuthor(text);
                break;

            default:
                if (!int.TryParse(tbValue.Text, out int yearFrom))
                {
                    MessageBox.Show("Введіть рік «від» числом, наприклад 1990.", "Пошук",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    tbValue.Focus();
                    return;
                }
                int yearTo = int.MaxValue;
                if (!string.IsNullOrWhiteSpace(tbTo.Text) && !int.TryParse(tbTo.Text, out yearTo))
                {
                    MessageBox.Show("Рік «до» має бути числом.", "Пошук",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    tbTo.Focus();
                    return;
                }
                if (yearFrom > yearTo)
                {
                    MessageBox.Show("Рік «від» не може бути більшим за рік «до».", "Пошук",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                result = repo.FilterByYear(yearFrom, yearTo);
                break;
        }

        var found = result.ToList();
        if (found.Count == 0)
        {
            MessageBox.Show("За вашим запитом нічого не знайдено.", "Пошук",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        dataGrid.DataSource = new BindingList<Book>(found);
    }
    private void ResetSearch()
    {
        tbValue.Clear();
        tbTo.Clear();
        dataGrid.DataSource = repo.GetAll();
    }

    private void ShowAbout()
    {
        using var form = new AboutForm();
        form.ShowDialog(this);
    }
    private void ShowHelp()
    {
        MessageBox.Show(
            "Цю програму розробила Яна Кравчук,\n" +
            "група К-26.",
            "Довідка",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);
    }
}