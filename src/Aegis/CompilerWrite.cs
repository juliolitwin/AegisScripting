using System;

namespace Scripting.Aegis;

public partial class Compiler
{
    public void WriteFunc(short func)
    {
        this._asm.Putf("[{0} {1:x}h] ", func, func);
        this._bin.WriteWord(func);
    }

    public void WriteType(char type)
    {
        switch (type)
        {
            case 's':
            {
                this._asm.Put("str:");
                break;
            }

            case 'n':
            {
                this._asm.Put("num:");
                break;
            }

            case 'f':
            {
                this._asm.Put("float:");
                break;
            }

            case ';':
            {
                this._asm.Put("end");
                break;
            }
        }

        this._bin.WriteByte(type);
    }

    public void WriteCode(Code code)
    {
        this._asm.Line(this._bin.GetPos());

        switch (code)
        {
            case Code.CodeEnd:
            {
                this._asm.Put("end ");
                break;
            }

            case Code.CodeMov:
            {
                this._asm.Put("mov ");
                break;
            }

            case Code.CodeAdd:
            {
                this._asm.Put("add ");
                break;
            }

            case Code.CodeSub:
            {
                this._asm.Put("sub ");
                break;
            }

            case Code.CodeMul:
            {
                this._asm.Put("mul ");
                break;
            }

            case Code.CodeDiv:
            {
                this._asm.Put("div ");
                break;
            }

            case Code.CodeInc:
            {
                this._asm.Put("inc ");
                break;
            }

            case Code.CodeDec:
            {
                this._asm.Put("dec ");
                break;
            }

            case Code.CodeFunc:
            {
                this._asm.Put("func ");
                break;
            }

            case Code.CodeCmp:
            {
                this._asm.Put("cmp ");
                break;
            }

            case Code.CodeGoto:
            {
                this._asm.Put("goto ");
                break;
            }

            case Code.CodePush:
            {
                this._asm.Put("push ");
                break;
            }

            case Code.CodePop:
            {
                this._asm.Put("pop ");
                break;
            }

            case Code.CodeCase:
            {
                this._asm.Put("case ");
                break;
            }

            default:
            {
                this._asm.Put("Error");
                break;
            }
        }

        this._bin.WriteByte('c');
        this._bin.WriteByte((byte)code);
    }

    public void WriteVar(string var)
    {
        this._asm.Putf("v[{0}] ", var);
        this._bin.WriteByte('v');
        this._bin.WriteStr(var);
    }

    public void WriteNum(int n)
    {
        this._asm.Putf("[{0} {0:x}h]", n);
        this._bin.WriteByte('n');
        this._bin.WriteNum(n);
    }

    public void WriteStr(string str)
    {
        this._asm.Putf("\"{0}\" ", str);
        this._bin.WriteByte('s');
        this._bin.WriteStr(str);
    }

    public void WriteCall(string func)
    {
        this._asm.Putf("call {0}", func);
        this._bin.WriteByte('f');
    }

    public bool WriteOp(string op)
    {
        Op code;

        if (string.Compare(op, ";", StringComparison.OrdinalIgnoreCase) == 0)
        {
            code = Op.OpEnd;
        }
        else if (string.Compare(op, "==", StringComparison.OrdinalIgnoreCase) == 0)
        {
            code = Op.OpEqual;
        }
        else if (string.Compare(op, "!=", StringComparison.OrdinalIgnoreCase) == 0)
        {
            code = Op.OpNotEqual;
        }
        else if (string.Compare(op, "<>", StringComparison.OrdinalIgnoreCase) == 0)
        {
            code = Op.OpNotEqual;
        }
        else if (string.Compare(op, ">=", StringComparison.OrdinalIgnoreCase) == 0 ||
                 string.Compare(op, "=>", StringComparison.OrdinalIgnoreCase) == 0)
        {
            code = Op.OpLargeOrEqual;
        }
        else if (string.Compare(op, "=<", StringComparison.OrdinalIgnoreCase) == 0 ||
                 string.Compare(op, "<=", StringComparison.OrdinalIgnoreCase) == 0)
        {
            code = Op.OpSmallOrEqual;
        }
        else if (string.Compare(op, ">", StringComparison.OrdinalIgnoreCase) == 0)
        {
            code = Op.OpLarge;
        }
        else if (string.Compare(op, "<", StringComparison.OrdinalIgnoreCase) == 0)
        {
            code = Op.OpSmall;
        }
        else if (string.Compare(op, "&&", StringComparison.OrdinalIgnoreCase) == 0 ||
                 string.Compare(op, "&",  StringComparison.OrdinalIgnoreCase) == 0)
        {
            code = Op.OpAnd;
        }
        else if (string.Compare(op, "||", StringComparison.OrdinalIgnoreCase) == 0 ||
                 string.Compare(op, "|",  StringComparison.OrdinalIgnoreCase) == 0)
        {
            code = Op.OpOr;
        }
        else if (string.Compare(op, "+", StringComparison.OrdinalIgnoreCase) == 0)
        {
            code = Op.OpAdd;
        }
        else if (string.Compare(op, "-", StringComparison.OrdinalIgnoreCase) == 0)
        {
            code = Op.OpSub;
        }
        else if (string.Compare(op, "*", StringComparison.OrdinalIgnoreCase) == 0)
        {
            code = Op.OpMul;
        }
        else if (string.Compare(op, "/", StringComparison.OrdinalIgnoreCase) == 0)
        {
            code = Op.OpDiv;
        }
        else if (string.Compare(op, "%", StringComparison.OrdinalIgnoreCase) == 0)
        {
            code = Op.OpMod;
        }
        else
        {
            this.Error("WriteOp: [{0}] not found", op);
            return false;
        }

        this._asm.Put(op);
        this._bin.WriteByte((byte)code);
        return true;
    }
}