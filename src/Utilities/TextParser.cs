using System;
using NetFabric.Hyperlinq;

namespace Scripting.Utilities;

public class TextParser
{
    public static char NullChar = (char)0;

    /// <summary>
    ///     Initializes a new instance of the <see cref="TextParser" /> class.
    /// </summary>
    public TextParser()
    {
        this.Reset("");
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="TextParser" /> class.
    /// </summary>
    /// <param name="text"></param>
    public TextParser(string text)
    {
        this.Reset(text);
    }

    public string Text { get; private set; } = "";

    public int Position { get; private set; }

    public int Remaining => this.Text.Length - this.Position;

    /// <summary>
    ///     Gets a value indicating whether indicates if the current position is at the end of the current document
    /// </summary>
    public bool EndOfText => this.Position >= this.Text.Length;

    /// <summary>
    ///     Resets the current position to the start of the current document
    /// </summary>
    public void Reset()
    {
        this.Position = 0;
    }

    /// <summary>
    ///     Sets the current document and resets the current position to the start of it
    /// </summary>
    /// <param name="text"></param>
    public void Reset(string text)
    {
        this.Text     = !string.IsNullOrEmpty(text) ? text : string.Empty;
        this.Position = 0;
    }

    /// <summary>
    ///     Returns the character at the current position, or a null character if we're
    ///     at the end of the document
    /// </summary>
    /// <returns>The character at the current position</returns>
    public char Peek()
    {
        return this.Peek(0);
    }

    /// <summary>
    ///     Returns the character at the specified number of characters beyond the current
    ///     position, or a null character if the specified position is at the end of the
    ///     document
    /// </summary>
    /// <param name="ahead">The number of characters beyond the current position</param>
    /// <returns>The character at the specified position</returns>
    public char Peek(int ahead)
    {
        var pos = this.Position + ahead;
        if (pos < this.Text.Length)
        {
            return this.Text[pos];
        }

        return NullChar;
    }

    /// <summary>
    ///     Extracts a substring from the specified position to the end of the text
    /// </summary>
    /// <param name="start"></param>
    /// <returns></returns>
    public string Extract(int start)
    {
        return this.Extract(start, this.Text.Length);
    }

    /// <summary>
    ///     Extracts a substring from the specified range of the current text
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <returns></returns>
    public string Extract(int start, int end)
    {
        return this.Text.Substring(start, end - start);
    }

    /// <summary>
    ///     Moves the current position ahead one character
    /// </summary>
    public void MoveAhead()
    {
        this.MoveAhead(1);
    }

    /// <summary>
    ///     Moves the current position ahead the specified number of characters
    /// </summary>
    /// <param name="ahead">The number of characters to move ahead</param>
    public void MoveAhead(int ahead)
    {
        this.Position = Math.Min(this.Position + ahead, this.Text.Length);
    }

    /// <summary>
    ///     Moves to the next occurrence of the specified string
    /// </summary>
    /// <param name="s">String to find</param>
    /// <param name="ignoreCase">
    ///     Indicates if case-insensitive comparisons
    ///     are used
    /// </param>
    public void MoveTo(string s, bool ignoreCase = false)
    {
        this.Position = this.Text.IndexOf(s, this.Position,
                                          ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);
        if (this.Position < 0)
        {
            this.Position = this.Text.Length;
        }
    }

    /// <summary>
    ///     Moves to the next occurrence of the specified character
    /// </summary>
    /// <param name="c">Character to find</param>
    public void MoveTo(char c)
    {
        this.Position = this.Text.IndexOf(c, this.Position);
        if (this.Position < 0)
        {
            this.Position = this.Text.Length;
        }
    }

    /// <summary>
    ///     Moves to the next occurrence of any one of the specified
    ///     characters
    /// </summary>
    /// <param name="chars">Array of characters to find</param>
    public void MoveTo(char[] chars)
    {
        this.Position = this.Text.IndexOfAny(chars, this.Position);
        if (this.Position < 0)
        {
            this.Position = this.Text.Length;
        }
    }

    /// <summary>
    ///     Moves to the next occurrence of any character that is not one
    ///     of the specified characters
    /// </summary>
    /// <param name="chars">Array of characters to move past</param>
    public void MovePast(char[] chars)
    {
        while (this.IsInArray(this.Peek(), chars))
        {
            this.MoveAhead();
        }
    }

    /// <summary>
    ///     Determines if the specified character exists in the specified
    ///     character array.
    /// </summary>
    /// <param name="c">Character to find</param>
    /// <param name="chars">Character array to search</param>
    /// <returns></returns>
    protected bool IsInArray(char c, char[] chars)
    {
        return chars.AsValueEnumerable().Any(ch => c == ch);
    }

    /// <summary>
    ///     Moves the current position to the first character that is part of a newline
    /// </summary>
    public void MoveToEndOfLine()
    {
        var c = this.Peek();
        while (c != '\r' &&
               c != '\n' &&
               !this.EndOfText)
        {
            this.MoveAhead();
            c = this.Peek();
        }
    }

    /// <summary>
    ///     Moves the current position to the next character that is not whitespace
    /// </summary>
    public void MovePastWhitespace()
    {
        while (char.IsWhiteSpace(this.Peek()))
        {
            this.MoveAhead();
        }
    }
}