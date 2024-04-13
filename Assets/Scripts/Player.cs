using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public struct TempStats
{
    public int Power;

    public static TempStats operator+(TempStats first, TempStats second)
    {
        TempStats stats;
        stats.Power = first.Power + second.Power;
        return stats;
    }
}

public class Player : MonoBehaviour
{
    const int NumSlots = 5;
    const int HandSize = 5;
    public static int CircularIndex(int index) => index < 0 ? NumSlots + index : index >= NumSlots ? index - NumSlots : index;
    public static Player Instance;

    public RuneVisuals RuneVisualPrefab;
    public List<RuneRef> BaseDeck;

    private List<Rune> deckRef = new();
    private List<Rune> bag = new();
    private List<Rune> hand = new();
    private List<Rune> circle = new(new Rune[NumSlots]);
    private List<Rune> discardPile = new();
    private Dictionary<Rune, TempStats> temporaryStats = new();
    private int circlePower;

    private RuneBoard runeBoard;


    public bool CircleIsFull => circle.All(rune => rune != null);
    public Rune GetRuneInCircle(int index) => circle[CircularIndex(index)];
    public int GetCirclePower() => circlePower;
    public int GetIndexOfRune(Rune rune) => circle.IndexOf(rune);
    public int GetRunePower(int runeIndex)
    {
        runeIndex = CircularIndex(runeIndex);
        Rune rune = circle[runeIndex];
        TempStats stats = temporaryStats[rune];
        int runePower = rune.Power + stats.Power;

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

    public void AddStats(Rune rune, TempStats stats)
    {
        TempStats current = temporaryStats[rune];
        current += stats;
        temporaryStats[rune] = current;
    }

    public void Place(Rune rune, int slot)
    {
        hand.Remove(rune);
        circle[slot] = rune;
        rune.OnEnter?.Invoke(slot, this);
    }

    private IEnumerator ResolveCircle()
    {
        for (int i = 0; i < circle.Count; i++)
        {
            if (circle[i] == null)
                continue;

            circlePower += GetRunePower(i);
            yield return new WaitForSeconds(0.3f);
            runeBoard.RemoveRune(circle[i]);
            yield return new WaitForSeconds(0.3f);
        }

        Debug.Log($"DEALING DAMAGE: {circlePower}");

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

        circlePower = 0;
    }

    private IEnumerator DrawHand()
    {
        while (hand.Count < HandSize)
        {
            if (bag.Count == 0)
            {
                foreach (Rune r in discardPile)
                {
                    bag.Add(r);
                }
                discardPile.Clear();
                bag.Shuffle();
            }

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

    public void Restart()
    {
        bag.Clear();
        hand.Clear();
        circle = new(new Rune[NumSlots]);
        discardPile.Clear();
        temporaryStats.Clear();

        circlePower = 0;

        foreach (Rune rune in deckRef)
        {
            bag.Add(rune);
            temporaryStats.Add(rune, new());
        }

        bag.Shuffle();
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
            deckRef.Add(rune);
        }

        Restart();
        StartCoroutine(DrawHand());
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            foreach (Rune rune in hand)
            {
                runeBoard.RemoveRune(rune);
                discardPile.Add(rune);
            }
            hand.Clear();

            StartCoroutine(EndStep());
        }
    }

    private IEnumerator EndStep()
    {
        yield return ResolveCircle();
        yield return DrawHand();
    }
}
