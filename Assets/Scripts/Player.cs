using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using System.Runtime.InteropServices.WindowsRuntime;

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

    private RuneBoard runeBoard;


    public bool AreNeighbours(int first, int second) => CircularIndex(first + 1) == second || CircularIndex(first - 1) == second;
    public bool AreOpposites(int first, int second) => first != second && !AreNeighbours(first, second);
    public bool CircleIsFull => circle.All(rune => rune != null);
    public bool HasRuneAtIndex(int index) => circle[CircularIndex(index)] != null;
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
            if (other != null && other.Aura != null && other.Aura.Count > 0)
            {
                foreach (Aura aura in other.Aura)
                {
                    if (aura.Application.Invoke(runeIndex, i, this))
                    {
                        runePower += aura.Power;
                        runeMultiplier += aura.Multiplier;
                    }
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
    public void AddCirclePower(int value)
    {
        circlePower += value;
    }

    public void Place(Rune rune, int slot)
    {
        hand.Remove(rune);
        circle[slot] = rune;
        rune.OnEnter?.Invoke(slot, this);
    }

    public void Remove(int index)
    {
        index = CircularIndex(index);
        if (circle[index] == null)
            return;

        runeBoard.DestroySlot(index);
        Discard(circle[index]);
        circle[index] = null;
    }
    public void Swap(Rune rune, int index)
    {
        index = CircularIndex(index);
        temporaryStats.Add(rune, new());
        Discard(GetRuneInCircle(index));
        runeBoard.SwapSlot(rune, index);
        circle[index] = rune;
    }

    private void ClearCircle()
    {
        for (int i = 0; i < circle.Count; i++)
        {
            if (circle[i] != null)
            {
                Discard(circle[i]);
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
        int currentRound = 0;
        int health = Settings.PlayerMaxHealth;
        int opponentHealth = Settings.GetOpponentHealth(currentRound);
        int set = 0;

        while (health > 0)
        {
            HUD.Instance.PlayerHealth.Set(health, Settings.PlayerMaxHealth);
            HUD.Instance.OpponentHealth.Set(opponentHealth, Settings.GetOpponentHealth(currentRound));

            Draw(true);
            yield return runeBoard.Draw(hand);
            yield return runeBoard.Play();

            for (int i = 0; i < circle.Count; i++)
            {
                if (circle[i] == null)
                    continue;

                Activate(i);

                yield return runeBoard.Resolve(i, GetRunePower(i), circlePower);
            }

            Debug.Log($"DEALING DAMAGE: {circlePower}");
            opponentHealth -= circlePower;
            HUD.Instance.OpponentHealth.Set(opponentHealth, Settings.GetOpponentHealth(currentRound));

            for (int i = 0; i < circle.Count; i++)
            {
                if (circle[i] != null)
                    runeBoard.DestroySlot(i);
            }

            yield return new WaitForSeconds(1.0f);

            ClearCircle();

            yield return runeBoard.EndRound();

            if (opponentHealth <= 0)
            {
                set = 0;
                currentRound++;
                Debug.Log("You defeated opponent!");
                yield return new WaitForSeconds(1.0f);
                
                yield return runeBoard.Draw(deckRef);
                yield return runeBoard.Shop();
                yield return runeBoard.EndRound();

                opponentHealth = Settings.GetOpponentHealth(currentRound);
                HUD.Instance.OpponentHealth.Set(opponentHealth, Settings.GetOpponentHealth(currentRound));
            }
            else
            {
                int damage = Settings.GetOpponentDamage(set);
                health -= damage;
                set++;
                HUD.Instance.PlayerHealth.Set(health, Settings.PlayerMaxHealth);
                Debug.Log($"TAKING DAMAGE: {damage}");

                yield return new WaitForSeconds(1.0f);
            }
        }

        Debug.Log("You lose");

        yield return null;
    }
    public void Activate(int index)
    {
        index = CircularIndex(index);
        if (circle[index] == null)
            return;

        int power = GetRunePower(index);
        circlePower += power;

        circle[index].OnActivate?.Invoke(index, this);
    }
    public void AddNewRuneToHand(Rune rune)
    {
        temporaryStats.Add(rune, new());
        hand.Add(rune);
        // TODO: Put in the main routine!
        StartCoroutine(runeBoard.Draw(rune));
    }
    private void Draw(bool discard)
    {
        if (discard)
        {
            foreach (Rune rune in hand)
            {
                Discard(rune);
            }
            hand.Clear();
        }
        Draw(Settings.HandSize - hand.Count);
    }
    public void Draw(int count)
    {
        for (int i = 0; i < count; i++)
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
    private void Discard(Rune rune)
    {
        if (rune.Rarity != Rarity.None)
        {
            discardPile.Add(rune);
        }
    }
}
