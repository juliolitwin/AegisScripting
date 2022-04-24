namespace Scripting.Aegis;

public class TokenInfo
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="TokenInfo" /> class.
    ///     Constructor.
    /// </summary>
    /// <param name="tokenType"></param>
    /// <param name="valueNumber"></param>
    /// <param name="valueString"></param>
    public TokenInfo(TokenType tokenType, int valueNumber, string valueString)
    {
        this.TokenType   = tokenType;
        this.ValueNumber = valueNumber;
        this.ValueString = valueString;
    }

    /// <summary>
    ///     Gets or sets the Token Type.
    /// </summary>
    public TokenType TokenType { get; set; }

    /// <summary>
    ///     Gets or sets the Value Number.
    /// </summary>
    public int ValueNumber { get; set; }

    /// <summary>
    ///     Gets or sets the Value String.
    /// </summary>
    public string ValueString { get; set; }

    /// <summary>
    ///     Set the token type.
    /// </summary>
    /// <param name="t"></param>
    /// <param name="n"></param>
    /// <param name="s"></param>
    public void Set(TokenType t, int n, string s)
    {
        this.TokenType   = t;
        this.ValueNumber = n;
        this.ValueString = s;
    }

    public string GetStr()
    {
        return this.ValueString;
    }
}