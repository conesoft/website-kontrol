namespace Conesoft_Website_Kontrol.Components.Controls.Finances;

public record Entry(Entry.EntryType Type, DateTime At, string Description, decimal Amount, Entry.RepetitionType Every, string? For, string? By, string? To)
{
    public enum EntryType { Invalid, Bill, Income, Compensation }
    public enum RepetitionType { NoRepetition, Month }

    public string PathToFile { get; set; }
}