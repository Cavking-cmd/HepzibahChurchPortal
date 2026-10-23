using System.Text.RegularExpressions;

namespace ChurchPortal.Services.Interfaces
{
    public static class SerialNumberScreener
    {
        private static readonly string[] SerializedItemKeywords =
        {
            "laptop", "computer", "desktop", "printer", "scanner", "copier",
            "phone", "smartphone", "mobile", "tablet", "ipad", "iphone",
            "camera", "camcorder", "projector", "television", "tv", "monitor",
            "router", "modem", "server", "nas", "switch", "hard drive",
            "ssd", "ups", "microphone", "mixer", "amplifier", "speaker",
            "console", "drone", "smartwatch", "chromebook"
        };

        public static bool RequiresSerialNumber(string? itemName)
        {
            if (string.IsNullOrWhiteSpace(itemName))
            {
                return false;
            }

            var normalized = itemName.ToLowerInvariant();

            return SerializedItemKeywords.Any(keyword =>
                Regex.IsMatch(normalized, $@"\b{Regex.Escape(keyword)}\b", RegexOptions.IgnoreCase));
        }
    }
}
