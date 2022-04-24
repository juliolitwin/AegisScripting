using System.IO;
using System.Text;

namespace Scripting.Aegis;

public class Bin
{
    private BinaryWriter _bw = default!;
    private FileStream   _fs = default!;

    public void Seek(int pos)
    {
        this._fs.Seek(pos, SeekOrigin.Begin);
    }

    public bool Set(string fName)
    {
        if (this._fs != null)
        {
            this._fs.Close();
        }

        this._fs = new FileStream(fName, FileMode.Create, FileAccess.Write);
        this._bw = new BinaryWriter(this._fs);
        if (this._fs == null)
        {
            return false;
        }

        return true;
    }

    public int GetPos()
    {
        if (this._fs == null)
        {
            return 0;
        }

        return (int)this._fs.Position;
    }

    public void WriteNum(long n)
    {
        if (this._fs == null)
        {
            return;
        }

        this._bw.Write(n);
    }

    public void WriteStr(string str)
    {
        if (this._fs == null)
        {
            return;
        }

        var len = Encoding.UTF8.GetByteCount(str) + 1;
        this.WriteWord((short)len);
        this._bw.Write(Encoding.UTF8.GetBytes(str));
        this._bw.Write((byte)0);
    }

    public void WriteByte(byte n)
    {
        if (this._fs == null)
        {
            return;
        }

        this._bw.Write(n);
    }

    public void WriteByte(char n)
    {
        if (this._fs == null)
        {
            return;
        }

        this._bw.Write(n);
    }

    public void WriteWord(short n)
    {
        if (this._fs == null)
        {
            return;
        }

        this._bw.Write(n);
    }

    public void WriteDword(long n)
    {
        if (this._fs == null)
        {
            return;
        }

        this._bw.Write(n);
    }

    public void Release()
    {
        try
        {
            this._bw?.Flush();
            this._fs?.Flush();
            this._fs?.Close();
        }
        catch { }
    }
}