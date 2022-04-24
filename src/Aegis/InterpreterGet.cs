using System.Text;

namespace Scripting.Aegis;

public partial class Interpreter
{
    public bool PeekByte(out byte data)
    {
        data = 0;
        var size = sizeof(byte);
        if (this._pos + size > this._size)
        {
            this.Error("PeekByte error {0} + {1}/{2}", this._pos, size, this._size);
            return false;
        }

        data = this._bin[this._pos];
        return true;
    }

    public bool GetByte(out byte data)
    {
        data = 0;
        var size = sizeof(byte);
        if (this._pos + size > this._size)
        {
            this.Error("GetByte error {0} + {1}/{2}", this._pos, size, this._size);
            return false;
        }

        data      =  this._bin[this._pos];
        this._pos += size;
        return true;
    }

    public bool GetWord(out ushort data)
    {
        data = 0;
        var size = sizeof(ushort);
        if (this._pos + size > this._size)
        {
            this.Error("GetWord error {0} + {1}/{2}", this._pos, size, this._size);
            return false;
        }

        data      =  (ushort)((this._bin[this._pos + 1] << 8) | this._bin[this._pos]);
        this._pos += size;
        return true;
    }

    public bool GetDword(out uint data)
    {
        data = 0;
        var size = sizeof(uint);
        if (this._pos + size > this._size)
        {
            this.Error("GetDword error {0} + {1}/{2}", this._pos, size, this._size);
            return false;
        }

        /*
        string pc = m_bin + m_pos;
        m_pos += size;
        data = (uint)pc;
        */
        return true;
    }

    public bool GetNum(out long data)
    {
        data = 0;
        const int size = sizeof(long);
        if (this._pos + size > this._size)
        {
            this.Error("GetNum error {0} + {1}/{2}", this._pos, size, this._size);
            return false;
        }

        data = this._bin[this._pos] | (this._bin[this._pos + 1] << 8) | (this._bin[this._pos + 2] << 16) |
               (this._bin[this._pos + 3] << 24) | (this._bin[this._pos + 4] << 32) | (this._bin[this._pos + 5] << 40) |
               (this._bin[this._pos + 6] << 48) | (this._bin[this._pos + 7] << 56);
        this._pos += size;
        return true;
    }

    public bool GetStr(out string str)
    {
        str = "";
        if (!this.GetWord(out var size))
        {
            this.Error("GetNum error {0} + {1}/{2}", this._pos, size, this._size);
            return false;
        }

        if (this._pos + size > this._size)
        {
            return false;
        }

        str       =  Encoding.UTF8.GetString(this._bin, (int)this._pos, size - 1);
        this._pos += size;
        return true;
    }

    public long GetCurPos()
    {
        return this._pos;
    }
}