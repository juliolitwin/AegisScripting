using System.Collections.Generic;

namespace Scripting.Aegis;

public partial class Interpreter
{
    private readonly Stack<TokenData> _tokenDataStack = new();

    private byte[]           _bin = default!;
    private bool             _gotoLock;
    private InterpretHandler _handler = default!;
    private long             _pos;

    private bool _scan;
    private int  _size;

    protected VarMap VarMap = new();

    /// <summary>
    ///     Initializes a new instance of the <see cref="Interpreter" /> class.
    ///     Constructor.
    /// </summary>
    public Interpreter()
    {
        this.Clear();
    }

    public void SetVar(string var, int value)
    {
        this.VarMap.Set(var, new TokenData
                             {
                                 String = "", Number = value, Type = TokenData.DataTypes.Num
                             });
    }

    public void RegisterHandler(InterpretHandler handler)
    {
        this._handler = handler;
        this._handler.SetVarMap(this.VarMap);
    }

    public bool Proc()
    {
        var temp = new TokenData();

        if (this._bin == null)
        {
            this.Error("m_bin == null");
            return false;
        }

        if (this._pos == this._size)
        {
            this.Error("pos == size");
            return false;
        }

        if (this._pos < 0 ||
            this._pos > this._size)
        {
            this.Error("pos {0} error [0~{0}] size= {1}", this._pos, this._size);
            return false;
        }

        if (!this.ReadCode(out var code))
        {
            this.Error("Read code error pos{0}", this._pos);
            return false;
        }

        switch ((Code)code)
        {
            case Code.CodeFunc:
            {
                if (!this.CodeFunc(ref temp))
                {
                    return false;
                }

                break;
            }

            case Code.CodeGoto:
            {
                if (!this.CodeGoto())
                {
                    return false;
                }

                break;
            }

            case Code.CodeMov:
            {
                if (!this.CodeMov())
                {
                    return false;
                }

                break;
            }

            case Code.CodeCmp:
            {
                if (!this.CodeCmp())
                {
                    return false;
                }

                break;
            }

            case Code.CodeAdd:
            {
                if (!this.CodeAdd())
                {
                    return false;
                }

                break;
            }

            case Code.CodeSub:
            {
                if (!this.CodeSub())
                {
                    return false;
                }

                break;
            }

            case Code.CodeMul:
            {
                if (!this.CodeMul())
                {
                    return false;
                }

                break;
            }

            case Code.CodeDiv:
            {
                if (!this.CodeDiv())
                {
                    return false;
                }

                break;
            }

            case Code.CodeMod:
            {
                if (!this.CodeMod())
                {
                    return false;
                }

                break;
            }

            case Code.CodeInc:
            {
                if (!this.CodeInc())
                {
                    return false;
                }

                break;
            }

            case Code.CodeDec:
            {
                if (!this.CodeDec())
                {
                    return false;
                }

                break;
            }

            case Code.CodePush:
            {
                if (!this.CodePush())
                {
                    return false;
                }

                break;
            }

            case Code.CodePop:
            {
                if (!this.CodePop())
                {
                    return false;
                }

                break;
            }

            case Code.CodeCase:
            {
                if (!this.CodeCase())
                {
                    return false;
                }

                break;
            }

            case Code.CodeEnd:
            {
                return false;
            }

            default:
            {
                this.Error("Code error pos{0} code[{1}]", this._pos, code);
                return false;
            }
        }

        return true;
    }

    public bool Scan(BinBuf binBuf, int pos)
    {
        if (!this.Run(binBuf, pos))
        {
            return false;
        }

        this._scan = true;

        // Process the interpreter.
        while (this.Proc()) { }

        this._scan = false;
        return true;
    }

    public bool Run(BinBuf binBuf, long pos, bool gotoLock = false)
    {
        this._scan     = false;
        this._gotoLock = gotoLock;

        if (binBuf == null)
        {
            this.Error("Bin error");
            return false;
        }

        this._bin  = binBuf.GetBase();
        this._size = binBuf.GetSize();
        this._pos  = pos;

        if (this._pos < 0 ||
            this._pos >= this._size)
        {
            this.Error("pos {0} error [0~{1}] limit", pos, this._size);
            return false;
        }

        this.VarMap.Clear();

        return true;
    }

    public void Error(string str, params object[] args)
    {
        this._handler?.OnError(string.Format(str, args));
    }

    public void Clear()
    {
        this._scan     = false;
        this._gotoLock = false;
        this._pos      = 0;
        this._size     = 0;

        this._handler = default(InterpretHandler)!;
        this._bin     = default(byte[])!;

        this.VarMap.Clear();
    }
}