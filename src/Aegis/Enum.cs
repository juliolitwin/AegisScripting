namespace Scripting.Aegis;

public enum Op
{
    OpEnd,
    OpEqual,
    OpNotEqual,
    OpLarge,
    OpSmall,
    OpAnd,
    OpOr,
    OpAdd,
    OpSub,
    OpMul,
    OpDiv,
    OpMod,
    OpLargeOrEqual,
    OpSmallOrEqual
}

public enum Cmd
{
    CmdEnd,
    CmdVar,
    CmdIf,
    CmdElseif,
    CmdElse,
    CmdEndif,
    CmdDeclare,
    CmdDefine,
    CmdSwitch,
    CmdCase,
    CmdBreak,
    CmdDefault,
    CmdEndswitch,
    CmdDefcmd,
    CmdBlockbreak,
    CmdWhile,
    CmdEndwhile,
    CmdExitwhile
}

public enum Code
{
    CodeEnd, // end		

    CodeMov, // mov var var/data
    CodeAdd, // add var var/data
    CodeSub, // sub var var/data
    CodeMul, // mul var var/data
    CodeDiv, // div var var/data

    CodeMod,  // mod var var/data
    CodeInc,  // inc var 
    CodeDec,  // dec var 
    CodeCmp,  // cmp var notgotoblock
    CodeGoto, // goto block

    CodeFunc,
    CodeCase,
    CodePush,
    CodePop
}

public enum TokenType
{
    None,
    Command,
    Func,
    Var,
    Define
}