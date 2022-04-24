// <copyright file="StringFunctions.cs" company="Poring">
// Copyright (c) Poring. All rights reserved.
// </copyright>

using System;

namespace Scripting.Utilities;

public static class StringFunctions
{
    //------------------------------------------------------------------------------------
    // This method replicates the classic C string function 'strtok' (and 'wcstok').
    // Note that the .NET string 'Split' method cannot be used to replicate 'strtok' since
    // it doesn't allow changing the delimiters between each token retrieval.
    //------------------------------------------------------------------------------------
    private static string _activeString = "";

    private static int _activePosition;

    //------------------------------------------------------------------------------------
    // This method allows replacing a single character in a string, to help convert
    // C++ code where a single character in a character array is replaced.
    //------------------------------------------------------------------------------------
    public static string ChangeCharacter(string sourceString, int charIndex, char newChar)
    {
        return (charIndex             > 0 ? sourceString.Substring(0, charIndex) : "")
               + newChar + (charIndex < sourceString.Length - 1 ? sourceString.Substring(charIndex + 1) : "");
    }

    //------------------------------------------------------------------------------------
    // This method replicates the classic C string function 'isxdigit' (and 'iswxdigit').
    //------------------------------------------------------------------------------------
    public static bool IsXDigit(char character)
    {
        if (char.IsDigit(character))
        {
            return true;
        }

        if ("ABCDEFabcdef".IndexOf(character) > -1)
        {
            return true;
        }

        return false;
    }

    //------------------------------------------------------------------------------------
    // This method replicates the classic C string function 'strchr' (and 'wcschr').
    //------------------------------------------------------------------------------------
    public static string StrChr(string stringToSearch, char charToFind)
    {
        var index = stringToSearch.IndexOf(charToFind);
        if (index > -1)
        {
            return stringToSearch.Substring(index);
        }

        return default(string)!;
    }

    /// <summary>
    ///     Function: Strchr
    ///     Returns the first occurrence of Character to be located
    ///     in string or null otherwise
    /// </summary>
    /// <param name="originalString"></param>
    /// <param name="valueToSearch"></param>
    /// <returns></returns>
    public static int Strchr(string originalString, char valueToSearch)
    {
        if (string.IsNullOrEmpty(originalString))
        {
            return -1;
        }

        return originalString.IndexOf(valueToSearch, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    ///     Function: Strchr
    ///     Returns the first occurrence of Character to be located
    ///     in string or null otherwise
    /// </summary>
    /// <param name="originalString"></param>
    /// <param name="valueToSearch"></param>
    /// <returns></returns>
    public static int Strchr(string originalString, string valueToSearch)
    {
        if (string.IsNullOrEmpty(originalString) ||
            string.IsNullOrEmpty(valueToSearch))
        {
            return -1;
        }

        if (valueToSearch.Length > originalString.Length)
        {
            return -1;
        }

        return originalString.IndexOf(valueToSearch, StringComparison.OrdinalIgnoreCase);
    }

    //------------------------------------------------------------------------------------
    // This method replicates the classic C string function 'strrchr' (and 'wcsrchr').
    //------------------------------------------------------------------------------------
    public static string StrRChr(string stringToSearch, char charToFind)
    {
        var index = stringToSearch.LastIndexOf(charToFind);
        if (index > -1)
        {
            return stringToSearch.Substring(index);
        }

        return default(string)!;
    }

    //------------------------------------------------------------------------------------
    // This method replicates the classic C string function 'strstr' (and 'wcsstr').
    //------------------------------------------------------------------------------------
    public static string StrStr(string stringToSearch, string stringToFind)
    {
        var index = stringToSearch.IndexOf(stringToFind);
        if (index > -1)
        {
            return stringToSearch.Substring(index);
        }

        return default(string)!;
    }

    public static string StrTok(string stringToTokenize, string delimiters)
    {
        if (stringToTokenize != null)
        {
            _activeString   = stringToTokenize;
            _activePosition = -1;
        }

        // the stringToTokenize was never set:
        if (_activeString is null)
        {
            return default(string)!;
        }

        // all tokens have already been extracted:
        if (_activePosition == _activeString.Length)
        {
            return default(string)!;
        }

        // bypass delimiters:
        _activePosition++;
        while (_activePosition                                    < _activeString.Length &&
               delimiters.IndexOf(_activeString[_activePosition]) > -1)
        {
            _activePosition++;
        }

        // only delimiters were left, so return null:
        if (_activePosition == _activeString.Length)
        {
            return default(string)!;
        }

        // get starting position of string to return:
        var startingPosition = _activePosition;

        // read until next delimiter:
        do
        {
            _activePosition++;
        }
        while (_activePosition                                    < _activeString.Length &&
               delimiters.IndexOf(_activeString[_activePosition]) == -1);

        return _activeString.Substring(startingPosition, _activePosition - startingPosition);
    }
}