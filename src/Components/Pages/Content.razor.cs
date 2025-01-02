using Conesoft.Files;
using Conesoft.Tools;
using Humanizer;
using Serilog;
using System.Diagnostics;

namespace Conesoft_Website_Kontrol.Components.Pages;
public partial class Content
{
    private static readonly Conesoft.Files.Directory Storage = Conesoft.Hosting.Host.GlobalStorage / "FromSources" / "Feeds" / "Entries";

    public IEnumerable<Entry> Entries { get; set; } = [];

    public record Entry(string Name, string Url, DateTime Published, string Description, string Category, string Filename)
    {
        private string[] types = ["jpg", "png", "svg", "gif"];
        public string? ImageFilename { get; set; }
        public string DescriptionIntro => $"{string.Join('.', (Description ?? "").Split('.', 5 + 1, StringSplitOptions.None).Take(5))}.";
    }

    protected override async Task OnInitializedAsync()
    {
        var within24Hours = DateTime.Today.AddDays(-1);

        using (Timed.Run("all content"))
        {
            var loadedEntries = Storage
                .FilteredFiles("*", allDirectories: false)
                .Where(f => f.Info.CreationTime > within24Hours)
                .OrderByDescending(f => f.Info.CreationTime)
                .ToArray(); // don't remove this! huge performance issue if you do

            var entries = await loadedEntries.Where(f => f.Extension == ".json").ReadFromJson<Entry>();
            Entries = entries.NotNull().Select(e =>
            {
                var c = e.Content;
                c.ImageFilename = loadedEntries.FirstOrDefault(f => f.Extension != ".json" && f.Name.StartsWith(e.NameWithoutExtension))?.Name;
                return c;
            });
        }
    }

    class Timed : IDisposable
    {
        private readonly Stopwatch timer;
        private readonly string message;

        private Timed(string message)
        {
            this.message = message;
            timer = new Stopwatch();
            timer.Start();
        }

        public static Timed Run(string message) => new(message);

        public void Dispose()
        {
            timer.Stop();
            Log.Information("{message}: {timer}", message, timer.Elapsed.Humanize());
        }
    }
}
