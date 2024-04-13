using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using TMPro;

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
    public static Player Instance;

    public RuneVisuals RuneVisualPrefab;
    public List<RuneRef> BaseDeck;

    private List<Rune> bag = new();
    [SerializeField]
    private List<Rune> hand = new();
    private List<Rune> circle = new(new Rune[NumSlots]);
    private List<Rune> discardPile = new();

    private RuneBoard runeBoard;


    public bool CircleIsFull => circle.All(rune => rune != null);
    public Rune GetRuneInCircle(int index) => circle[CircularIndex(index)];
    public int GetIndexOfRune(Rune rune) => circle.IndexOf(rune);
    public int GetRunePower(int runeIndex)
    {
        runeIndex = CircularIndex(runeIndex);
        Rune rune = circle[runeIndex];
        int runePower = rune.Power;

        for (int i = 0; i < NumSlots; i++)
        {
            Rune other = circle[i];
            if (other != null && other.Aura.IsValid)
            {
                if (other.Aura.Application.Invoke(runeIndex, i, this))
                {
                    runePower += other.Aura.Power;
                }
            }
        }

        return runePower;
    }

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
    private void TryPlace(int runeIndex, int slot)
    {
        if (CircleIsFull || runeIndex < 0 || runeIndex >= hand.Count || circle[slot] != null)
            return;

        Rune rune = hand[runeIndex];
        hand.Remove(rune);
        Place(rune, slot);
    }

    public void Place(Rune rune, int slot)
    {
        circle[slot] = rune;
        rune.OnEnter?.Invoke(slot, this);
    }

    private void ResolveCircle()
    {
        int power = 0;
        for (int i = 0; i < circle.Count; i++)
        {
            if (circle[i] == null)
                continue;

            power += GetRunePower(i);
        }

        Debug.Log($"DEALING DAMAGE: {power}");

        ClearCircle();
    }

    private void ClearCircle()
    {
        for (int i = 0; i < circle.Count; i++)
        {
            if (circle[i] != null)
            {
                discardPile.Add(circle[i]);
                circle[i] = null;
            }
        }
    }

    private IEnumerator DrawHand()
    {
        while (bag.Count > 0 && hand.Count < HandSize)
        {
            Rune rune = bag[0];
            hand.Add(rune);
            bag.RemoveAt(0);

            RuneVisuals runeVisual = Instantiate(RuneVisualPrefab);
            runeVisual.Init(rune, this);
            runeBoard.AddRune(runeVisual);

            runeVisual.transform.position = new Vector3(
                UnityEngine.Random.Range(-0.5f, 0.5f),
                1.0f,
                UnityEngine.Random.Range(-2.5f, -1.5f));
            var rigidBody = runeVisual.GetComponent<Rigidbody>();
            rigidBody.AddForce(UnityEngine.Random.onUnitSphere, ForceMode.VelocityChange);

            yield return new WaitForSeconds(0.2f);
        }
    }

    private void Awake()
    {
        Instance = this;
        runeBoard = FindObjectOfType<RuneBoard>();
    }

    private void Start()
    {
        foreach (RuneRef runeRef in BaseDeck)
        {
            Rune rune = runeRef.Get();
            bag.Add(rune);
        }

        bag.Shuffle();
        StartCoroutine(DrawHand());
    }

    private int? state = null;
    public List<TextMeshProUGUI> SlotTexts;
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            if (state == null)
                state = 0;
            else
            {
                TryPlace(state.Value, 0);
                state = null;
            }
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            if (state == null)
                state = 1;
            else
            {
                TryPlace(state.Value, 1);
                state = null;
            }
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            if (state == null)
                state = 2;
            else
            {
                TryPlace(state.Value, 2);
                state = null;
            }
        }
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            if (state == null)
                state = 3;
            else
            {
                TryPlace(state.Value, 3);
                state = null;
            }
        }
        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            if (state == null)
                state = 4;
            else
            {
                TryPlace(state.Value, 4);
                state = null;
            }
        }

        for (int i = 0; i < SlotTexts.Count; i++)
        {
            if (circle[i] != null)
                SlotTexts[i].text = $"{circle[i].Name}: {GetRunePower(i)}";
            else
                SlotTexts[i].text = "...";
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            ResolveCircle();
            StartCoroutine(DrawHand());
        }
    }
}
