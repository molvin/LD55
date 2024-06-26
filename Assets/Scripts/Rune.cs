using System;
using System.Collections;
using System.Collections.Generic;


public enum TriggerType
{
    OnEnter,
    OnActivate,
    OnDestroy,
    OnExile,
}
public enum EventType
{
    None,
    PowerToSummon,
    PowerToRune,
    Exile,
    Destroy,
    Swap,
    Replace,
    Draw,
    Discard,
    AddLife,
    DiceRoll,
    ReturnToHand,
}
public class EventHistory
{
    public EventType Type;
    public int Actor = -1;
    public int Target = -1;
    public int Power = 0;
    public int Delta = 0;
    public Rune[] Others;

    public static EventHistory PowerToSummon(int power, int delta, int? actor = null) => new() { Type = EventType.PowerToSummon, Power = power, Delta = delta, Actor = actor.HasValue ? actor.Value : -1};
    public static EventHistory PowerToRune(int actor, int power) => new() { Type = EventType.PowerToRune, Actor = actor, Power = power };
    public static EventHistory Exile(int actor) => new() { Type = EventType.Exile, Actor = actor };
    public static EventHistory Destroy(int actor) => new() { Type = EventType.Destroy, Actor = actor };
    public static EventHistory Swap(int actor, int target) => new() { Type = EventType.Swap, Actor = actor, Target = target };
    public static EventHistory Replace(int actor, params Rune[] runes) => new() { Type = EventType.Replace, Actor = actor, Others = runes };
    public static EventHistory Draw(params Rune[] runes) => new() { Type = EventType.Draw, Others = runes };
    public static EventHistory Discard(params Rune[] runes) => new() { Type = EventType.Discard, Others = runes };
    public static EventHistory AddLife(int life) => new() { Type = EventType.AddLife, Power = life };
    public static EventHistory DiceRoll(bool success) => new() { Type = EventType.DiceRoll, Power = success ? 1 : 0 };
    public static EventHistory ReturnToHand(int actor) => new() { Type = EventType.ReturnToHand, Actor = actor };
}

public delegate List<EventHistory> EventTrigger(int selfIndex, Player player);
public delegate List<EventHistory> TriggerTrigger(TriggerType trigger, int selfIndex, int other, Player player);
public delegate bool AuraPredicate(int selfIndex, int other, Player player);

public static class Keywords
{
    public static readonly string Energy = "Energy";
}

public struct Aura
{
    public int Power;
    public string Keyword;

    public AuraPredicate Application;

    public readonly bool IsValid => (Power != 0 || Keyword != null) && Application != null;
}

public enum Rarity
{
    None,
    Starter,
    Common,
    Rare,
    Legendary,
}

[System.Serializable]
public class Rune : ICloneable
{
    public string Name;
    public int Power;
    public Rarity Rarity;
    public bool Token;
    public List<string> Keywords = new();
    public string Text;
    public int StartCount = 1;

    public List<string> StuffToExplain;

    public EventTrigger OnEnter;
    public EventTrigger OnActivate;
    public EventTrigger OnDestroy;
    public EventTrigger OnExile;
    public TriggerTrigger OnOtherRuneTrigger;
    public List<Aura> Aura;

    public object Clone()
    {
        return MemberwiseClone();
    }
}
