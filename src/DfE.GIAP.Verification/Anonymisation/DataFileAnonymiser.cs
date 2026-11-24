using System.Text;

namespace DfE.GIAP.Verification.Anonymisation;

/// <summary>
/// Anonymises a tab-separated data file as it's streamed, changing only the bytes of the values it replaces. Everything
/// else, including the BOM, quoting, line endings and every other column, is written exactly as it was, so the
/// pipelines read the anonymised file as they would the real one.
/// </summary>
/// <remarks>
/// Values are read as the pipelines read them: a value is quoted if it starts with <c>"</c>, and is trimmed.
/// An empty value, or <c>NULL</c>, is left as it is.
/// </remarks>
public sealed class DataFileAnonymiser(ValueAnonymiser values)
{
    private static readonly byte[] _bom = [0xEF, 0xBB, 0xBF];

    /// <returns>How many rows there were, not counting the header.</returns>
    /// <exception cref="AnonymisationException">A column isn't classified, a row has more values than the header, or a value can't be anonymised.</exception>
    public int Anonymise(Stream input, Stream output, FolderClassification classification)
    {
        RecordReader reader = new(input);
        Record record = new();

        if (!reader.TryRead(record))
        {
            return 0;
        }

        string[] header = [.. Enumerable.Range(0, record.Fields.Count).Select(i => Decode(record, i, out _).Trim())];

        string[] unclassified = [.. header.Where(c => classification.GetTransform(c) is null)];
        if (unclassified.Length > 0)
        {
            throw new AnonymisationException(
                $"The data file has columns that aren't classified: {string.Join(", ", unclassified.Select(c => $"'{c}'"))}.");
        }

        ColumnTransform[] transforms = [.. header.Select(c => classification.GetTransform(c)!.Value)];
        int subjectColumn = classification.Subject is null ? -1 : Array.FindIndex(header, c => string.Equals(c, classification.Subject, StringComparison.OrdinalIgnoreCase));
        int sexColumn = classification.Sex is null ? -1 : Array.FindIndex(header, c => string.Equals(c, classification.Sex, StringComparison.OrdinalIgnoreCase));

        output.Write(record.Raw, 0, record.Length);

        int rows = 0;
        byte[]?[] replacements = new byte[]?[header.Length];
        HashSet<string> originals = new(StringComparer.OrdinalIgnoreCase);

        while (reader.TryRead(record))
        {
            rows++;

            if (record.Fields.Count > header.Length)
            {
                throw new AnonymisationException($"Row {rows} has {record.Fields.Count} values, but the header has {header.Length}.");
            }

            string? subject = subjectColumn >= 0 && subjectColumn < record.Fields.Count ? ValueOrNull(Decode(record, subjectColumn, out _)) : null;
            Sex sex = sexColumn >= 0 && sexColumn < record.Fields.Count ? ValueAnonymiser.ParseSex(Decode(record, sexColumn, out _)) : Sex.Unknown;

            Array.Clear(replacements);
            originals.Clear();

            for (int i = 0; i < record.Fields.Count; i++)
            {
                if (transforms[i] == ColumnTransform.Keep)
                {
                    continue;
                }

                string raw = Decode(record, i, out bool quoted);
                if (ValueOrNull(raw) is not { } value)
                {
                    continue;
                }

                if (ValueAnonymiser.IsIdentifying(transforms[i]))
                {
                    originals.Add(value);
                }

                string anonymised = values.Anonymise(transforms[i], value, subject, sex);
                replacements[i] = Encoding.UTF8.GetBytes(quoted ? $"\"{anonymised.Replace("\"", "\"\"")}\"" : Rewrap(raw, value, anonymised));
            }

            CheckNothingLeaks(record, replacements, originals, header, rows);
            Write(output, record, replacements);
        }

        return rows;
    }

    // Stops the file if an identifier or name that was replaced is still in the row, in any column
    private static void CheckNothingLeaks(Record record, byte[]?[] replacements, HashSet<string> originals, string[] header, int row)
    {
        if (originals.Count == 0)
        {
            return;
        }

        for (int i = 0; i < record.Fields.Count; i++)
        {
            string value = replacements[i] is { } replacement ? Unquote(Encoding.UTF8.GetString(replacement), out _).Trim() : Decode(record, i, out _).Trim();
            if (value.Length > 0 && originals.Contains(value))
            {
                throw new AnonymisationException($"Row {row} would still have a replaced value, in '{header[i]}'.");
            }
        }
    }

    private static void Write(Stream output, Record record, byte[]?[] replacements)
    {
        int position = 0;
        for (int i = 0; i < record.Fields.Count; i++)
        {
            (int start, int end) = record.Fields[i];
            output.Write(record.Raw, position, start - position);

            if (replacements[i] is { } replacement)
            {
                output.Write(replacement);
            }
            else
            {
                output.Write(record.Raw, start, end - start);
            }

            position = end;
        }

        output.Write(record.Raw, position, record.Length - position);
    }

    internal static string Decode(Record record, int field, out bool quoted)
    {
        (int start, int end) = record.Fields[field];
        if (field == 0 && record.Raw.AsSpan(start, end - start).StartsWith(_bom))
        {
            start += _bom.Length;
        }

        return Unquote(Encoding.UTF8.GetString(record.Raw, start, end - start), out quoted);
    }

    private static string Unquote(string raw, out bool quoted)
    {
        quoted = raw.Length >= 2 && raw[0] == '"' && raw[^1] == '"';
        return quoted ? raw[1..^1].Replace("\"\"", "\"") : raw;
    }

    internal static string? ValueOrNull(string raw)
    {
        string value = raw.Trim();
        return value.Length == 0 || value.Equals("NULL", StringComparison.OrdinalIgnoreCase) ? null : value;
    }

    // Keeps the whitespace around an unquoted value
    private static string Rewrap(string raw, string value, string anonymised)
    {
        int start = raw.IndexOf(value, StringComparison.Ordinal);
        return raw[..start] + anonymised + raw[(start + value.Length)..];
    }

    internal sealed class Record
    {
        public byte[] Raw { get; private set; } = new byte[4096];

        public int Length { get; private set; }

        /// <summary>
        /// Each value's bytes in <see cref="Raw"/>; the bytes between them are tabs, and those after the last are the
        /// line ending.
        /// </summary>
        public List<(int Start, int End)> Fields { get; } = [];

        public void Clear()
        {
            Length = 0;
            Fields.Clear();
        }

        public void Append(byte value)
        {
            if (Length == Raw.Length)
            {
                byte[] raw = new byte[Raw.Length * 2];
                Raw.AsSpan(0, Length).CopyTo(raw);
                Raw = raw;
            }

            Raw[Length++] = value;
        }
    }

    // Splits the file into records as the pipelines' CSV reader does: on tabs and line endings outside quotes, where
    // a value is quoted if it starts with a quote
    internal sealed class RecordReader(Stream stream)
    {
        private readonly byte[] _buffer = new byte[81920];
        private int _position;
        private int _length;

        public bool TryRead(Record record)
        {
            record.Clear();

            int fieldStart = 0;
            bool inQuotes = false;

            while (Next() is int b)
            {
                record.Append((byte)b);
                int position = record.Length - 1;

                if (inQuotes)
                {
                    if (b == '"')
                    {
                        if (Peek() == '"')
                        {
                            record.Append((byte)Next()!.Value);
                        }
                        else
                        {
                            inQuotes = false;
                        }
                    }

                    continue;
                }

                if (b == '"' && position == fieldStart)
                {
                    inQuotes = true;
                }
                else if (b == '\t')
                {
                    record.Fields.Add((fieldStart, position));
                    fieldStart = position + 1;
                }
                else if (b is '\n' or '\r')
                {
                    record.Fields.Add((fieldStart, position));
                    if (b == '\r' && Peek() == '\n')
                    {
                        record.Append((byte)Next()!.Value);
                    }

                    return true;
                }
            }

            if (record.Length == 0)
            {
                return false;
            }

            record.Fields.Add((fieldStart, record.Length));
            return true;
        }

        private int? Peek()
        {
            if (_position == _length && !Fill())
            {
                return null;
            }

            return _buffer[_position];
        }

        private int? Next()
        {
            int? b = Peek();
            if (b is not null)
            {
                _position++;
            }

            return b;
        }

        private bool Fill()
        {
            _length = stream.Read(_buffer, 0, _buffer.Length);
            _position = 0;
            return _length > 0;
        }
    }
}
