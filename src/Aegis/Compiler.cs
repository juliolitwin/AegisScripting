using System;
using System.Collections.Generic;
using NetFabric.Hyperlinq;
using Nito.Collections;
using Scripting.Utilities;

namespace Scripting.Aegis;

public partial class Compiler : ScriptHandler
{
    public delegate void GlobalErrorFunc(string message);

    public const int MaxSwitch = 30;

    private readonly Asm _asm = new();
    private readonly Bin _bin = new();

    private readonly Block                  _block         = new();
    private readonly IDictionary<int, bool> _blockCheckMap = new SortedDictionary<int, bool>();

    private readonly IList<GotoInfo> _gotoInfo    = new List<GotoInfo>();
    private readonly int[]           _nSwitchInIf = new int[MaxSwitch];
    private readonly Script          _script      = new();

    private readonly Stack<int> _whileStack = new();

    private string _fileName = "";
    private bool   _isCase;

    private bool _isError;
    private Cmd  _nCmd;

    private int _nIf;
    private int _nSwitch;
    private int _whileBlock;

    private GlobalErrorFunc OnGlobalErrorFunc;

    /// <inheritdoc />
    public Compiler()
    {
        this._script.RegisterHandler(this);
        this.OnGlobalErrorFunc = default(GlobalErrorFunc)!;

        this.SetCmd(Cmd.CmdBlockbreak, "__block");
        this.SetCmd(Cmd.CmdEnd,        "end");
        this.SetCmd(Cmd.CmdVar,        "var");
        this.SetCmd(Cmd.CmdIf,         "if");
        this.SetCmd(Cmd.CmdElseif,     "elseif");
        this.SetCmd(Cmd.CmdElse,       "else");
        this.SetCmd(Cmd.CmdEndif,      "endif");
        this.SetCmd(Cmd.CmdDeclare,    "declare");
        this.SetCmd(Cmd.CmdDefine,     "define");
        this.SetCmd(Cmd.CmdSwitch,     "choose");
        this.SetCmd(Cmd.CmdCase,       "case");
        this.SetCmd(Cmd.CmdBreak,      "break");
        this.SetCmd(Cmd.CmdDefault,    "default");
        this.SetCmd(Cmd.CmdEndswitch,  "endchoose");
        this.SetCmd(Cmd.CmdDefcmd,     "defcmd");
        this.SetCmd(Cmd.CmdWhile,      "while");
        this.SetCmd(Cmd.CmdEndwhile,   "endwhile");
        this.SetCmd(Cmd.CmdExitwhile,  "exitwhile");

        this._whileBlock = 0;
        this._nSwitch    = 0;
    }

    /// <inheritdoc />
    public void Optimize()
    {
        this.WriteCode(Code.CodeEnd);

        this._asm.Put("\n\n\n");
        this._asm.Comment("optimize");
        this._asm.Comment("===================================================================");

        var count = this._gotoInfo.Count;
        for (var i = 0; i < count; i++)
        {
            this._bin.Seek(this._gotoInfo[i].Pos);
            this._block.GetInfo(this._gotoInfo[i].Id, out var addr);
            this._asm.Commentf("optimize: move {0:x} - block {1} -> addr hex {2:x}:dec ", this._gotoInfo[i].Pos,
                               this._gotoInfo[i].Id, addr);
            this.WriteNum(addr);
        }
    }

    /// <inheritdoc />
    private void SplitString(string src, out Deque<string> ret, string szDiv)
    {
        ret = new Deque<string>();

        var stTemp = "";
        for (var i = 0; i < src.Length; i++)
        {
            var c = src[i];

            if (!string.IsNullOrEmpty(StringFunctions.StrChr(szDiv, c)))
            {
                if (stTemp == "")
                {
                    continue;
                }

                ret.AddToBack(stTemp);
                stTemp = "";
            }
            else
            {
                stTemp += src[i];
            }
        }

        if (stTemp != "")
        {
            ret.AddToBack(stTemp);
        }
    }

    /// <inheritdoc />
    public bool LoadEnum(string fName, int verDate)
    {
        var scan = new ScanFormatted();

        var iter = 0;
        int date;

        var lines = FileUtilities.OpenFileAndGetLines(fName);
        if (lines is null ||
            lines.Count == 0)
        {
            return false;
        }

        if (verDate != 0)
        {
            scan.Parse(lines.AsValueEnumerable().First().Value ?? string.Empty, "%d\n");
            if (scan.Results.Count > 0)
            {
                date = (int)scan.Results.AsValueEnumerable().First().Value;
                if (date != verDate)
                {
                    // Fixed to ignore even if there is a date due to frequent errors in the overseas version check part.
#if false
                        return false;
#endif
                }
            }
        }

        for (var i = 0; i < lines.Count; i++)
        {
            var line = lines.AsValueEnumerable().ElementAt(i).Value;

            // Ignore if line is null or empty.
            if (string.IsNullOrEmpty(line))
            {
                continue;
            }

            switch (line[0])
            {
                case ';':
                {
                    continue;
                }

                case '#':
                {
                    var parseCount = scan.Parse(line, "%c %d");
                    if (parseCount != 2)
                    {
                        throw new InvalidOperationException($"Invalid parsing count. Got count: {parseCount}.");
                    }

                    iter = (int)scan.Results[1];
                    break;
                }

                default:
                {
                    this.SplitString(line, out var wordQ, ", \t\r\n");

                    for (var k = 0; k < wordQ.Count; k++)
                    {
                        var word = wordQ.AsValueEnumerable().ElementAt(k).Value;
                        if (string.IsNullOrEmpty(word))
                        {
                            continue;
                        }

                        if (word.Length >= 2 &&
                            word[..2]   == "//")
                        {
                            break;
                        }

                        this.TokenMap.Set(word, TokenType.Define, 1, $"{iter:D}\n");
                        iter++;
                    }

                    break;
                }
            }
        }

        return true;
    }

    /// <inheritdoc />
    public bool LoadDef(string fName)
    {
        var scan = new ScanFormatted();

        var lines = FileUtilities.OpenFileAndGetLines(fName);
        for (var i = 0; i < lines.Count; i++)
        {
            var line = lines.AsValueEnumerable().ElementAt(i).Value;
            if (string.IsNullOrEmpty(line))
            {
                continue;
            }

            var parseCount = scan.Parse(line, "%s %s");
            if (parseCount < 2)
            {
                throw new InvalidOperationException($"Invalid parsing count. Got count: {parseCount}.");
            }

            var name = scan.Results[0] as string ?? throw new InvalidOperationException();
            var num  = scan.Results[1] as string ?? throw new InvalidOperationException();

            this.TokenMap.Set(name, TokenType.Define, 1, num);
        }

        return true;
    }

    /// <inheritdoc />
    public bool Run(string fName, int verDate = 0)
    {
        this._isError = false;
        this._nSwitch = 0;
        this._nIf     = 0;

        for (var i = 0; i < MaxSwitch; i++)
        {
            this._nSwitchInIf[i] = 0;
        }

        this._nCmd   = 0;
        this._isCase = false;
        this._asm.Comment("===================================================================\n");
        this._asm.Commentf("Load {0}", fName);

        this._fileName = fName;

        if (!this._script.Load(fName, verDate))
        {
            this.Error("Load Error {0}", fName);
            return false;
        }

        this._asm.Comment("===================================================================\n");
        for (var i = 0; i < this._script.GetLineNum(); i++)
        {
            if (!this._script.Analyze(i) ||
                this._isError)
            {
                this.Error("file {0} line {1}: {2}", fName, i + 1, this._script.GetLine(i));
                return false;
            }
        }

        return true;
    }

    /// <inheritdoc />
    public void SetCmd(Cmd cmd, string str)
    {
        this.TokenMap.Set(str, TokenType.Command, (int)cmd);
    }

    /// <inheritdoc />
    public bool SetBin(string fName)
    {
        return this._bin.Set(fName);
    }

    /// <inheritdoc />
    public bool SetAsm(string fName)
    {
        return this._asm.Set(fName);
    }

    /// <inheritdoc />
    public void Release()
    {
        this._asm.Release();
        this._bin.Release();
        this._gotoInfo.Clear();
    }

    /// <inheritdoc />
    public override void Error(string str, params object[] args)
    {
        this._isError = true;

        var buf = string.Format(str, args);

        this._asm.Error(buf);
        this.OnGlobalErrorFunc?.Invoke($"{this._fileName}, {buf}");
    }

    /// <inheritdoc />
    public void SetGlobalErrorFunc(GlobalErrorFunc func)
    {
        this.OnGlobalErrorFunc = func;
    }

    private class GotoInfo
    {
        public int Pos { get; init; }

        public int Id { get; init; }
    }
}