using System;

namespace Scripting.Aegis;

public partial class Compiler
{
    public bool CheckSwitchBlock()
    {
        if (this._nSwitch > 0)
        {
            if (this._isCase)
            {
                return true;
            }

            if (this._nCmd == Cmd.CmdEndif)
            {
                if (this._nIf > 0)
                {
                    return true;
                }
            }
            else
            {
                this.Error("at switch-endswitch just can use command in case-break!");
                return false;
            }
        }

        return true;
    }

    public bool CmdBlockBreak(ref ScriptLine line)
    {
        this._block.GetStrInfo(out var info);
        this.Error("current block info - {0}", info);
        return false;
    }

    public bool CmdDefCmd(ref ScriptLine line)
    {
        if (!this.CheckSwitchBlock())
        {
            return false;
        }

        line.Skip(" \t");

        if (!line.GetWord(out var name, " \t"))
        {
            this.Error("there are no define command");
            return false;
        }

        line.Skip(" \t");
        if (!line.GetWord(out var data, " \t"))
        {
            this.Error("there are no base command");
            return false;
        }

        if (!this.TokenMap.Get(data, out var pTokenInfo))
        {
            this.Error("{0} not exist", data);
            return false;
        }

        this.TokenMap.Set(name, pTokenInfo.TokenType, pTokenInfo.ValueNumber, pTokenInfo.GetStr());
        return true;
    }

    public bool GetDefVar(ref ScriptLine line, ref string data, out int isNum)
    {
        isNum = 0;

        if (!this.CheckSwitchBlock())
        {
            return false;
        }

        if (!line.GetWord(out data, " \t+-"))
        {
            this.Error("there are no define or number");
            return false;
        }

        if (this.IsNum(data))
        {
            isNum = 1;
            return true;
        }

        if (!this.TokenMap.Get(data, out var pTokenInfo))
        {
            this.Error("{0} - not defined token", data);
            return false;
        }

        data  = pTokenInfo.ValueString;
        isNum = pTokenInfo.ValueNumber;

        if (line.GetOperator(out var op, "+-") &&
            isNum > 0)
        {
            var n = int.Parse(data);
            if (string.Compare(op, "++", StringComparison.OrdinalIgnoreCase) == 0)
            {
                n++;
            }
            else if (string.Compare(op, "--", StringComparison.OrdinalIgnoreCase) == 0)
            {
                n--;
            }
            else
            {
                this.Error("{0} can not use define operator", op);
                return false;
            }

            pTokenInfo.ValueString = $"{n}\n";
        }

        return true;
    }

    public bool CmdDefine(ref ScriptLine line)
    {
        if (!this.CheckSwitchBlock())
        {
            return false;
        }

        int isNum;
        line.Skip(" \t");
        if (!line.GetWord(out var name, " \t"))
        {
            this.Error("no define name");
            return false;
        }

        line.Skip(" \t");
        if (line.GetParse(out var data, '"'))
        {
            isNum = 0;
        }
        else if (!this.GetDefVar(ref line, ref data, out isNum))
        {
            this.Error("problem of define value");
            return false;
        }

        this.TokenMap.Set(name, TokenType.Define, isNum, data);

        this._asm.Commentf("define {0} {1}", name, data);
        return true;
    }

    public bool CmdDeclare(ref ScriptLine line)
    {
        if (!this.CheckSwitchBlock())
        {
            return false;
        }

        var code = "";

        line.Skip(" \t");

        if (!line.GetWord(out var func, " \t"))
        {
            this.Error("no func name");
            return false;
        }

        line.Skip(" \t");
        if (!line.GetWord(out var parm, " \t"))
        {
            this.Error("no parameter info");
            return false;
        }

        line.Skip(" \t");
        if (!this.GetDefVar(ref line, ref code, out var isNum))
        {
            this.Error("there are problem func code part");
            return false;
        }

        line.Skip(" \t");
        line.GetWord(out var sp, " \t");

        if (isNum == 0)
        {
            this.Error("just only number at func code");
            return false;
        }

        this.TokenMap.Set(func, TokenType.Func, int.Parse(code), parm);
        this._asm.Commentf("declare {0} {1} {2}", func, parm, code);

        if (string.Compare(sp, "blockcheck", StringComparison.OrdinalIgnoreCase) == 0)
        {
            this._blockCheckMap[int.Parse(code)] = true;
            this._asm.Comment("block check func");
        }

        return true;
    }

    public bool CmdVar(ref ScriptLine line)
    {
        if (!this.CheckSwitchBlock())
        {
            return false;
        }

        line.Skip(" \t");
        if (!line.GetWord(out var var, "= \t"))
        {
            this.Error("no value name");
            return false;
        }

        this.TokenMap.Set(var, TokenType.Var);
        if (!this.OnVar(ref line, var))
        {
            return false;
        }

        this._asm.Commentf("int {0}", var);
        return true;
    }

    public bool CmdWhile(ref ScriptLine line)
    {
        if (!this.CheckSwitchBlock())
        {
            return false;
        }

        this._whileStack.Push(this._whileBlock);

        this.WriteCode(Code.CodeCmp);
        if (!this.Value(ref line))
        {
            this.Error("if error");
            return false;
        }

        this._asm.Putf("not ");

        this._whileBlock = this._block.GetEndId();
        this.WriteGotoBlock(this._whileBlock);
        return true;
    }

    public bool CmdExitWhile(ref ScriptLine line)
    {
        if (!this.CheckSwitchBlock())
        {
            return false;
        }

        this.Goto(this._whileBlock);
        return true;
    }

    public bool CmdEndWhile(ref ScriptLine line)
    {
        if (!this.CheckSwitchBlock())
        {
            return false;
        }

        if (this._whileStack.Count == 0)
        {
            this.Error("while - endwhile not match number");
            return false;
        }

        this._whileBlock = this._whileStack.Pop();
        return true;
    }

    public bool CmdIf(ref ScriptLine line)
    {
        if (!this.CheckSwitchBlock())
        {
            return false;
        }

        this.WriteCode(Code.CodeCmp);
        if (!this.Value(ref line))
        {
            this.Error("if error");
            return false;
        }

        this._asm.Putf("not ");
        this._nIf++;
        this.WriteGotoBlock(this._block.GetNextId());
        return true;
    }

    public bool CmdElseIf(ref ScriptLine line)
    {
        if (!this.CheckSwitchBlock())
        {
            return false;
        }

        this.WriteCode(Code.CodeCmp);
        if (!this.Value(ref line))
        {
            this.Error("if error");
            return false;
        }

        this._asm.Putf("not ");
        this.WriteGotoBlock(this._block.GetNextId());
        return true;
    }

    public bool CmdElse(ref ScriptLine line)
    {
        if (!this.CheckSwitchBlock())
        {
            return false;
        }

        return true;
    }

    public bool CmdEndIf(ref ScriptLine line)
    {
        if (!this.CheckSwitchBlock())
        {
            return false;
        }

        if (this._nSwitchInIf[this._nIf] > 0)
        {
            this.Error("there are not endchoose commanded in if block!");
        }

        this._nIf--;
        return true;
    }

    public bool CmdEnd(ref ScriptLine line)
    {
        if (!this.CheckSwitchBlock())
        {
            return false;
        }

        this.WriteCode(Code.CodeEnd);
        return true;
    }

    public bool CmdSwitch(ref ScriptLine line)
    {
        if (!this.CheckSwitchBlock())
        {
            return false;
        }

        this.WriteCode(Code.CodePush);
        this.WriteVar("$case");

        this.WriteCode(Code.CodeMov);
        this.WriteVar("$case");

        if (!this.Value(ref line))
        {
            this.Error("switch error");
            return false;
        }

        this._nSwitch++;
        if (this._nIf > 0)
        {
            if (this._nSwitchInIf[this._nIf] < MaxSwitch)
            {
                this._nSwitchInIf[this._nIf]++;
            }
            else
            {
                this.Error("too many choosemenu command in if block");
            }
        }

        return true;
    }

    public bool CmdCase(ref ScriptLine line)
    {
        this.WriteCode(Code.CodeCase);
        if (!this.Value(ref line))
        {
            this.Error("case error");
            return false;
        }

        this._asm.Putf("not ");
        this.WriteGotoBlock(this._block.GetNextId());
        this._isCase = true;
        return true;
    }

    public bool CmdBreak(ref ScriptLine line)
    {
        if (!this.CheckSwitchBlock())
        {
            return false;
        }

        if (this._nSwitch == 1)
        {
            this._isCase = false;
        }

        return true;
    }

    public bool CmdDefault(ref ScriptLine line)
    {
        if (!this.CheckSwitchBlock())
        {
            return false;
        }

        return true;
    }

    public bool CmdEndSwitch(ref ScriptLine line)
    {
        if (this._nSwitch == 0)
        {
            this.Error("none choosemenu commanded before");
        }

        this._nSwitch--;
        if (this._nIf > 0)
        {
            this._nSwitchInIf[this._nIf]--;
        }

        this.WriteCode(Code.CodePop);
        this.WriteVar("$case");
        return true;
    }

    /// <inheritdoc/>
    public override bool OnCommand(ref ScriptLine line, Cmd cmd)
    {
        this._nCmd = cmd;

        switch (cmd)
        {
            case Cmd.CmdEnd:
            {
                return this.CmdEnd(ref line);
            }

            case Cmd.CmdVar:
            {
                return this.CmdVar(ref line);
            }

            case Cmd.CmdIf:
            {
                return this.CmdIf(ref line);
            }

            case Cmd.CmdElseif:
            {
                return this.CmdElseIf(ref line);
            }

            case Cmd.CmdElse:
            {
                return this.CmdElse(ref line);
            }

            case Cmd.CmdEndif:
            {
                return this.CmdEndIf(ref line);
            }

            case Cmd.CmdDeclare:
            {
                return this.CmdDeclare(ref line);
            }

            case Cmd.CmdDefine:
            {
                return this.CmdDefine(ref line);
            }

            case Cmd.CmdSwitch:
            {
                return this.CmdSwitch(ref line);
            }

            case Cmd.CmdCase:
            {
                return this.CmdCase(ref line);
            }

            case Cmd.CmdBreak:
            {
                return this.CmdBreak(ref line);
            }

            case Cmd.CmdDefault:
            {
                return this.CmdDefault(ref line);
            }

            case Cmd.CmdEndswitch:
            {
                return this.CmdEndSwitch(ref line);
            }

            case Cmd.CmdDefcmd:
            {
                return this.CmdDefCmd(ref line);
            }

            case Cmd.CmdBlockbreak:
            {
                return this.CmdBlockBreak(ref line);
            }

            case Cmd.CmdWhile:
            {
                return this.CmdWhile(ref line);
            }

            case Cmd.CmdEndwhile:
            {
                return this.CmdEndWhile(ref line);
            }

            case Cmd.CmdExitwhile:
            {
                return this.CmdExitWhile(ref line);
            }

            default:
            {
                this.Error("cant identify func code{0}", cmd);
                break;
            }
        }

        return false;
    }

    /// <inheritdoc/>
    public override bool OnControl(ref ScriptLine line, int cmd)
    {
        switch ((Cmd)cmd)
        {
            case Cmd.CmdWhile:
            case Cmd.CmdSwitch:
            case Cmd.CmdIf:
            {
                if (!this._block.Start(this._bin.GetPos()))
                {
                    this.Error("too many block number");
                    return false;
                }

                this._asm.Commentf("block {0}-{1} id{2}  addr hex[{3}]", this._block.GetCurId() >> 8,
                                   this._block.GetCurId() & 0xff, this._block.GetCurId(), this._bin.GetPos());
                break;
            }

            case Cmd.CmdBreak:
            {
                this.Goto(this._block.GetEndId());
                break;
            }

            case Cmd.CmdDefault:
            case Cmd.CmdCase:
            {
                if (!this._block.Link(this._bin.GetPos()))
                {
                    this.Error("too many linked block number");
                    return false;
                }

                this._asm.Commentf("block {0}-{1} id{2}  addr hex[{3}]", this._block.GetCurId() >> 8,
                                   this._block.GetCurId() & 0xff, this._block.GetCurId(), this._bin.GetPos());
                break;
            }

            case Cmd.CmdElseif:
            case Cmd.CmdElse:
            {
                this.Goto(this._block.GetEndId());
                if (!this._block.Link(this._bin.GetPos()))
                {
                    this.Error("too many linked block number");
                    return false;
                }

                this._asm.Commentf("block {0}-{1} id{2}  addr hex[{3}]", this._block.GetCurId() >> 8,
                                   this._block.GetCurId() & 0xff, this._block.GetCurId(), this._bin.GetPos());
                break;
            }

            case Cmd.CmdEndwhile:
            case Cmd.CmdEndswitch:
            case Cmd.CmdEndif:
            {
                if ((Cmd)cmd == Cmd.CmdEndwhile)
                {
                    this.Goto(this._block.GetStartId());
                }

                if (!this._block.Link(this._bin.GetPos()))
                {
                    this.Error("too many linked block number");
                    return false;
                }

                this._asm.Commentf("block {0}-{1} id{2}  addr hex[{3}]", this._block.GetCurId() >> 8,
                                   this._block.GetCurId() & 0xff, this._block.GetCurId(), this._bin.GetPos());
                this._asm.Commentf("block {0}-255 id{1}  addr hex[{2}]", this._block.GetCurId() >> 8,
                                   ((this._block.GetCurId() >> 8) << 8) | 0xff, this._bin.GetPos());

                if (!this._block.End(this._bin.GetPos()))
                {
                    this.Error("block {} not match number");
                    return false;
                }

                break;
            }
        }

        return true;
    }

    public void Goto(int block)
    {
        this.WriteCode(Code.CodeGoto);
        this.WriteGotoBlock(block);
    }

    public void WriteGotoBlock(int block)
    {
        var gotoInfo = new GotoInfo
                       {
                           Pos = this._bin.GetPos(), Id = block
                       };

        this._gotoInfo.Add(gotoInfo);
        this._asm.Putf("[addr {0:x}] goto {1}-{2}\tid", gotoInfo.Pos, block >> 8, block & 0xff);
        this.WriteNum(block);
    }
}