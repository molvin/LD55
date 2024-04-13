using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class RuneRef
{
    // Index into Runes.GetAllRunes()
    public int Index;

    public Rune Get()
    {
        return Index == 0 ? null : Runes.GetAllRunes()[Index - 1];
    }
}
