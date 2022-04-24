namespace Scripting.Aegis;

public abstract class ScriptHandler
{
    public TokenMap TokenMap { get; protected set; } = default!;

    public abstract bool OnControl(ref ScriptLine line, int cmd);

    public abstract bool OnCommand(ref ScriptLine line, Cmd cmd);

    public abstract bool OnFunc(ref ScriptLine line, int func, string parameter);

    public abstract bool OnVar(ref ScriptLine line, string var);

    public abstract void Error(string str, params object[] args);

    public void SetTokenMap(TokenMap tokenMap)
    {
        this.TokenMap = tokenMap;
    }

    public bool AnalyzeLine(string str)
    {
        ScriptLine line = new(str);

        line.Skip(" \t");
        if (!line.GetWord(out var word, " \t(=+-*/["))
        {
            return true;
        }

        if (!this.TokenMap.Get(word, out var tokenInfo))
        {
            this.Error("AnalyzeLine - [{0}] is not a tokenMap", word);
            return false;
        }

        if (tokenInfo.TokenType == TokenType.Command)
        {
            if (!this.OnControl(ref line, tokenInfo.ValueNumber))
            {
                return false;
            }
        }

        Parsing parse = new();
        if (!parse.Run(line.GetBase(), "$"))
        {
            this.Error("(,) error type AnalyzeLine 1");
            return false;
        }

        while (parse.Get(out var buf))
        {
            if (!this.AnalyzeParse(buf))
            {
                return false;
            }
        }

        return true;
    }

    protected bool AnalyzeParse(string str)
    {
        var line = new ScriptLine(str);

        line.Skip(" \t");
        if (!line.GetWord(out var word, " \t(=+-*/%["))
        {
            return true;
        }

        if (!this.TokenMap.Get(word, out var tokenInfo))
        {
            if (word[0] != '$')
            {
                this.Error("AnalyzeParse - 1:[{0}] is not in a tokenMap", word);
                return false;
            }

            this.TokenMap.Set(word, TokenType.Var);
            if (!this.TokenMap.Get(word, out tokenInfo))
            {
                this.Error("AnalyzeParse - 2:[{0}] is not in a tokenMap", word);
                return false;
            }
        }

        switch (tokenInfo.TokenType)
        {
            case TokenType.Command:
            {
                return this.OnCommand(ref line, (Cmd)tokenInfo.ValueNumber);
            }

            case TokenType.Func:
            {
                return this.OnFunc(ref line, tokenInfo.ValueNumber, tokenInfo.ValueString);
            }

            case TokenType.Var:
            {
                return this.OnVar(ref line, word);
            }
        }

        this.Error("[{1}] token type not found", word, tokenInfo.TokenType);
        return false;
    }
}