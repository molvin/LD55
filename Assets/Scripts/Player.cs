using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public enum Location
{
    None,
    Bag,
    Hand,
    Circle,
}

public class Player : MonoBehaviour
{
    const int NumSlots = 5;
    const int HandSize = 5;
    public static int CircularIndex(int index) => index < 0 ? NumSlots + index : index >= NumSlots ? index - NumSlots : index;

    public List<RuneRef> BaseDeck;

    private List<Rune> bag = new();
    [SerializeField]
    private List<Rune> hand = new();
    private List<Rune> circle = new(new Rune[NumSlots]);


    public bool CircleIsFull => circle.All(rune => rune != null);
    public Rune GetRuneInCircle(int index) => circle[CircularIndex(index)];
    public void ForEach(Location location, Action<Rune> action) {
        List<Rune> collection = new();
        switch (location)
        {
            case Location.None:
                return;
            case Location.Bag:
                collection = bag;
                break;
            case Location.Hand:
                collection = hand;
                break;
            case Location.Circle:
                collection = circle;
                break;
        }

        foreach (Rune rune in collection)
            if (rune != null)
                action(rune);
    }
    private void TryPlace(int runeIndex)
    {
        if (CircleIsFull || runeIndex < 0 || runeIndex >= hand.Count)
            return;

        Rune rune = hand[runeIndex];
        hand.Remove(rune);
        Place(rune);

        DebugPrint();
    }

    public void Place(Rune rune, int? slot = null)
    {
        if (slot == null)
        {
            for (int i = 0; i < NumSlots; i++)
            {
                if (circle[i] == null)
                {
                    slot = i;
                    break;
                }
            }
        }

        circle[slot.Value] = rune;
        rune.OnEnter?.Invoke(slot.Value, this);
    }

    void DebugPrint()
    {
        Debug.Log("SUMMON CIRCLE");
        ForEach(Location.Circle, rune => Debug.Log($"{rune.Name}: {rune.Power}"));
        Debug.Log("------------");
    }

    private void Start()
    {
        foreach (RuneRef runeRef in BaseDeck)
        {
            Rune rune = runeRef.Get();
            bag.Add(rune);
        }

        bag.Shuffle();

        for (int i = 0; i < HandSize && bag.Count > 0; i++)
        {
            hand.Add(bag[0]);
            bag.RemoveAt(0);
        }
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
