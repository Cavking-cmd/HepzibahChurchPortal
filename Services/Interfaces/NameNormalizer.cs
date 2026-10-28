using System.Text.RegularExpressions;

namespace ChurchPortal.Services.Interfaces
{
    public static class NameNormalizer
    {
        private static readonly Regex Whitespace = new(@"\s+", RegexOptions.Compiled);

        // Titles stripped only when comparing pastor names for possible duplicates,
        // so "Pastor John Ade" and "john ade" are flagged as maybe the same person.
        private static readonly HashSet<string> Titles = new(StringComparer.OrdinalIgnoreCase)
        {
            "pastor", "pst", "pst.", "ps", "ps.", "rev", "rev.", "reverend", "dr", "dr.", "evang", "evang.",
            "evangelist", "minister", "min", "min.", "deacon", "dcn", "dcn.", "deaconess", "elder", "bishop",
            "apostle", "prophet", "mr", "mr.", "mrs", "mrs.", "ms", "ms.", "pastora"
        };

        /// <summary>Trims and collapses repeated spaces. Use for the value that is saved.</summary>
        public static string Clean(string? value)
        {
            return Whitespace.Replace((value ?? string.Empty).Trim(), " ");
        }

        /// <summary>Clean + upper-case. Stored in NormalizedName columns.</summary>
        public static string Normalize(string? value)
        {
            return Clean(value).ToUpperInvariant();
        }

        /// <summary>Normalize, then drop leading titles. Used only for duplicate warnings.</summary>
        public static string ComparisonKey(string? value)
        {
            var words = Clean(value).Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList();
            while (words.Count > 1 && Titles.Contains(words[0]))
            {
                words.RemoveAt(0);
            }

            return string.Join(' ', words).Replace(".", string.Empty).ToUpperInvariant();
        }
    }
}
