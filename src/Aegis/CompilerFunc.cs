namespace Scripting.Aegis;

public partial class Compiler
{
    /// <inheritdoc/>
    public override bool OnFunc(ref ScriptLine line, int func, string parameter)
    {
        if (!this.CheckSwitchBlock())
        {
            return false;
        }

        if (this._blockCheckMap.ContainsKey(func))
        {
            if (!this._block.IsComplete())
            {
                this.Error("The if or multiple-selection block at the top is not complete. Check with __block.");
                return false;
            }
        }

        this.WriteCode(Code.CodeFunc);
        this.WriteFunc((short)func);

        var index     = 0;
        var maxLength = parameter.Length;

        while (true)
        {
            if (index == maxLength)
            {
                break;
            }

            var pc = parameter[index];

            if (pc == '.')
            {
                break;
            }

            if (pc == 't')
            {
                line.Skip(" \t");
                line.GetWord(out _, " \t");
                index++;

                continue;
            }

            this.WriteType(pc);

            var flag = pc != '?';
            if (!this.Value(ref line, flag))
            {
                if (flag)
                {
                    this.Error("function parameter error {0} [{1}]", parameter, pc);
                }
                else
                {
                    this.WriteVar("NULL");
                    this.WriteOp(";");
                    break;
                }

                return false;
            }

            if (flag)
            {
                index++;
            }
        }

        this.WriteType(';');
        return true;
    }
}