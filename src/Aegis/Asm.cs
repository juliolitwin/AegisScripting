using System.IO;
using System.Text;

namespace Scripting.Aegis;

public class Asm
{
    private Stream       _fs = default!;
    private StreamWriter _sw = default!;

    /// <summary>
    ///     Initializes a new instance of the <see cref="Asm" /> class.
    /// </summary>
    public Asm()
    {
        this._sw = default(StreamWriter)!;
        this._fs = default(Stream)!;
    }

    /// <summary>
    ///     Finalizes an instance of the <see cref="Asm" /> class.
    /// </summary>
    ~Asm()
    {
        this.Release();
    }

    public void Line(int addr)
    {
        this.Putf("\n{0:x}: ", addr);
    }

    public void Put(string str)
    {
        this._sw?.Write(str);
        this._sw?.Flush();
        this._fs?.Flush();
    }

    public void Putf(string str, params object[] args)
    {
        if (this._fs == null)
        {
            return;
        }

        this.Put(string.Format(str, args));
    }

    public void Error(string str)
    {
        this.Put("\n// Error: ");
        this.Put(str);
    }

    public void Errorf(string str, params object[] args)
    {
        if (this._fs == null)
        {
            return;
        }

        this.Error(string.Format(str, args));
    }

    public void Comment(string str)
    {
        this.Put("\n// ");
        this.Put(str);
    }

    public void Commentf(string str, params object[] args)
    {
        if (this._fs == null)
        {
            return;
        }

        this.Comment(string.Format(str, args));
    }

    public bool Set(string fName)
    {
        if (this._fs != null)
        {
            this._sw.Flush();
            this._fs.Flush();
            this._fs.Close();
        }

        this._fs = new FileStream(fName, FileMode.Create, FileAccess.Write);
        this._sw = new StreamWriter(this._fs, Encoding.UTF8);

        if (this._fs == null)
        {
            return false;
        }

        return true;
    }

    public bool Set(Stream s)
    {
        this._fs = s;
        this._sw = new StreamWriter(this._fs, Encoding.UTF8);
        if (this._fs == null)
        {
            return false;
        }

        return true;
    }

    public void Release()
    {
        this._sw?.Flush();

        this._fs?.Flush();
        this._sw?.Close();
        this._fs?.Close();

        this._fs?.Dispose();
        this._sw?.Dispose();

        this._sw = default(StreamWriter?)!;
        this._fs = default(Stream)!;
    }
}