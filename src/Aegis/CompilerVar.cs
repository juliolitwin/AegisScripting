using System;
using NetFabric.Hyperlinq;

namespace Scripting.Aegis;

public partial class Compiler
{
    public bool Value(ref ScriptLine line, bool flag = true)
    {
        while (true)
        {
            line.Skip(" \t[");

            if (line.GetParse(out var data, '"'))
            {
                if (data.Length >= 250)
                {
                    this.Error("Value: 250 strlen(data) >= 250 {0}", data);
                    return false;
                }

                this.WriteStr(data);
            }
            else if (line.GetParse(out data, '#'))
            {
                if (!this.TokenMap.Get(data, out var tokenInfo))
                {
                    this.Error("Value: [{0}]  is not in a token map - GetParse", data);
                    return false;
                }

                if (tokenInfo.ValueNumber != 0)
                {
                    this.WriteNum(int.Parse(tokenInfo.GetStr()));
                }
                else
                {
                    this.WriteStr(tokenInfo.GetStr());
                }
            }
            else if (line.GetWord(out data, "%!=+-/*&|>< \t[],"))
            {
                if (this.IsNum(data))
                {
                    this.WriteNum(int.Parse(data));
                }
                else
                {
                    if (!this.TokenMap.Get(data, out var tokenInfo))
                    {
                        this.Error("Value: [{0}] is not in a token map - GetWord", data);
                        return false;
                    }

                    switch (tokenInfo.TokenType)
                    {
                        case TokenType.Var:
                        {
                            this.WriteVar(data);
                            break;
                        }

                        case TokenType.Define:
                        {
                            if (tokenInfo.ValueNumber != 0)
                            {
                                this.WriteNum(int.Parse(tokenInfo.GetStr()));
                            }
                            else
                            {
                                this.WriteStr(tokenInfo.GetStr());
                            }

                            break;
                        }

                        case TokenType.Func:
                        {
                            this.WriteCall(data);
                            if (!this.OnFunc(ref line, tokenInfo.ValueNumber, tokenInfo.GetStr()))
                            {
                                this.Error("Value: Func not found");
                                return false;
                            }

                            break;
                        }

                        default:
                        {
                            this.Error("Value: {0} tokenInfo->type not found line:{1}", data, line.GetBase());
                            return false;
                        }
                    }
                }
            }
            else
            {
                // if (flag) Error("Value:값이 없습니다 line:%s", line.GetBase());
                return false;
            }

            line.Skip(" \t,");
            if (!line.GetOperator(out var op, "%=+-/*&|><!"))
            {
                break;
            }

            if (!this.WriteOp(op))
            {
                this.Error("Value: write error!");
                return false;
            }
        }

        line.Skip("]");
        this.WriteOp(";");
        return true;
    }

    /// <inheritdoc/>
    public override bool OnVar(ref ScriptLine line, string name)
    {
        line.Skip(" \t");
        if (!line.GetOperator(out var op, "=+-*/%"))
        {
            if (string.IsNullOrEmpty(op))
            {
                return true;
            }

            this.Error("OnVar1: !op [{0}] ", op);
            return false;
        }

        // Get the current code.
        Code code;

        if (string.Compare(op, "=", StringComparison.OrdinalIgnoreCase) == 0)
        {
            code = Code.CodeMov;
        }
        else if (string.Compare(op, "+=", StringComparison.OrdinalIgnoreCase) == 0)
        {
            code = Code.CodeAdd;
        }
        else if (string.Compare(op, "-=", StringComparison.OrdinalIgnoreCase) == 0)
        {
            code = Code.CodeSub;
        }
        else if (string.Compare(op, "*=", StringComparison.OrdinalIgnoreCase) == 0)
        {
            code = Code.CodeMul;
        }
        else if (string.Compare(op, "/=", StringComparison.OrdinalIgnoreCase) == 0)
        {
            code = Code.CodeDiv;
        }
        else if (string.Compare(op, "++", StringComparison.OrdinalIgnoreCase) == 0)
        {
            code = Code.CodeInc;
        }
        else if (string.Compare(op, "--", StringComparison.OrdinalIgnoreCase) == 0)
        {
            code = Code.CodeDec;
        }
        else if (string.Compare(op, "%=", StringComparison.OrdinalIgnoreCase) == 0)
        {
            code = Code.CodeMod;
        }
        else
        {
            this.Error("OnVar2: [{0}]  operator error1", op);
            return false;
        }

        this.WriteCode(code);
        this.WriteVar(name);

        if (code is Code.CodeInc or Code.CodeDec)
        {
            return true;
        }

        if (this.Value(ref line))
        {
            return true;
        }

        this.Error("{0} OnVar: !Value(line)", name);
        return false;
    }

    public bool IsNum(string str)
    {
        return str.AsValueEnumerable().All(char.IsNumber);
    }
}