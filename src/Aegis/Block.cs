using System.Collections.Generic;

namespace Scripting.Aegis;

internal class Block
{
    private readonly SortedDictionary<int, int> _infoMap = new();

    private readonly Stack<int> _posStack  = new();
    private readonly Stack<int> _stepStack = new();
    private          int        _alloc;
    private          int        _depth;
    private          int        _pos;
    private          int        _step;

    /// <summary>
    ///     Initializes a new instance of the <see cref="Block" /> class.
    ///     Constructor.
    /// </summary>
    public Block()
    {
        this._alloc = 0;
        this._pos   = 0;
        this._step  = 0;
        this._depth = 0;
    }

    public bool IsComplete()
    {
        return this._depth == 0;
    }

    public void GetStrInfo(out string buf)
    {
        buf = $"index {this._pos:D}-{this._step:D} depth {this._depth:D}";
    }

    public bool GetInfo(int id, out int info)
    {
        info = 0;

        if (!this._infoMap.ContainsKey(id))
        {
            return false;
        }

        info = this._infoMap[id];
        return true;
    }

    public int GetStartId()
    {
        return (this._pos << 8) | 0;
    }

    public int GetCurId()
    {
        return (this._pos << 8) | this._step;
    }

    public int GetNextId()
    {
        return (this._pos << 8) | (this._step + 1);
    }

    public int GetEndId()
    {
        return (this._pos << 8) | 0xff;
    }

    private void Label(int id, int info)
    {
        this._infoMap[id] = info;
    }

    public bool Start(int info = 0)
    {
        if (this._alloc >= 0xffffff)
        {
            return false;
        }

        this._stepStack.Push(this._step);
        this._posStack.Push(this._pos);

        this._pos = this._alloc;
        this._alloc++;

        this._step = 0;
        this._depth++;

        this.Label(this.GetCurId(), info);
        return true;
    }

    public bool Link(int info = 0)
    {
        if (this._step >= byte.MaxValue)
        {
            return false;
        }

        this._step++;
        this.Label(this.GetCurId(), info);
        return true;
    }

    public bool End(int info = 0)
    {
        this.Label(this.GetEndId(), info);

        if (this._depth == 0)
        {
            return false;
        }

        this._step = this._stepStack.Pop();
        this._pos  = this._posStack.Pop();

        this._depth--;
        return this._depth >= 0;
    }
}