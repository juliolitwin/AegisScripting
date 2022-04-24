using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Scripting.Utilities;

public class FileUtilities
{
    public static IEnumerable<string> SplitAndKeep(string s, char[] delims)
    {
        int start = 0, index;

        while ((index = s.IndexOfAny(delims, start)) != -1)
        {
            if (index - start > 0)
            {
                yield return s.Substring(start, index - start);
            }

            yield return s.Substring(index, 1);
            start = index + 1;
        }

        if (start < s.Length)
        {
            yield return s.Substring(start);
        }
    }

    /// <summary>
    ///     Splits the given string into a list of substrings, while outputting the splitting
    ///     delimiters (each in its own string) as well. It's just like String.Split() except
    ///     the delimiters are preserved. No empty strings are output.
    /// </summary>
    /// <param name="s">String to parse. Can be null or empty.</param>
    /// <param name="delimiters">The delimiting characters. Can be an empty array.</param>
    /// <returns></returns>
    public static IList<string> SplitAndKeepDelimiters(string s, params char[] delimiters)
    {
        var parts = new List<string>();
        if (!string.IsNullOrEmpty(s))
        {
            var iFirst = 0;
            do
            {
                var iLast = s.IndexOfAny(delimiters, iFirst);
                if (iLast >= 0)
                {
                    if (iLast > iFirst)
                    {
                        parts.Add(s.Substring(iFirst, iLast - iFirst)); // part before the delimiter
                    }

                    parts.Add(new string(s[iLast], 1)); // the delimiter
                    iFirst = iLast + 1;
                    continue;
                }

                // No delimiters were found, but at least one character remains. Add the rest and stop.
                parts.Add(s.Substring(iFirst, s.Length - iFirst));
                break;
            }
            while (iFirst < s.Length);
        }

        return parts;
    }

    public static string[] SplitAndKeepSeparators(string value, char[] separators, StringSplitOptions splitOptions)
    {
        var splitValues = new List<string>();
        var itemStart   = 0;
        for (var pos = 0; pos < value.Length; pos++)
        {
            for (var sepIndex = 0; sepIndex < separators.Length; sepIndex++)
            {
                if (separators[sepIndex] == value[pos])
                {
                    // add the section of string before the separator 
                    // (unless its empty and we are discarding empty sections)
                    if (itemStart    != pos ||
                        splitOptions == StringSplitOptions.None)
                    {
                        splitValues.Add(value.Substring(itemStart, pos - itemStart));
                    }

                    itemStart = pos + 1;

                    // add the separator
                    splitValues.Add(separators[sepIndex].ToString());
                    break;
                }
            }
        }

        // add anything after the final separator 
        // (unless its empty and we are discarding empty sections)
        if (itemStart    != value.Length ||
            splitOptions == StringSplitOptions.None)
        {
            splitValues.Add(value.Substring(itemStart, value.Length - itemStart));
        }

        return splitValues.ToArray();
    }

    public static List<string> SplitAndGetSeparator(string text, string[] delims)
    {
        var rows = new List<string>
                   {
                       text
                   };
        foreach (var delim in delims)
        {
            for (var i = 0; i < rows.Count; i++)
            {
                var index = rows[i].IndexOf(delim, StringComparison.OrdinalIgnoreCase);
                if (index          > -1 &&
                    rows[i].Length > index + 1)
                {
                    var leftPart  = rows[i][..(index + delim.Length)];
                    var rightPart = rows[i][(index   + delim.Length)..];
                    rows[i] = leftPart;
                    rows.Insert(i + 1, rightPart);
                }
            }
        }

        return rows;
    }

    public static List<string> OpenFileAndGetLines(string path)
    {
        using var streamReader = new StreamReader(path, Encoding.UTF8);
        var       text         = streamReader.ReadToEnd();

        var lines = SplitAndGetSeparator(text, new[]
                                               {
                                                   "\n", Environment.NewLine
                                               });
        streamReader.Close();

        return lines;
    }
}