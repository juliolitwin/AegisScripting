using System;

namespace Scripting.Aegis;

public struct TokenData
{
    public static readonly TokenData Null = new();

    private const float CheckTolerance = 0.0001f;

    /// <summary>
    ///     Enumeration with current data types.
    /// </summary>
    public enum DataTypes
    {
        Num,
        Str,
        Float,
        Double
    }

    /// <summary>
    ///     Gets or sets string data value.
    /// </summary>
    public string String { get; set; }

    /// <summary>
    ///     Gets or sets number data value.
    /// </summary>
    public int Number { get; set; }

    /// <summary>
    ///     Gets or sets date type.
    /// </summary>
    public DataTypes Type { get; set; }

    /// <summary>
    ///     Initializes a new instance of the <see cref="TokenData" /> struct.
    ///     Constructor.
    /// </summary>
    public TokenData()
    {
        this.String = "";
        this.Number = 0;
        this.Type   = 0;
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="TokenData" /> struct.
    ///     Constructor.
    /// </summary>
    /// <param name="data"></param>
    public TokenData(TokenData data)
    {
        this.String = data.String;
        this.Number = data.Number;
        this.Type   = data.Type;
    }

    public static bool operator true(TokenData a)
    {
        return a.Type switch
               {
                   DataTypes.Str    => !string.IsNullOrEmpty(a.GetStr()),
                   DataTypes.Num    => a.GetNum()    != 0,
                   DataTypes.Float  => a.GetFloat()  != 0.0f,
                   DataTypes.Double => a.GetDouble() != 0.0f,
                   _                => false
               };
    }

    public static bool operator false(TokenData a)
    {
        return a.Type switch
               {
                   DataTypes.Str    => string.IsNullOrEmpty(a.GetStr()),
                   DataTypes.Num    => a.GetNum()    == 0,
                   DataTypes.Float  => a.GetFloat()  == 0.0f,
                   DataTypes.Double => a.GetDouble() == 0.0f,
                   _                => false
               };
    }

    public static bool operator &(TokenData a, TokenData b)
    {
        return a.Type switch
               {
                   DataTypes.Str    => !string.IsNullOrEmpty(a.GetStr()) && !string.IsNullOrEmpty(b.GetStr()),
                   DataTypes.Num    => a.GetNum()    != 0                && b.GetNum()    != 0,
                   DataTypes.Float  => a.GetFloat()  != 0.0f             && b.GetFloat()  != 0.0f,
                   DataTypes.Double => a.GetDouble() != 0.0f             && b.GetDouble() != 0.0f,
                   _                => false
               };
    }

    public static bool operator |(TokenData a, TokenData b)
    {
        return a.Type switch
               {
                   DataTypes.Str    => !string.IsNullOrEmpty(a.GetStr()) || !string.IsNullOrEmpty(b.GetStr()),
                   DataTypes.Num    => a.GetNum()    != 0                || b.GetNum()    != 0,
                   DataTypes.Float  => a.GetFloat()  != 0.0f             || b.GetFloat()  != 0.0f,
                   DataTypes.Double => a.GetDouble() != 0.0f             || b.GetDouble() != 0.0f,
                   _                => false
               };
    }

    public static bool operator !=(TokenData a, TokenData b)
    {
        return a.Type switch
               {
                   DataTypes.Str    => string.Compare(a.GetStr(), b.GetStr(), StringComparison.OrdinalIgnoreCase) != 0,
                   DataTypes.Num    => a.GetNum() != b.GetNum(),
                   DataTypes.Float  => Math.Abs(a.GetFloat()  - b.GetFloat()) > CheckTolerance,
                   DataTypes.Double => Math.Abs(a.GetDouble() - b.GetDouble()) > CheckTolerance,
                   _                => false
               };
    }

    public static bool operator ==(TokenData a, TokenData b)
    {
        return a.Type switch
               {
                   DataTypes.Str    => string.Compare(a.GetStr(), b.GetStr(), StringComparison.OrdinalIgnoreCase) == 0,
                   DataTypes.Num    => a.GetNum() == b.GetNum(),
                   DataTypes.Float  => Math.Abs(a.GetFloat()  - b.GetFloat()) < CheckTolerance,
                   DataTypes.Double => Math.Abs(a.GetDouble() - b.GetDouble()) < CheckTolerance,
                   _                => false
               };
    }

    public static bool operator >(TokenData a, TokenData b)
    {
        return a.Type switch
               {
                   DataTypes.Str    => string.Compare(a.GetStr(), b.GetStr(), StringComparison.OrdinalIgnoreCase) > 0,
                   DataTypes.Num    => a.GetNum() > b.GetNum(),
                   DataTypes.Float  => a.GetFloat() > b.GetFloat(),
                   DataTypes.Double => a.GetDouble() > b.GetDouble(),
                   _                => false
               };
    }

    public static bool operator <(TokenData a, TokenData b)
    {
        return a.Type switch
               {
                   DataTypes.Str    => string.Compare(a.GetStr(), b.GetStr(), StringComparison.OrdinalIgnoreCase) < 0,
                   DataTypes.Num    => a.GetNum() < b.GetNum(),
                   DataTypes.Float  => a.GetFloat() < b.GetFloat(),
                   DataTypes.Double => a.GetDouble() < b.GetDouble(),
                   _                => false
               };
    }

    public static bool operator >=(TokenData a, TokenData b)
    {
        return a.Type switch
               {
                   DataTypes.Str    => string.Compare(a.GetStr(), b.GetStr(), StringComparison.OrdinalIgnoreCase) >= 0,
                   DataTypes.Num    => a.GetNum() >= b.GetNum(),
                   DataTypes.Float  => a.GetFloat() >= b.GetFloat(),
                   DataTypes.Double => a.GetDouble() >= b.GetDouble(),
                   _                => false
               };
    }

    public static bool operator <=(TokenData a, TokenData b)
    {
        return a.Type switch
               {
                   DataTypes.Str    => string.Compare(a.GetStr(), b.GetStr(), StringComparison.OrdinalIgnoreCase) <= 0,
                   DataTypes.Num    => a.GetNum() <= b.GetNum(),
                   DataTypes.Float  => a.GetFloat() <= b.GetFloat(),
                   DataTypes.Double => a.GetDouble() <= b.GetDouble(),
                   _                => false
               };
    }

    public static TokenData operator %(TokenData a, TokenData b)
    {
        switch (a.Type)
        {
            case DataTypes.Str:
            {
                break;
            }

            case DataTypes.Num:
            {
                a.Number %= b.GetNum();
                break;
            }

            case DataTypes.Float:
            {
                break;
            }

            case DataTypes.Double:
            {
                break;
            }

            default:
            {
                throw new ArgumentOutOfRangeException();
            }
        }

        return a;
    }

    public static TokenData operator +(TokenData a, TokenData b)
    {
        switch (a.Type)
        {
            case DataTypes.Str:
            {
                a.String += b.GetStr();
                break;
            }

            case DataTypes.Num:
            {
                a.Number += b.GetNum();
                break;
            }

            case DataTypes.Float:
            {
                a.Set(a.GetFloat() + b.GetFloat());
                break;
            }

            case DataTypes.Double:
            {
                a.Set(a.GetDouble() + b.GetDouble());
                break;
            }

            default:
            {
                throw new ArgumentOutOfRangeException();
            }
        }

        return a;
    }

    public static TokenData operator -(TokenData a, TokenData b)
    {
        switch (a.Type)
        {
            case DataTypes.Str:
            {
                break;
            }

            case DataTypes.Num:
            {
                a.Number -= b.GetNum();
                break;
            }

            case DataTypes.Float:
            {
                a.Set(a.GetFloat() - b.GetFloat());
                break;
            }

            case DataTypes.Double:
            {
                a.Set(a.GetDouble() - b.GetDouble());
                break;
            }

            default:
            {
                throw new ArgumentOutOfRangeException();
            }
        }

        return a;
    }

    public static TokenData operator *(TokenData a, TokenData b)
    {
        switch (a.Type)
        {
            case DataTypes.Str:
            {
                break;
            }

            case DataTypes.Num:
            {
                a.Number *= b.GetNum();
                break;
            }

            case DataTypes.Float:
            {
                a.Set(a.GetFloat() * b.GetFloat());
                break;
            }

            case DataTypes.Double:
            {
                a.Set(a.GetDouble() * b.GetDouble());
                break;
            }

            default:
            {
                throw new ArgumentOutOfRangeException();
            }
        }

        return a;
    }

    public static TokenData operator /(TokenData a, TokenData b)
    {
        switch (a.Type)
        {
            case DataTypes.Str:
            {
                break;
            }

            case DataTypes.Num:
            {
                var i = b.GetNum();
                if (i != 0)
                {
                    a.Number /= i;
                }

                break;
            }

            case DataTypes.Float:
            {
                var f = b.GetFloat();
                if (f != 0F)
                {
                    a.Set(a.GetFloat() / f);
                }

                break;
            }

            case DataTypes.Double:
            {
                var d = b.GetDouble();
                if (d != 0)
                {
                    a.Set(a.GetDouble() / d);
                }

                break;
            }

            default:
            {
                throw new ArgumentOutOfRangeException();
            }
        }

        return a;
    }

    public static TokenData operator ++(TokenData a)
    {
        switch (a.Type)
        {
            case DataTypes.Str:
            {
                break;
            }

            case DataTypes.Num:
            {
                a.Number++;
                break;
            }

            case DataTypes.Float:
            {
                break;
            }

            case DataTypes.Double:
            {
                break;
            }

            default:
            {
                throw new ArgumentOutOfRangeException();
            }
        }

        return a;
    }

    public static TokenData operator --(TokenData a)
    {
        switch (a.Type)
        {
            case DataTypes.Str:
            {
                break;
            }

            case DataTypes.Num:
            {
                a.Number--;
                break;
            }

            case DataTypes.Float:
            {
                break;
            }

            case DataTypes.Double:
            {
                break;
            }

            default:
            {
                throw new ArgumentOutOfRangeException();
            }
        }

        return a;
    }

    public void SetType(DataTypes t)
    {
        if (this.Type == t)
        {
            return;
        }

        switch (t)
        {
            case DataTypes.Str:
            {
                this.String = this.GetStr();
                this.Type   = t;
                break;
            }

            case DataTypes.Num:
            {
                this.Number = this.GetNum();
                this.Type   = t;
                break;
            }

            case DataTypes.Float:
            {
                this.Set(this.GetFloat());
                this.Type = t;
                break;
            }

            case DataTypes.Double:
            {
                this.Set(this.GetDouble());
                this.Type = t;
                break;
            }

            default:
            {
                throw new ArgumentOutOfRangeException(nameof(t), t, null);
            }
        }
    }

    public void Set(string s)
    {
        this.String = string.IsNullOrEmpty(s) ? "" : s;
        this.Type   = DataTypes.Str;
    }

    public void Set(int n)
    {
        this.Number = n;
        this.Type   = DataTypes.Num;
    }

    public void Set(bool n)
    {
        this.Number = n ? 1 : 0;
        this.Type   = DataTypes.Num;
    }

    public void Set(long n)
    {
        this.Set((int)n);
    }

    public void Set(float f)
    {
        this.String = $"{f:f}";
        this.Type   = DataTypes.Num;
    }

    public void Set(double d)
    {
        this.String = $"{d:f}";
        this.Type   = DataTypes.Num;
    }

    public string GetStr()
    {
        if (this.Type == DataTypes.Num)
        {
            this.String = $"{this.Number:D}";
        }

        return this.String;
    }

    public int GetNum()
    {
        if (this.Type == DataTypes.Str)
        {
            if (string.IsNullOrEmpty(this.String))
            {
                return -1;
            }

            this.Number = Convert.ToInt32(this.String);
        }

        return this.Number;
    }

    public float GetFloat()
    {
        if (this.Type == DataTypes.Str)
        {
            return float.Parse(this.String);
        }

        return this.Number;
    }

    public double GetDouble()
    {
        if (this.Type == DataTypes.Str)
        {
            return double.Parse(this.String);
        }

        return this.Number;
    }

    public bool IsNumber()
    {
        return this.Type == DataTypes.Num;
    }

    public bool IsString()
    {
        return this.Type == DataTypes.Str;
    }

    public bool IsFloat()
    {
        return this.Type == DataTypes.Float;
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"String = {this.String} | Number = {this.Number}";
    }
}