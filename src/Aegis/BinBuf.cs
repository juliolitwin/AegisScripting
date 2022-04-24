using System;
using System.IO;

namespace Scripting.Aegis;

public class BinBuf
{
    private byte[] _buf = default!;
    private int    _size;

    /// <summary>
    ///     Initializes a new instance of the <see cref="BinBuf" /> class.
    ///     Constructor.
    /// </summary>
    public BinBuf()
    {
        this._buf = default(byte[])!;
        this.Clear();
    }

    public DateTime LastModified { get; private set; }

    /// <summary>
    ///     Finalizes an instance of the <see cref="BinBuf" /> class.
    /// </summary>
    ~BinBuf()
    {
        this.Clear();
    }

    public byte[] GetBase()
    {
        return this._buf;
    }

    public int GetSize()
    {
        return this._size;
    }

    public void Clear()
    {
        if (this._buf != null)
        {
            this._buf = default(byte[])!;
        }

        this._size = 0;
    }

    public bool Load(string fName)
    {
        this._buf  = File.ReadAllBytes(fName);
        this._size = this._buf.Length;

        this.LastModified = File.GetLastWriteTime(fName);
        return true;
    }

    public static BinBuf CreateAndLoad(string fileName)
    {
        var binBuf = new BinBuf();
        binBuf.Load(fileName);

        return binBuf;
    }
}