using System.Collections.Generic;

namespace Scripting.Aegis;

public readonly struct VarMap
{
    private const string DefaultNullVar = "NULL";

    /// <summary>
    ///     Gets the variable map.
    /// </summary>
    public IDictionary<string, TokenData> Map { get; } = new SortedDictionary<string, TokenData>();

    /// <summary>
    ///     Initializes a new instance of the <see cref="VarMap" /> struct.
    ///     Constructor.
    /// </summary>
    public VarMap()
    {
        // It is necessary to clear the dictionary when it is instantiated
        // but by default 'NULL' is added after clearing. 
        this.Clear();
    }

    /// <summary>
    ///     Check if map is empty.
    /// </summary>
    /// <returns></returns>
    public bool IsEmpty()
    {
        return this.Map.Count == 0;
    }

    /// <summary>
    ///     Clear the map and add the default 'NULL'.
    /// </summary>
    public void Clear()
    {
        this.Map.Clear();
        this.Map[DefaultNullVar] = TokenData.Null;
    }

    /// <summary>
    ///     Set the data in the map.
    /// </summary>
    /// <param name="str"></param>
    /// <param name="t"></param>
    public void Set(string str, TokenData t)
    {
        this.Map[str] = t;
    }

    /// <summary>
    ///     Get the data from the map.
    /// </summary>
    /// <param name="str"></param>
    /// <param name="t"></param>
    /// <returns></returns>
    public bool Get(string str, ref TokenData t)
    {
        if (!this.Map.ContainsKey(str))
        {
            return false;
        }

        t = this.Map[str];
        return true;
    }

    /// <summary>
    ///     Check if exist in the map.
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public bool IsExist(string str)
    {
        return this.Map.ContainsKey(str);
    }
}