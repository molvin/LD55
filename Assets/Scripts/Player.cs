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
    public static int CircularIndex(int index) => index < 0 ? Settings.NumSlots + index : index >= Settings.NumSlots ? index - Settings.NumSlots : index;
    public static Player Instance;

    public List<RuneRef> BaseDeck;
    public bool UseStarters;
    private int health;
    public int Health => health;

    private List<Rune> deckRef = new();
    private List<Rune> bag = new();
    private List<Rune> hand = new();
    private List<Rune> circle = new(new Rune[Settings.NumSlots]);
    private List<Rune> discardPile = new();
    private Dictionary<Rune, TempStats> temporaryStats = new();
    private int circlePower;
    private int circlePowerPromise;

    private RuneBoard runeBoard;


    public bool AreNeighbours(int first, int second) => CircularIndex(first + 1) == second || CircularIndex(first - 1) == second;
    public bool AreOpposites(int first, int second) => first != second && !AreNeighbours(first, second);
    public bool CircleIsFull => circle.All(rune => rune != null);
    public bool HasRuneAtIndex(int index) => circle[CircularIndex(index)] != null;
    public Rune GetRuneInCircle(int index) => circle[CircularIndex(index)];
    public int GetCirclePower() => circlePower;
    public int GetIndexOfRune(Rune rune) => circle.IndexOf(rune);
    public int RunesInCircle() => circle.Sum(r => r != null ? 1 : 0);
    public int GetRunePower(int runeIndex)
    {
        runeIndex = CircularIndex(runeIndex);
        Rune rune = circle[runeIndex];
        TempStats stats = temporaryStats[rune];
        int runePower = rune.Power + stats.Power;

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
                    }
                }
            }
        }

        return runePower;
    }

    public void AddStats(int index, TempStats stats)
    {
        Rune rune = circle[index];
        AddStats(rune, stats);
    }
    public void AddStats(Rune rune, TempStats stats)
    {
        TempStats current = temporaryStats[rune];
        current += stats;
        temporaryStats[rune] = current;
    }
    public void MultiplyPower(int index, float multiplier)
    {
        Rune rune = GetRuneInCircle(index);
        TempStats stats = temporaryStats[rune];
        int auraPower = 0;

        for (int i = 0; i < Settings.NumSlots; i++)
        {
            Rune other = circle[i];
            if (other != null && other.Aura != null && other.Aura.Count > 0)
            {
                foreach (Aura aura in other.Aura)
                {
                    if (aura.Application.Invoke(index, i, this))
                    {
                        auraPower += aura.Power;
                    }
                }
            }
        }
        int totalPower = rune.Power + stats.Power + auraPower;
        stats.Power = Mathf.RoundToInt(totalPower * multiplier) - (rune.Power + auraPower);
        temporaryStats[rune] = stats;
    }
    public void AddCirclePower(int value)
    {
        circlePower += value;
    }
    public void AddCirclePowerPromise(int value)
    {
        circlePowerPromise += value;
    }
    public void AddLife(int value)
    {
        health += value;
    }
    public List<EventHistory> Place(Rune rune, int slot)
    {
        hand.Remove(rune);
        circle[slot] = rune;
        return Trigger(TriggerType.OnEnter, slot);
    }

    public bool Remove(int index)
    {
        index = CircularIndex(index);
        if (circle[index] == null)
            return false;

        Trigger(TriggerType.OnDestroy, index);
        Discard(circle[index]);
        circle[index] = null;
        return true;
    }
    public bool Exile(int index)
    {
        index = CircularIndex(index);
        if (circle[index] == null)
            return false;

        Trigger(TriggerType.OnExile, index);
        deckRef.Remove(circle[index]);
        circle[index] = null;
        return true;
    }
    public List<EventHistory> Trigger(TriggerType trigger, int index)
    {
        List<EventHistory> history = new();
        switch (trigger)
        {
            case TriggerType.OnEnter:
            {
                history = circle[index].OnEnter?.Invoke(index, this);
            } break;
            case TriggerType.OnActivate:
            {
                history = circle[index].OnActivate?.Invoke(index, this);
            } break;
            case TriggerType.OnDestroy:
            {
                history = circle[index].OnDestroy?.Invoke(index, this);
            } break;
            case TriggerType.OnExile:
            { 
                history = circle[index].OnExile?.Invoke(index, this);
            } break;
        }

        for (int i = 0; i < 5; i++)
        {
            if (circle[i] != null)
            {
                if (circle[i].OnOtherRuneTrigger != null)
                {
                    history.AddRange(circle[i].OnOtherRuneTrigger.Invoke(trigger, i, this));
                }
            }
        }

        return history;
    }
    public void Swap(int first, int second)
    {
        first = CircularIndex(first);
        second = CircularIndex(second);

        (circle[second], circle[first]) = (circle[first], circle[second]);
    }
    public void Replace(Rune rune, int index)
    {
        index = CircularIndex(index);
        temporaryStats.Add(rune, new());
        Discard(GetRuneInCircle(index));
        // runeBoard.ReplaceSlot(rune, index);
        circle[index] = rune;
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
                Discard(circle[i]);
                circle[i] = null;
            }
        }

        circlePower = circlePowerPromise;
        circlePowerPromise = 0;
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
        if(UseStarters)
        {
            deckRef = new();
            var runes = Runes.GetAllRunes(r => r.Rarity == Rarity.Starter);
            foreach(Rune rune in runes)
            {
                for(int i = 0; i < rune.StartCount; i++)
                {
                    deckRef.Add(rune.Clone() as Rune);
                }
            }
        }
        else
        {
            foreach (RuneRef runeRef in BaseDeck)
            {
                Rune rune = runeRef.Get();
                deckRef.Add(rune);
            }
        }
        

        Restart();
        StartCoroutine(Game());
    }

    private IEnumerator Game()
    {
        int currentRound = 0;
        health = Settings.PlayerMaxHealth;
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

                var events = Activate(i);

                yield return runeBoard.Resolve(i, events, circlePower);
            }

            Debug.Log($"DEALING DAMAGE: {circlePower}");
            opponentHealth -= circlePower;
            HUD.Instance.OpponentHealth.Set(opponentHealth, Settings.GetOpponentHealth(currentRound));

            ClearCircle();
            yield return runeBoard.EndSummon();
            yield return runeBoard.UpdateScore(circlePower);

            if (opponentHealth <= 0)
            {
                set = 0;
                currentRound++;
                Debug.Log("You defeated opponent!");
                yield return new WaitForSeconds(1.0f);

                yield return runeBoard.Shop();
                Restart();

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
    public List<EventHistory> Activate(int index)
    {
        index = CircularIndex(index);
        if (circle[index] == null)
            return null;

        var events = circle[index].OnActivate?.Invoke(index, this);

        if (HasRuneAtIndex(index))
        {
            int power = GetRunePower(index);
            circlePower += power;
        }

        return events;
    }
    public void AddNewRuneToHand(Rune rune)
    {
        temporaryStats.Add(rune, new());
        hand.Add(rune);
    }
    private List<Rune> Draw(bool discard)
    {
        if (discard)
        {
            foreach (Rune rune in hand)
            {
                Discard(rune);
            }
            hand.Clear();
        }
        return Draw(Settings.HandSize - hand.Count);
    }
    public List<Rune> Draw(int count)
    {
        List<Rune> result = new();

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
                result.Add(rune);
            }
            else
            {
                Debug.LogWarning("Your deck is too small");
            }
        }

        return result;
    }
    private void Discard(Rune rune)
    {
        if (!rune.Token)
        {
            discardPile.Add(rune);
        }
    }

    public void Buy(Rune rune)
    {
        deckRef.Add(rune);
    }

}
