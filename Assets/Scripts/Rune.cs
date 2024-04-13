using System;
using System.Collections;
using System.Collections.Generic;

public delegate void EventTrigger(int selfIndex, Player player);
public delegate bool AuraPredicate(int selfIndex, int other, Player player);

public struct Aura
{
    public int Power;
    public int Multiplier;
    public string Keyword;

    public AuraPredicate Application;

    public readonly bool IsValid => (Power != 0 || Multiplier != 0 || Keyword != null) && Application != null;
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
    public List<string> Keywords = new();
    public string Text;

    public EventTrigger OnEnter;
    public EventTrigger OnActivate;
    public Aura Aura;
}
