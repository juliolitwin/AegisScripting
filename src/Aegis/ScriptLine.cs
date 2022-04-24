using Scripting.Utilities;

namespace Scripting.Aegis;

public struct ScriptLine
{
    /// <summary>
    ///     Line of the current script.
    /// </summary>
    private readonly string _line = "";

    /// <summary>
    ///     Current text of the line.
    /// </summary>
    private string _current = "";

    /// <summary>
    ///     Initializes a new instance of the <see cref="ScriptLine" /> struct.
    ///     Constructor.
    /// </summary>
    /// <param name="str"></param>
    public ScriptLine(string str)
    {
        this._line    = str;
        this._current = str;
    }

    public void Skip(string v)
    {
        var index = 0;

        while (index < this._current.Length)
        {
            if (v.Contains(this._current[index]))
            {
                index++;
                continue;
            }

            break;
        }

        // Skip by the index.
        this._current = this._current[index..];
    }

    public string GetCurrent()
    {
        return this._current;
    }

    public string GetBase()
    {
        return this._line;
    }

    internal bool GetWord(out string word, string v)
    {
        word = "";

        if (string.IsNullOrEmpty(this._current))
        {
            return false;
        }

        word = this._current.IndexOfAny(v.ToCharArray()) == -1
                   ? this._current
                   : this._current[..this._current.IndexOfAny(v.ToCharArray())];
        this._current = this._current[word.Length..];
        return true;
    }

    internal bool GetParse(out string data, char div)
    {
        data = "";

        if (this._current.Length == 0)
        {
            return false;
        }

        if (this._current[0] != div)
        {
            return false;
        }

        var idx = 1;
        while (true)
        {
            if (this._current.Length <= idx)
            {
                return false;
            }

            if (this._current[idx] == div)
            {
                idx++;
                break;
            }

            idx++;
        }

        data          = 1 == idx ? string.Empty : this._current.Substring(1, idx - 2);
        this._current = this._current[(data.Length + 2)..];
        return true;
    }

    public bool GetOperator(out string op, string v)
    {
        op = "";

        int i;
        for (i = 0; i < this._current.Length;)
        {
            var c = this._current[i];

            if (string.IsNullOrEmpty(StringFunctions.StrChr(v, c)))
            {
                break;
            }

            i += (c & 0x80) != 0 ? 2 : 1;
        }

        op            = this._current[..i];
        this._current = this._current[i..];

        return !string.IsNullOrEmpty(op);
    }
}