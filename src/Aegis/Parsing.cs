using System.Collections.Generic;
using Nito.Collections;

namespace Scripting.Aegis;

public class Parsing
{
    private readonly Deque<string> _phase = new();

    public int GetNum()
    {
        return this._phase.Count;
    }

    public bool Get(out string line)
    {
        line = "";

        if (this._phase.Count == 0)
        {
            return false;
        }

        // Copy the first.
        line = this._phase[0];

        // Remove the first.
        this._phase.RemoveFromFront();
        return true;
    }

    public string Get(int pos)
    {
        return this._phase[pos];
    }

    public bool Run(string line, string reg)
    {
        this._phase.Clear();

        Stack<string> phase   = new();
        var           isQuote = false;

        var depth  = 0;
        var regPos = 0;

        var currentPhase = "";

        // Process the all characters of the line.
        for (var index = 0; index < line.Length; index++)
        {
            var currentCharacter = line[index];
            if (isQuote)
            {
                if (currentCharacter == '"')
                {
                    // Found the end quote.
                    isQuote = false;
                }

                currentPhase += currentCharacter;
                continue;
            }

            switch (currentCharacter)
            {
                case '"':
                {
                    currentPhase += currentCharacter;
                    isQuote      =  true;
                    break;
                }

                case '(':
                {
                    currentPhase += " ";
                    currentPhase += $"{reg}{regPos}";
                    phase.Push(currentPhase);

                    currentPhase = $"{reg}{regPos}=";
                    regPos++;
                    depth++;

                    break;
                }

                case ')':
                {
                    if (depth <= 0)
                    {
                        return false;
                    }

                    this._phase.AddToBack(currentPhase);
                    currentPhase = phase.Peek();
                    phase.Pop();
                    depth--;

                    break;
                }

                default:
                {
                    currentPhase += currentCharacter;
                    break;
                }
            }
        }

        this._phase.AddToBack(currentPhase);
        return depth == 0;
    }
}