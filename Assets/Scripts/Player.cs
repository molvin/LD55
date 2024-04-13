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

    public void Remove(int index)
    {
        index = CircularIndex(index);
        if (circle[index] == null)
            return;

        runeBoard.DestroySlot(index);
        discardPile.Add(circle[index]);
        circle[index] = null;
    }
    
    public void RemoveFromDeck(Rune rune)
    {
        deckRef.Remove(rune);
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
        int currentRound = 0;
        int health = Settings.PlayerMaxHealth;
        int opponentHealth = Settings.GetOpponentHealth(currentRound);
        int set = 0;

        while (health > 0)
        {
            Restart();

            HUD.Instance.PlayerHealth.Set(health, Settings.PlayerMaxHealth);
            HUD.Instance.OpponentHealth.Set(opponentHealth, Settings.GetOpponentHealth(currentRound));

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
            opponentHealth -= circlePower;
            HUD.Instance.OpponentHealth.Set(opponentHealth, Settings.GetOpponentHealth(currentRound));
            yield return new WaitForSeconds(1.0f);

            ClearCircle();

            yield return runeBoard.EndRound();

            if (opponentHealth <= 0)
            {
                set = 0;
                currentRound++;
                Debug.Log("You defeated opponent!");
                yield return new WaitForSeconds(1.0f);

                Restart();
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

    private void Draw()
    {
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

            if(bag.Count > 0)
            {
                Rune rune = bag[0];
                hand.Add(rune);
                bag.RemoveAt(0);
            }
            else
            {
                Debug.LogWarning("Your deck is too small");
            }
        }
    }
}
