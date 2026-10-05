using System.Globalization;
using System.Text;

namespace DentalClinicSystem.Helpers;

public sealed record CsvSection(string Title, IReadOnlyList<string> Headers, IReadOnlyList<IReadOnlyList<object?>> Rows);

public static class CsvExporter
{
    public static byte[] Export(IEnumerable<CsvSection> sections)
    {
        var output = new StringBuilder();
        foreach (var section in sections)
        {
            Row(output, [section.Title]); Row(output, section.Headers.Cast<object?>());
            foreach (var row in section.Rows) Row(output, row);
            output.Append("\r\n");
        }
        var encoding = new UTF8Encoding(true);
        return [.. encoding.GetPreamble(), .. encoding.GetBytes(output.ToString())];
    }
    private static void Row(StringBuilder output, IEnumerable<object?> cells) =>
        output.AppendJoin(',', cells.Select(Cell)).Append("\r\n");

    private static string Cell(object? value)
    {
        var text = value switch
        {
            null => string.Empty,
            DateTime date => date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            DateOnly date => date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            string content => Neutralize(content),
            IFormattable number => number.ToString(null, CultureInfo.InvariantCulture),
            _ => Neutralize(value.ToString() ?? string.Empty)
        };
        return text.IndexOfAny([',', '"', '\r', '\n']) >= 0 ? "\"" + text.Replace("\"", "\"\"") + "\"" : text;
    }
    private static string Neutralize(string text) => text.Length > 0 && text[0] is '=' or '+' or '-' or '@' ? "'" + text : text;
}
