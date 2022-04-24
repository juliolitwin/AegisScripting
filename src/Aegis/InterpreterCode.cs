using System;
using Nito.Collections;
using Scripting.Utilities;

namespace Scripting.Aegis;

public partial class Interpreter
{
    public bool CodeMov()
    {
        var data = new TokenData();

        if (!this.ReadVar(out var var))
        {
            this.Error("CodeMov : var error");
            return false;
        }

        if (!this.ReadValue(ref data))
        {
            this.Error("CodeMov : value error");
            return false;
        }

        if (this._scan)
        {
            return true;
        }

        this.VarMap.Map[var] = data;
        return true;
    }

    public bool CodeAdd()
    {
        var data = new TokenData();

        if (!this.ReadVar(out var var))
        {
            this.Error("CodeAdd : var error");
            return false;
        }

        if (!this.ReadValue(ref data))
        {
            this.Error("CodeAdd : value error");
            return false;
        }

        if (this._scan)
        {
            return true;
        }

        this.VarMap.Map[var] += data;
        return true;
    }

    public bool CodeSub()
    {
        var data = new TokenData();

        if (!this.ReadVar(out var var))
        {
            this.Error("CodeSub : var error");
            return false;
        }

        if (!this.ReadValue(ref data))
        {
            this.Error("CodeSub : value error");
            return false;
        }

        if (this._scan)
        {
            return true;
        }

        this.VarMap.Map[var] -= data;
        return true;
    }

    public bool CodeMul()
    {
        var data = new TokenData();

        if (!this.ReadVar(out var var))
        {
            this.Error("CodeMul : var error");
            return false;
        }

        if (!this.ReadValue(ref data))
        {
            this.Error("CodeMul : value error");
            return false;
        }

        if (this._scan)
        {
            return true;
        }

        this.VarMap.Map[var] *= data;
        return true;
    }

    public bool CodeMod()
    {
        var data = new TokenData();

        if (!this.ReadVar(out var var))
        {
            this.Error("CodeMod : var error");
            return false;
        }

        if (!this.ReadValue(ref data))
        {
            this.Error("CodeMod : value error");
            return false;
        }

        if (this._scan)
        {
            return true;
        }

        this.VarMap.Map[var] %= data;
        return true;
    }

    public bool CodeDiv()
    {
        var data = new TokenData();
        if (!this.ReadVar(out var var))
        {
            this.Error("CodeDiv : var error");
            return false;
        }

        if (!this.ReadValue(ref data))
        {
            this.Error("CodeDiv : value error");
            return false;
        }

        if (this._scan)
        {
            return true;
        }

        this.VarMap.Map[var] /= data;
        return true;
    }

    public bool CodeInc()
    {
        if (!this.ReadVar(out var var))
        {
            this.Error("CodeInc : var error");
            return false;
        }

        if (this._scan)
        {
            return true;
        }

        this.VarMap.Map[var]++;
        return true;
    }

    public bool CodeDec()
    {
        if (!this.ReadVar(out var var))
        {
            this.Error("CodeDec : var error");
            return false;
        }

        if (this._scan)
        {
            return true;
        }

        this.VarMap.Map[var]--;
        return true;
    }

    public bool CodeCmp()
    {
        var data = new TokenData();
        if (!this.ReadValue(ref data))
        {
            this.Error("CodeCmp : ReadValue");
            return false;
        }

        if (!this.ReadNum(out var address))
        {
            this.Error("CodeCmp : not goto ");
            return false;
        }

        if (this._scan)
        {
            return true;
        }

        if (data.GetNum() == 0) // Only when the comparison value is false. 
        {
            if (!this.Goto(address))
            {
                this.Error("CodeCmp : goto addr: hex[{0:x}] dec[{1}] ", address, address);
                return false;
            }
        }

        return true;
    }

    public bool CodePush()
    {
        if (!this.ReadVar(out var var))
        {
            this.Error("CodePop : var error");
            return false;
        }

        if (this._scan)
        {
            return true;
        }

        var data = new TokenData();
        if (!this.VarMap.Get(var, ref data))
        {
            data = TokenData.Null;
        }

        this._tokenDataStack.Push(data);
        return true;
    }

    public bool CodePop()
    {
        if (!this.ReadVar(out var var))
        {
            this.Error("CodePop : var error");
            return false;
        }

        if (this._scan)
        {
            return true;
        }

        if (this._tokenDataStack.Count == 0)
        {
            return false;
        }

        this.VarMap.Set(var, this._tokenDataStack.Peek());
        this._tokenDataStack.Pop();
        return true;
    }

    public bool CodeCase()
    {
        var data = new TokenData();
        if (!this.ReadValue(ref data))
        {
            this.Error("CodeCase : ReadValue");
            return false;
        }

        if (!this.ReadNum(out var addr))
        {
            this.Error("CodeCmp : not goto ");
            return false;
        }

        if (this._scan)
        {
            return true;
        }

        if (this.VarMap.Map["$case"] != data)
        {
            if (!this.Goto(addr))
            {
                this.Error("CodeCase : goto addr: hex[{0:x}] dec[{1}] error", addr, addr);
                return false;
            }
        }

        return true;
    }

    public bool CodeGoto()
    {
        if (!this.ReadNum(out var addr))
        {
            this.Error("CodeGoto : ReadNum(addr)");
            return false;
        }

        if (this._scan)
        {
            return true;
        }

        if (!this.Goto(addr))
        {
            this.Error("CodeGoto : goto addr: hex[{0:x}] dec[{1}] !Goto(addr)", addr, addr);
            return false;
        }

        return true;
    }

    public bool Goto(long pos)
    {
        if (this._gotoLock)
        {
            return true;
        }

        if (pos < 0)
        {
            this.Error("Goto : pos < 0 {0}", pos);
            return false;
        }

        if (pos > this._size)
        {
            this.Error("Goto : pos > m_size {0}", pos);
            return false;
        }

        this._pos = pos;
        return true;
    }

    public bool CodeFunc(ref TokenData ret)
    {
        if (!this.GetWord(out var func))
        {
            this.Error("CodeFunc : GetWordError1");
            return false;
        }

        Deque<TokenData> parameters = new();

        while (true)
        {
            TokenData data = new();

            if (!this.GetByte(out var type))
            {
                this.Error("CodeFunc : GetByteError1");
                return false;
            }

            if (type == ';')
            {
                break;
            }

            if (string.IsNullOrEmpty(StringFunctions.StrChr("snfv?", Convert.ToChar(type))))
            {
                this.Error("CodeFunc: {0} !strchr(\"snfv?\", type)", type);
                return false;
            }

            if (!this.ReadValue(ref data))
            {
                this.Error("CodeFunc: ReadValue(data)");
                return false;
            }

            switch (Convert.ToChar(type))
            {
                case 's':
                {
                    data.SetType(TokenData.DataTypes.Str);
                    break;
                }

                case 'n':
                {
                    data.SetType(TokenData.DataTypes.Num);
                    break;
                }

                case 'f':
                {
                    break;
                }
            }

            parameters.AddToBack(data);
        }

        return this._handler.OnFunc(func, parameters, ref ret);
    }
}