using System;
using System.Text.RegularExpressions;

namespace StudioCommunication;

/// A parsed command line inside a TAS file
public readonly record struct CommandLine(
    string Command,
    string[] Arguments,

    string OriginalText,
    string ArgumentSeparator
) {
    // Matches against command or space or both as a separator
    public static readonly Regex SeparatorRegex = new(@"(?:\s+)|(?:\s*,\s*)", RegexOptions.Compiled);

    public bool IsCommand(string? command) => string.Equals(command, Command, StringComparison.OrdinalIgnoreCase);

    public static CommandLine? Parse(string line) => TryParse(line, out var commandLine) ? commandLine : null;
    public static bool TryParse(string line, out CommandLine commandLine) {
        var separatorMatch = SeparatorRegex.Match(line);
        string[] split = line.Split(separatorMatch.Value);

        if (split.Length == 0) {
            commandLine = default;
            return false;
        }

        commandLine = new CommandLine {
            Command = split[0],
            Arguments = split[1..],

            OriginalText = line,
            ArgumentSeparator = separatorMatch.Value,
        };

        return true;
    }
}
