using Nito.Collections;

namespace Scripting.Aegis;

public abstract class InterpretHandler
{
    public VarMap VarMap { get; private set; }

    public void SetVarMap(VarMap varMap)
    {
        this.VarMap = varMap;
    }

    public abstract bool OnFunc(int func, Deque<TokenData> parameter, ref TokenData ret);

    public abstract void OnError(string str);
}