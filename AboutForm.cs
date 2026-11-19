namespace BookCatalog;

public class AboutForm : Form
{
    public string FullName { get; } = "Кравчук Яна ОЛександрівна";   // <-- ВАШЕ ПІБ
    public int Course { get; } = 2;                                   // <-- КУРС
    public string Group { get; } = "КН-26";                           // <-- ГРУПА
    public int Year { get; } = 2026;                                  // <-- РІК
    public string Description { get; } =
        "Програма для роботи з каталогом книг у форматі JSON: відкриття файлу, " +
        "перегляд, додавання, редагування, видалення, пошук (LINQ) та збереження.";

    public AboutForm()
    {
        Text = "Про програму";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new Size(420, 260);

        var info = new Label
        {
            Left = 20,
            Top = 20,
            Width = 380,
            Height = 180,
            Text = $"ПІБ: {FullName}\n" +
                   $"Курс: {Course}\n" +
                   $"Група: {Group}\n" +
                   $"Рік: {Year}\n\n" +
                   $"{Description}"
        };

        var btnOk = new Button
        {
            Text = "OK",
            Left = 160,
            Top = 215,
            Width = 100,
            DialogResult = DialogResult.OK
        };

        Controls.Add(info);
        Controls.Add(btnOk);
        AcceptButton = btnOk;
    }
}