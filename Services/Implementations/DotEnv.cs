namespace ChurchPortal.Services.Implementations
{
    /// <summary>
    /// Minimal .env loader. Each KEY=VALUE line becomes a process environment
    /// variable unless that variable is already set, so real environment
    /// variables (Render dashboard, shell, launchSettings) always take priority.
    /// </summary>
    public static class DotEnv
    {
        public static void Load(string path)
        {
            if (!File.Exists(path))
            {
                return;
            }

            foreach (var rawLine in File.ReadAllLines(path))
            {
                var line = rawLine.Trim();
                if (line.Length == 0 || line.StartsWith('#'))
                {
                    continue;
                }

                if (line.StartsWith("export ", StringComparison.Ordinal))
                {
                    line = line["export ".Length..].TrimStart();
                }

                var separator = line.IndexOf('=');
                if (separator <= 0)
                {
                    continue;
                }

                var key = line[..separator].Trim();
                var value = line[(separator + 1)..].Trim();

                if (value.Length >= 2 &&
                    ((value[0] == '"' && value[^1] == '"') || (value[0] == '\'' && value[^1] == '\'')))
                {
                    value = value[1..^1];
                }

                // An empty value in the file should not blank out anything.
                if (value.Length == 0)
                {
                    continue;
                }

                if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable(key)))
                {
                    Environment.SetEnvironmentVariable(key, value);
                }
            }
        }
    }
}
