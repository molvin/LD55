using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Player : MonoBehaviour
{
    public List<RuneRef> Hand;

    private SummonCircle circle = new();


    private void TryPlace(int runeIndex)
    {
        if (circle.IsFull || runeIndex < 0 || runeIndex >= Hand.Count || Hand[runeIndex].Get() == null)
            return;

        Rune rune = Hand[runeIndex].Get();
        circle.Place(rune);

        DebugPrint();
    }

    void DebugPrint()
    {
        Debug.Log("SUMMON CIRCLE");
        circle.ForEach(rune => Debug.Log($"{rune.Name}: {rune.Power}"));
        Debug.Log("------------");
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            TryPlace(0);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            TryPlace(1);
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            TryPlace(2);
        }
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            TryPlace(3);
        }
        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            TryPlace(4);
        }
        if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            TryPlace(5);
        }
        if (Input.GetKeyDown(KeyCode.Alpha7))
        {
            TryPlace(6);
        }
        if (Input.GetKeyDown(KeyCode.Alpha8))
        {
            TryPlace(7);
        }
        if (Input.GetKeyDown(KeyCode.Alpha9))
        {
            TryPlace(8);
        }
    }
}
