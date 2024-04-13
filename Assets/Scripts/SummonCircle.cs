using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;

public class SummonCircle
{
    const int NumSlots = 5;
    public static int CircularIndex(int index) => index < 0 ? NumSlots + index : index >= NumSlots ? index - NumSlots : index;

    private Rune[] slots = new Rune[NumSlots];

    public bool IsFull => slots.All(rune => rune != null);
    public Rune GetRune(int index) => slots[CircularIndex(index)];
    public void ForEach(Action<Rune> action) {
        foreach (Rune rune in slots)
            if (rune != null)
                action(rune);
    }

    public void Place(Rune rune, int? slot = null)
    {
        if (slot == null)
        {
            for (int i = 0; i < NumSlots; i++)
            {
                if (slots[i] == null)
                {
                    slot = i;
                    break;
                }
            }
        }

        slots[slot.Value] = rune;
        rune.OnEnter?.Invoke(slot.Value, this);
    }
}
