namespace Conesoft_Website_Kontrol.Components.Controls.Finances;

public record Entry(Entry.EntryType Type, DateTime At, string Description, decimal Amount)
{
    public string? For { get; set; }
    public string? By { get; set; }
    public string? To { get; set; }

    public enum EntryType { Invalid, Bill, Income, Compensation }

    public string PathToFile { get; set; } = "";
}