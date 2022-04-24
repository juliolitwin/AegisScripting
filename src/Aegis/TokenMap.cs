using System.Collections.Generic;

namespace Scripting.Aegis;

public class TokenMap
{
    private readonly IDictionary<string, TokenInfo> _tokenMap = new Dictionary<string, TokenInfo>();

    public void Set(string name, TokenType type, int num = 0, string str = "")
    {
        if (!this._tokenMap.ContainsKey(name))
        {
            this._tokenMap[name] = new TokenInfo(type, num, str);
            return;
        }

        this._tokenMap[name].Set(type, num, str);
    }

    public void Set(string name, TokenInfo info)
    {
        this._tokenMap[name] = info;
    }

    public bool Get(string name, out TokenInfo pInfo)
    {
        pInfo = default(TokenInfo)!;
        if (!this._tokenMap.ContainsKey(name))
        {
            return false;
        }

        pInfo = this._tokenMap[name];
        return true;
    }
}