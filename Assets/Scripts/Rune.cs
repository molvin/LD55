using System;
using System.Collections;
using System.Collections.Generic;


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
    AddLife,
}
public class EventHistory
{
    public EventType[] Types;
    public int Actor = -1;
    public int Target = -1;
    public int Power = 0;
    public Rune[] Others;
}

public delegate void EventTrigger(int selfIndex, Player player);
public delegate bool AuraPredicate(int selfIndex, int other, Player player);

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
public class Rune
{
    public string Name;
    public int Power;
    public Rarity Rarity;
    public bool Token;
    public List<string> Keywords = new();
    public string Text;
    public int StartCount = 1;

    public EventTrigger OnEnter;
    public EventTrigger OnActivate;
    public EventTrigger OnDestroy;
    public EventTrigger OnExile;
    public List<Aura> Aura;
}
