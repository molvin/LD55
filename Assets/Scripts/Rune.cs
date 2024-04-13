using System;
using System.Collections;
using System.Collections.Generic;

public delegate void EventTrigger(int selfIndex, Player player);
public delegate bool AuraPredicate(int selfIndex, int other, Player player);

public struct Aura
{
    public int Power;
    public string Keyword;

    public AuraPredicate Application;

    public readonly bool IsValid => (Power != 0 || Keyword != null) && Application != null;
}

[System.Serializable]
public class Rune
{
    public string Name;
    public int Cost;
    public int Power;
    public List<string> Keywords = new();
    public string Text;

    public EventTrigger OnEnter;
    public Aura Aura;


    public object Clone()
    {
        // Note: I think this is a shallow clone, though it shouldn't matter since we don't really care about what the deep copy would cover
        return MemberwiseClone();
    }
}
