using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public struct TempStats
{
    public int Power;
    public int Multiplier;

    public static TempStats operator+(TempStats first, TempStats second)
    {
        TempStats stats;
        stats.Power = first.Power + second.Power;
        stats.Multiplier = first.Multiplier + second.Multiplier;
        return stats;
    }
}

public class Player : MonoBehaviour
{
    public static int CircularIndex(int index) => index < 0 ? Settings.NumSlots + index : index >= Settings.NumSlots ? index - Settings.NumSlots : index;
    public static Player Instance;

    public RuneVisuals RuneVisualPrefab;
    public List<RuneRef> BaseDeck;

    private List<Rune> deckRef = new();
    private List<Rune> bag = new();
    private List<Rune> hand = new();
    private List<Rune> circle = new(new Rune[Settings.NumSlots]);
    private List<Rune> discardPile = new();
    private Dictionary<Rune, TempStats> temporaryStats = new();
    private int circlePower;
    private int health = Settings.PlayerMaxHealth;

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
        int runeMultiplier = stats.Multiplier;

        for (int i = 0; i < Settings.NumSlots; i++)
        {
            Rune other = circle[i];
            if (other != null && other.Aura.IsValid)
            {
                if (other.Aura.Application.Invoke(runeIndex, i, this))
                {
                    runePower += other.Aura.Power;
                    runeMultiplier += other.Aura.Multiplier;
                }
            }
        }

        return runePower * Mathf.Max(runeMultiplier, 1);
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

    public void Restart()
    {
        bag.Clear();
        hand.Clear();
        circle = new(new Rune[Settings.NumSlots]);
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
        StartCoroutine(Game());
    }

    private IEnumerator Game()
    {
        while(health > 0)
        {
            Draw();
            yield return runeBoard.Draw(hand);
            yield return runeBoard.Play();

            for (int i = 0; i < circle.Count; i++)
            {
                if (circle[i] == null)
                    continue;

                int power = GetRunePower(i);
                circlePower += power;

                yield return runeBoard.Resolve(i, power, circlePower);
            }

            Debug.Log($"DEALING DAMAGE: {circlePower}");
            ClearCircle();

            yield return runeBoard.EndRound();

            // TODO: check if opponent died, if so do shopping round, else continue
            //       if opponent is still alive he should do damage to the player
        }

        Debug.Log("You lose");

        yield return null;
    }

    private void Draw()
    {
        while (hand.Count < Settings.HandSize)
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
        }
    }
}
