namespace EasySave.Model.Backup
{
    /// <summary>
    /// Parses CLI job-selection arguments.
    /// Supports:
    ///   "1-3"   → indices 1, 2, 3
    ///   "1;3"   → indices 1 and 3
    ///   "2"     → index 2 only
    ///   Mixed: "1-3;5" → 1, 2, 3, 5
    /// Indices are 1-based (as shown in the UI).
    /// </summary>
    public static class BackupJobArgsParser
    {
        public static List<int> Parse(string arg)
        {
            var indices = new SortedSet<int>();

            // Split on semicolons first
            foreach (var segment in arg.Split(';', StringSplitOptions.RemoveEmptyEntries))
            {
                var part = segment.Trim();

                if (part.Contains('-'))
                {
                    // Range: "1-3"
                    var bounds = part.Split('-');
                    if (bounds.Length == 2
                        && int.TryParse(bounds[0].Trim(), out int from)
                        && int.TryParse(bounds[1].Trim(), out int to)
                        && from <= to)
                    {
                        for (int i = from; i <= to; i++)
                            indices.Add(i);
                    }
                    else
                    {
                        throw new FormatException(
                            $"Invalid range segment: '{part}'. Expected format '1-3'.");
                    }
                }
                else if (int.TryParse(part, out int single))
                {
                    indices.Add(single);
                }
                else
                {
                    throw new FormatException(
                        $"Invalid segment: '{part}'. Expected a number or a range like '1-3'.");
                }
            }

            return indices.ToList();
        }
    }
}