using System;
using System.Collections.Generic;
using System.Linq;

namespace Scripting.Utilities;

#nullable enable


/// <summary>
///     Class that provides functionality of the standard C library sscanf()
///     function.
/// </summary>
public class ScanFormatted
{
    // Holds results after calling Parse()
    public List<object> Results;

    // Lookup table to find parser by parser type
    protected TypeParser[] TypeParsers;

    // Constructor
    /// <summary>
    ///     Initializes a new instance of the <see cref="ScanFormatted" /> class.
    /// </summary>
    public ScanFormatted()
    {
        // Populate parser type lookup table
        this.TypeParsers = new[]
                           {
                               new()
                               {
                                   Type = Types.Character, Parser = this.ParseCharacter
                               },
                               new TypeParser
                               {
                                   Type = Types.Decimal, Parser = this.ParseDecimal
                               },
                               new TypeParser
                               {
                                   Type = Types.Float, Parser = this.ParseFloat
                               },
                               new TypeParser
                               {
                                   Type = Types.Hexadecimal, Parser = this.ParseHexadecimal
                               },
                               new TypeParser
                               {
                                   Type = Types.Octal, Parser = this.ParseOctal
                               },
                               new TypeParser
                               {
                                   Type = Types.ScanSet, Parser = this.ParseScanSet
                               },
                               new TypeParser
                               {
                                   Type = Types.String, Parser = this.ParseString
                               },
                               new TypeParser
                               {
                                   Type = Types.Unsigned, Parser = this.ParseDecimal
                               }
                           };

        // Allocate results collection
        this.Results = new List<object>();
    }

    /// <summary>
    ///     Parses the input string according to the rules in the
    ///     format string. Similar to the standard C library's
    ///     sscanf() function. Parsed fields are placed in the
    ///     class' Results member.
    /// </summary>
    /// <param name="input">String to parse</param>
    /// <param name="format">Specifies rules for parsing input</param>
    /// <returns></returns>
    public int Parse(string input, string format)
    {
        var inp     = new TextParser(input);
        var fmt     = new TextParser(format);
        var results = new List<object>();
        var spec    = new FormatSpecifier();
        var count   = 0;

        // Clear any previous results
        this.Results.Clear();

        // Process input string as indicated in format string
        while (!fmt.EndOfText &&
               !inp.EndOfText)
        {
            if (this.ParseFormatSpecifier(fmt, spec))
            {
                // Found a format specifier
                var parser = this.TypeParsers.First(tp => tp.Type == spec.Type);
                if (parser.Parser != null &&
                    parser.Parser(inp, spec))
                {
                    count++;
                }
                else
                {
                    break;
                }
            }
            else if (char.IsWhiteSpace(fmt.Peek()))
            {
                // Whitespace
                inp.MovePastWhitespace();
                fmt.MoveAhead();
            }
            else if (fmt.Peek() == inp.Peek())
            {
                // Matching character
                inp.MoveAhead();
                fmt.MoveAhead();
            }
            else
            {
                break; // Break at mismatch
            }
        }

        // Return number of fields successfully parsed
        return count;
    }

    /// <summary>
    ///     Attempts to parse a field format specifier from the format string.
    /// </summary>
    /// <returns></returns>
    protected bool ParseFormatSpecifier(TextParser format, FormatSpecifier spec)
    {
        // Return if not a field format specifier
        if (format.Peek() != '%')
        {
            return false;
        }

        format.MoveAhead();

        // Return if "%%" (treat as '%' literal)
        if (format.Peek() == '%')
        {
            return false;
        }

        // Test for asterisk, which indicates result is not stored
        if (format.Peek() == '*')
        {
            spec.NoResult = true;
            format.MoveAhead();
        }
        else
        {
            spec.NoResult = false;
        }

        // Parse width
        var start = format.Position;
        while (char.IsDigit(format.Peek()))
        {
            format.MoveAhead();
        }

        if (format.Position > start)
        {
            spec.Width = int.Parse(format.Extract(start, format.Position));
        }
        else
        {
            spec.Width = 0;
        }

        // Parse modifier
        if (format.Peek() == 'h')
        {
            format.MoveAhead();
            if (format.Peek() == 'h')
            {
                format.MoveAhead();
                spec.Modifier = Modifiers.ShortShort;
            }
            else
            {
                spec.Modifier = Modifiers.Short;
            }
        }
        else if (char.ToLower(format.Peek()) == 'l')
        {
            format.MoveAhead();
            if (format.Peek() == 'l')
            {
                format.MoveAhead();
                spec.Modifier = Modifiers.LongLong;
            }
            else
            {
                spec.Modifier = Modifiers.Long;
            }
        }
        else
        {
            spec.Modifier = Modifiers.None;
        }

        // Parse type
        switch (format.Peek())
        {
            case 'c':
            {
                spec.Type = Types.Character;
                break;
            }

            case 'd':
            case 'i':
            {
                spec.Type = Types.Decimal;
                break;
            }

            case 'a':
            case 'A':
            case 'e':
            case 'E':
            case 'f':
            case 'F':
            case 'g':
            case 'G':
            {
                spec.Type = Types.Float;
                break;
            }

            case 'o':
            {
                spec.Type = Types.Octal;
                break;
            }

            case 's':
            {
                spec.Type = Types.String;
                break;
            }

            case 'u':
            {
                spec.Type = Types.Unsigned;
                break;
            }

            case 'x':
            case 'X':
            {
                spec.Type = Types.Hexadecimal;
                break;
            }

            case '[':
            {
                spec.Type = Types.ScanSet;
                format.MoveAhead();

                // Parse scan set characters
                if (format.Peek() == '^')
                {
                    spec.ScanSetExclude = true;
                    format.MoveAhead();
                }
                else
                {
                    spec.ScanSetExclude = false;
                }

                start = format.Position;

                // Treat immediate ']' as literal
                if (format.Peek() == ']')
                {
                    format.MoveAhead();
                }

                format.MoveTo(']');
                if (format.EndOfText)
                {
                    throw new Exception("Type specifier expected character : ']'");
                }

                spec.ScanSet = format.Extract(start, format.Position);
                break;
            }

            default:
            {
                var msg = string.Format("Unknown format type specified : '{0}'", format.Peek());
                throw new Exception(msg);
            }
        }

        format.MoveAhead();
        return true;
    }

    /// <summary>
    ///     Parse a character field
    /// </summary>
    private bool ParseCharacter(TextParser input, FormatSpecifier spec)
    {
        // Parse character(s)
        var start = input.Position;
        var count = spec.Width > 1 ? spec.Width : 1;
        while (!input.EndOfText &&
               count-- > 0)
        {
            input.MoveAhead();
        }

        // Extract token
        if (count          <= 0 &&
            input.Position > start)
        {
            if (!spec.NoResult)
            {
                var token = input.Extract(start, input.Position);
                if (token.Length > 1)
                {
                    this.Results.Add(token.ToCharArray());
                }
                else
                {
                    this.Results.Add(token[0]);
                }
            }

            return true;
        }

        return false;
    }

    /// <summary>
    ///     Parse integer field
    /// </summary>
    private bool ParseDecimal(TextParser input, FormatSpecifier spec)
    {
        var radix = 10;

        // Skip any whitespace
        input.MovePastWhitespace();

        // Parse leading sign
        var start = input.Position;
        if (input.Peek() == '+' ||
            input.Peek() == '-')
        {
            input.MoveAhead();
        }
        else if (input.Peek() == '0')
        {
            if (char.ToLower(input.Peek(1)) == 'x')
            {
                radix = 16;
                input.MoveAhead(2);
            }
            else
            {
                radix = 8;
                input.MoveAhead();
            }
        }

        // Parse digits
        while (this.IsValidDigit(input.Peek(), radix))
        {
            input.MoveAhead();
        }

        // Don't exceed field width
        if (spec.Width > 0)
        {
            var count = input.Position - start;
            if (spec.Width < count)
            {
                input.MoveAhead(spec.Width - count);
            }
        }

        // Extract token
        if (input.Position > start)
        {
            if (!spec.NoResult)
            {
                if (spec.Type == Types.Decimal)
                {
                    this.AddSigned(input.Extract(start, input.Position), spec.Modifier, radix);
                }
                else
                {
                    this.AddUnsigned(input.Extract(start, input.Position), spec.Modifier, radix);
                }
            }

            return true;
        }

        return false;
    }

    /// <summary>
    ///     Parse a floating-point field
    /// </summary>
    private bool ParseFloat(TextParser input, FormatSpecifier spec)
    {
        // Skip any whitespace
        input.MovePastWhitespace();

        // Parse leading sign
        var start = input.Position;
        if (input.Peek() == '+' ||
            input.Peek() == '-')
        {
            input.MoveAhead();
        }

        // Parse digits
        var hasPoint = false;
        while (char.IsDigit(input.Peek()) ||
               input.Peek() == '.')
        {
            if (input.Peek() == '.')
            {
                if (hasPoint)
                {
                    break;
                }

                hasPoint = true;
            }

            input.MoveAhead();
        }

        // Parse exponential notation
        if (char.ToLower(input.Peek()) == 'e')
        {
            input.MoveAhead();
            if (input.Peek() == '+' ||
                input.Peek() == '-')
            {
                input.MoveAhead();
            }

            while (char.IsDigit(input.Peek()))
            {
                input.MoveAhead();
            }
        }

        // Don't exceed field width
        if (spec.Width > 0)
        {
            var count = input.Position - start;
            if (spec.Width < count)
            {
                input.MoveAhead(spec.Width - count);
            }
        }

        // Because we parse the exponential notation before we apply
        // any field-width constraint, it becomes awkward to verify
        // we have a valid floating point token. To prevent an
        // exception, we use TryParse() here instead of Parse().
        double result;

        // Extract token
        if (input.Position > start &&
            double.TryParse(input.Extract(start, input.Position), out result))
        {
            if (!spec.NoResult)
            {
                if (spec.Modifier == Modifiers.Long ||
                    spec.Modifier == Modifiers.LongLong)
                {
                    this.Results.Add(result);
                }
                else
                {
                    this.Results.Add((float)result);
                }
            }

            return true;
        }

        return false;
    }

    /// <summary>
    ///     Parse hexadecimal field
    /// </summary>
    /// <returns></returns>
    protected bool ParseHexadecimal(TextParser input, FormatSpecifier spec)
    {
        // Skip any whitespace
        input.MovePastWhitespace();

        // Parse 0x prefix
        var start = input.Position;
        if (input.Peek()  == '0' &&
            input.Peek(1) == 'x')
        {
            input.MoveAhead(2);
        }

        // Parse digits
        while (this.IsValidDigit(input.Peek(), 16))
        {
            input.MoveAhead();
        }

        // Don't exceed field width
        if (spec.Width > 0)
        {
            var count = input.Position - start;
            if (spec.Width < count)
            {
                input.MoveAhead(spec.Width - count);
            }
        }

        // Extract token
        if (input.Position > start)
        {
            if (!spec.NoResult)
            {
                this.AddUnsigned(input.Extract(start, input.Position), spec.Modifier, 16);
            }

            return true;
        }

        return false;
    }

    /// <summary>
    ///     Parse an octal field
    /// </summary>
    private bool ParseOctal(TextParser input, FormatSpecifier spec)
    {
        // Skip any whitespace
        input.MovePastWhitespace();

        // Parse digits
        var start = input.Position;
        while (this.IsValidDigit(input.Peek(), 8))
        {
            input.MoveAhead();
        }

        // Don't exceed field width
        if (spec.Width > 0)
        {
            var count = input.Position - start;
            if (spec.Width < count)
            {
                input.MoveAhead(spec.Width - count);
            }
        }

        // Extract token
        if (input.Position > start)
        {
            if (!spec.NoResult)
            {
                this.AddUnsigned(input.Extract(start, input.Position), spec.Modifier, 8);
            }

            return true;
        }

        return false;
    }

    /// <summary>
    ///     Parse a scan-set field
    /// </summary>
    /// <returns></returns>
    protected bool ParseScanSet(TextParser input, FormatSpecifier spec)
    {
        // Parse characters
        var start = input.Position;
        if (!spec.ScanSetExclude)
        {
            while (spec.ScanSet != null &&
                   spec.ScanSet.Contains(input.Peek()))
            {
                input.MoveAhead();
            }
        }
        else
        {
            while (spec.ScanSet != null &&
                   !input.EndOfText     &&
                   !spec.ScanSet.Contains(input.Peek()))
            {
                input.MoveAhead();
            }
        }

        // Don't exceed field width
        if (spec.Width > 0)
        {
            var count = input.Position - start;
            if (spec.Width < count)
            {
                input.MoveAhead(spec.Width - count);
            }
        }

        // Extract token
        if (input.Position > start)
        {
            if (!spec.NoResult)
            {
                this.Results.Add(input.Extract(start, input.Position));
            }

            return true;
        }

        return false;
    }

    /// <summary>
    ///     Parse a string field
    /// </summary>
    private bool ParseString(TextParser input, FormatSpecifier spec)
    {
        // Skip any whitespace
        input.MovePastWhitespace();

        // Parse string characters
        var start = input.Position;
        while (!input.EndOfText &&
               !char.IsWhiteSpace(input.Peek()))
        {
            input.MoveAhead();
        }

        // Don't exceed field width
        if (spec.Width > 0)
        {
            var count = input.Position - start;
            if (spec.Width < count)
            {
                input.MoveAhead(spec.Width - count);
            }
        }

        // Extract token
        if (input.Position > start)
        {
            if (!spec.NoResult)
            {
                this.Results.Add(input.Extract(start, input.Position));
            }

            return true;
        }

        return false;
    }

    // Determines if the given digit is valid for the given radix
    private bool IsValidDigit(char c, int radix)
    {
        var i = "0123456789abcdef".IndexOf(char.ToLower(c));
        if (i >= 0 &&
            i < radix)
        {
            return true;
        }

        return false;
    }

    // Parse signed token and add to results
    private void AddSigned(string token, Modifiers mod, int radix)
    {
        object obj;
        if (mod == Modifiers.ShortShort)
        {
            obj = Convert.ToSByte(token, radix);
        }
        else if (mod == Modifiers.Short)
        {
            obj = Convert.ToInt16(token, radix);
        }
        else if (mod == Modifiers.Long ||
                 mod == Modifiers.LongLong)
        {
            obj = Convert.ToInt64(token, radix);
        }
        else
        {
            obj = Convert.ToInt32(token, radix);
        }

        this.Results.Add(obj);
    }

    // Parse unsigned token and add to results
    private void AddUnsigned(string token, Modifiers mod, int radix)
    {
        object obj;
        if (mod == Modifiers.ShortShort)
        {
            obj = Convert.ToByte(token, radix);
        }
        else if (mod == Modifiers.Short)
        {
            obj = Convert.ToUInt16(token, radix);
        }
        else if (mod == Modifiers.Long ||
                 mod == Modifiers.LongLong)
        {
            obj = Convert.ToUInt64(token, radix);
        }
        else
        {
            obj = Convert.ToUInt32(token, radix);
        }

        this.Results.Add(obj);
    }

    // Format type specifiers
    protected enum Types
    {
        Character,
        Decimal,
        Float,
        Hexadecimal,
        Octal,
        ScanSet,
        String,
        Unsigned
    }

    // Format modifiers
    protected enum Modifiers
    {
        None,
        ShortShort,
        Short,
        Long,
        LongLong
    }

    // Delegate to parse a type
    protected delegate bool ParseValue(TextParser input, FormatSpecifier spec);

    // Class to associate format type with type parser
    protected class TypeParser
    {
        public Types Type { get; set; }

        public ParseValue? Parser { get; set; }
    }

    // Class to hold format specifier information
    protected class FormatSpecifier
    {
        public Types Type { get; set; }

        public Modifiers Modifier { get; set; }

        public int Width { get; set; }

        public bool NoResult { get; set; }

        public string? ScanSet { get; set; }

        public bool ScanSetExclude { get; set; }
    }
}