using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using NetFabric.Hyperlinq;
using Scripting.Utilities;

namespace Scripting.Aegis;

public class Script
{
    private const string BlockCommentBegin = "/*";
    private const string BlockCommentEnd   = "*/";
    private const string LineComment       = "//";
    private const string StringBegin       = "\"";
    private const string StringEnd         = "\"";

    private readonly TokenMap _tokenMap = new();
    private          bool     _comment;

    private string _data = "";
    private int    _dataSize;

    private ScriptHandler _handler = default!;

    private List<string> _lines = new();

    public int GetLineNum()
    {
        return this._lines?.Count ?? 0;
    }

    public string GetLine(int pos)
    {
        return this._lines[pos];
    }

    public bool Analyze(int pos)
    {
        return this._handler.AnalyzeLine(this._lines[pos]);
    }

    public void RegisterHandler(ScriptHandler handler)
    {
        this._handler = handler;
        this._handler.SetTokenMap(this._tokenMap);
    }

    internal bool Load(string fName, int verDate)
    {
        this.Clear();

        var scan = new ScanFormatted();

        using StreamReader streamReader = new(fName, Encoding.UTF8);
        this._data     = streamReader.ReadToEnd();
        this._dataSize = this._data.Length;

        if (verDate != 0)
        {
            // MMDDYYYY
            const string dateExpected       = "0000000";
            const int    dateExpectedLength = 7;

            scan.Parse(this._data[..dateExpectedLength], "%d\n");
            var date = (int)scan.Results.AsValueEnumerable().First().Value;
            if (date != verDate)
            {
#if false
                    return false;
#endif
            }
        }

        this._data = verDate != 0 ? this._data[8..this._dataSize] : this._data[..this._dataSize];

        streamReader.Close();

        this.DivideLine();
        this.RemoveComment();
        return true;
    }

    private void DivideLine()
    {
        this._lines?.Clear();
        this._lines = this._data.Split(new[]
                                       {
                                           "\r", "\n", "\r\n"
                                       }, StringSplitOptions.None).AsValueEnumerable().ToList();
    }

    private void RemoveComment()
    {
#if true
        var flag    = false;
        var comment = false;
        var count   = this._lines.Count;

        for (var r = 0; r < count; r++)
        {
            var line = this._lines[r];
            if (string.IsNullOrEmpty(line))
            {
                continue;
            }

#if DEBUG
            var quoteCount = line.AsValueEnumerable().Count(f => f == '"');
            if (quoteCount % 2 != 0)
            {
                throw new InvalidOperationException("Quote's missing in the script.");
            }
#endif

            var lineArray = line.ToCharArray();

            var lineLength     = line.Length;
            var characterIndex = 0;

            while (characterIndex < lineLength)
            {
                var pc = new string(lineArray, characterIndex, lineLength - characterIndex);

                if (comment)
                {
                    if (pc.StartsWith(BlockCommentEnd, StringComparison.OrdinalIgnoreCase))
                    {
                        comment                     = false;
                        lineArray[characterIndex++] = ' ';
                    }

                    lineArray[characterIndex] = ' ';
                }
                else
                {
                    if (flag)
                    {
                        if (pc.StartsWith(StringEnd, StringComparison.OrdinalIgnoreCase))
                        {
                            // Console.WriteLine($"set flag false line {r}");
                            flag = false;
                        }
                    }
                    else
                    {
                        if (pc.StartsWith(StringBegin, StringComparison.OrdinalIgnoreCase))
                        {
                            // Console.WriteLine($"set flag true line {r}");
                            flag = true;
                        }
                        else if (pc.StartsWith(BlockCommentBegin, StringComparison.OrdinalIgnoreCase))
                        {
                            comment                     = true;
                            lineArray[characterIndex++] = ' ';
                            lineArray[characterIndex]   = ' ';
                        }
                        else if (pc.StartsWith(LineComment, StringComparison.OrdinalIgnoreCase))
                        {
                            // Is a comment line, just skip.
                            break;
                        }
                        else if (lineArray[characterIndex] == '\n' ||
                                 lineArray[characterIndex] == '\r')
                        {
                            // if (characterIndex > 0)
                            //    characterIndex--;

                            // Line is finished.
                            break;
                        }
                    }
                }

                characterIndex++;
            }

            this._lines[r] = characterIndex > 0 ? new string(lineArray, 0, characterIndex) : string.Empty;
        }

#else
            bool flag = false;
            bool comment = false;
            int count = _lines.Count;

            for (int r = 0; r < count; r++)
            {
                if (flag == true)
                {
                    Console.WriteLine();
                }

                flag = false;
                string pc = _lines[r];
                int idx = 0;
                while (idx < pc.Length)
                {
                    var check = pc.Substring(idx);
                    if (comment)
                    {
                        if (check.IndexOf(BLOCKCOMMENT_END, StringComparison.Ordinal) > -1)
                        {
                            comment = false;
                            pc =
 pc.Substring(check.IndexOf(BLOCKCOMMENT_END, StringComparison.Ordinal) + BLOCKCOMMENT_END.Length);
                        }
                        else
                        {
                            pc = string.Empty;
                        }
                    }
                    else
                    {
                        if (flag)
                        {
                            if (check.StartsWith(STRING_END, StringComparison.Ordinal))
                                flag = false;
                        }
                        else
                        {
                            if (check.StartsWith(STRING_BEGIN, StringComparison.Ordinal))
                            {
                                flag = true;
                            }
                            else if (check.StartsWith(BLOCKCOMMENT_BEGIN, StringComparison.Ordinal))
                            {
                                comment = true;
                                pc = pc.Substring(0, idx);
                            }
                            else if (check.StartsWith(LINECOMMENT, StringComparison.Ordinal))
                            {
                                pc = pc.Substring(0, idx);
                                break;
                            }
                        }
                    }

                    idx++;
                }

                m_line[r] = pc;
            }
#endif
    }

    private void Clear()
    {
        // if (m_buf)
        // {
        // delete[] m_buf;
        // m_buf = NULL;
        // }

        this._lines?.Clear();
        this._dataSize = 0;
        this._comment  = false;
    }
}