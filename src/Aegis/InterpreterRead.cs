using System;
using Nito.Collections;

namespace Scripting.Aegis;

public partial class Interpreter
{
    public bool CheckType(char c)
    {
        if (!this.GetByte(out var chk))
        {
            this.Error("CheckType: !GetByte");
            return false;
        }

        if ((char)chk != c)
        {
            this.Error("CheckType: chk != c[{0}][{1}] != chk != c [{2}][{3}]", c, c, chk, chk);
            return false;
        }

        return true;
    }

    public bool ReadCode(out int c)
    {
        c = 0;
        if (!this.CheckType('c'))
        {
            this.Error("ReadCode:!CheckType");
            return false;
        }

        if (!this.GetByte(out var code))
        {
            this.Error("ReadCode: !GetByte");
            return false;
        }

        c = code;
        return true;
    }

    public bool ReadNum(out long num)
    {
        num = 0;

        if (!this.CheckType('n'))
        {
            this.Error("ReadNum: !CheckType");
            return false;
        }

        if (!this.GetNum(out num))
        {
            this.Error("ReadNum: !GetNum");
            return false;
        }

        return true;
    }

    public bool ReadVar(out string var)
    {
        var = "";

        if (!this.CheckType('v'))
        {
            this.Error("ReadVar: !CheckType");
            return false;
        }

        if (!this.GetStr(out var))
        {
            this.Error("ReadVar: !GetStr");
            return false;
        }

        return true;
    }

    public bool ReadStr(out string str)
    {
        str = "";

        if (!this.CheckType('s'))
        {
            this.Error("ReadStr: CheckType");
            return false;
        }

        if (!this.GetStr(out str))
        {
            this.Error("ReadStr: !GetStr(str))");
            return false;
        }

        return true;
    }

    public bool ReadValue(ref TokenData ret)
    {
        Deque<TokenData> dataQ = new();
        Deque<byte>      opQ   = new();

        TokenData data = new();

        while (true)
        {
            if (!this.PeekByte(out var type))
            {
                this.Error("ReadValue: !PeekByte(type)");
                return false;
            }

            var str = "";
            switch (Convert.ToChar(type))
            {
                case 's':
                {
                    if (!this.ReadStr(out str))
                    {
                        this.Error("ReadValue: !ReadStr(str)");
                        return false;
                    }

                    data.Set(str);
                    break;
                }

                case 'n':
                {
                    if (!this.ReadNum(out var num))
                    {
                        this.Error("ReadValue: !ReadNum(num)");
                        return false;
                    }

                    data.Set(num);
                    break;
                }

                case 'v':
                {
                    if (!this.ReadVar(out str))
                    {
                        this.Error("ReadValue: !ReadVar(str)");
                        return false;
                    }

                    if (!this.VarMap.Get(str, ref data))
                    {
#if false
                                if (str[0] == '$')
                                {
                                    data = TokenData.Null;
                                }
                                else
                                {
                                    Error("ReadValue: variable that doesn't exist [{0}]", str);
                                    return false;
                                }
#endif
                        data = TokenData.Null;
                    }

                    break;
                }

                case 'f':
                {
                    this.GetByte(out type);

                    if (!this.ReadCode(out var code))
                    {
                        this.Error("ReadValue: !ReadCode(num)");
                        return false;
                    }

                    if (code != (int)Code.CodeFunc)
                    {
                        this.Error("ReadValue: num != CODE_FUNC {0}", code);
                        return false;
                    }

                    if (!this.CodeFunc(ref data))
                    {
                        this.Error("ReadValue: !CodeFunc(data)");
                        return false;
                    }

                    break;
                }

                default:
                {
                    this.Error("ReadValue: not found c[{0}] d[{1}] h[{2:x}] ", type, type, type);
                    return false;
                }
            }

            dataQ.AddToBack(data);

            if (!this.GetByte(out var op))
            {
                this.Error("ReadValue:  !GetByte(op) {0}", type);
                return false;
            }

            if ((Op)op == Op.OpEnd)
            {
                break;
            }

            opQ.AddToBack(op);
        }

        for (var i = 0; i < opQ.Count; i++)
        {
            switch ((Op)opQ[i])
            {
                case Op.OpMul:
                {
                    dataQ[i + 1] = dataQ[i] * dataQ[i + 1];
                    dataQ[i].Set(0);
                    opQ[i] = (byte)Op.OpAdd;

                    break;
                }

                case Op.OpDiv:
                {
                    dataQ[i + 1] = dataQ[i] / dataQ[i + 1];
                    dataQ[i].Set(0);
                    opQ[i] = (byte)Op.OpAdd;

                    break;
                }
            }
        }

        while (opQ.Count > 0)
        {
            var d1 = dataQ[0];
            dataQ.RemoveFromFront();

            var d2 = dataQ[0];
            dataQ.RemoveFromFront();

            TokenData result = new();
            result.Set(0);

            switch ((Op)opQ[0])
            {
                case Op.OpAdd:
                {
                    result = d1 + d2;
                    break;
                }

                case Op.OpSub:
                {
                    result = d1 - d2;
                    break;
                }

                case Op.OpMod:
                {
                    result = d1 % d2;
                    break;
                }

                case Op.OpEqual:
                {
                    result.Set(d1 == d2);
                    break;
                }

                case Op.OpNotEqual:
                {
                    result.Set(d1 != d2);
                    break;
                }

                case Op.OpLarge:
                {
                    result.Set(d1 > d2);
                    break;
                }

                case Op.OpSmall:
                {
                    result.Set(d1 < d2);
                    break;
                }

                case Op.OpLargeOrEqual:
                {
                    result.Set(d1 >= d2);
                    break;
                }

                case Op.OpSmallOrEqual:
                {
                    result.Set(d1 <= d2);
                    break;
                }

                case Op.OpAnd:
                {
                    result.Set(d1 & d2);
                    break;
                }

                case Op.OpOr:
                {
                    result.Set(d1 | d2);
                    break;
                }

                default:
                {
                    throw new ArgumentOutOfRangeException();
                }
            }

            dataQ.AddToFront(result);
            opQ.RemoveFromFront();
        }

        ret = dataQ[0];
        return true;
    }
}