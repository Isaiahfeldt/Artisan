using System;

namespace Artisan.Tasks;

internal static class ListNameCommand
{
    // Existing commands win unless the user explicitly quotes the list name.
    internal static string? GetListName(string args)
    {
        args = args.Trim();
        if (args.Length == 0) return null;
        if (args.Length >= 2 && args[0] == '"' && args[^1] == '"')
            return args[1..^1].Trim();

        var first = args.Split(' ', 2)[0].ToLowerInvariant();
        return first is "lists" or "macros" or "endurance" or "settings"
            or "workshop" or "workshops" or "builder" or "sim" or "automode"
            ? null : args;
    }

    internal static bool Matches(string? name, string requested) =>
        string.Equals(name, requested, StringComparison.OrdinalIgnoreCase);
}
