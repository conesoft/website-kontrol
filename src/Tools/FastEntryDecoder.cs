using System.Net;
using System.Text.RegularExpressions;
using static Conesoft_Website_Kontrol.Components.Pages.Content;

namespace Conesoft_Website_Kontrol.Tools
{
    public partial class FastEntryDecoder
    {
        public static string DecodeDescription(Entry entry)
        {
            var reg = MyRegex().Replace(entry?.DescriptionIntro ?? "", string.Empty);
            return WebUtility.HtmlDecode(reg);
        }

        [GeneratedRegex("<.*?>")]
        private static partial Regex MyRegex();
    }
}