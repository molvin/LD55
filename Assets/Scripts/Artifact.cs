using System;
using System.Collections;
using System.Collections.Generic;


public delegate List<EventHistory> ArtifactRuneTrigger(TriggerType trigger, int runeIndex, Player player);

public struct ArtifactStats
{
    public int ShopActions;
    public int HandSize;
    public int Regen;
}

[System.Serializable]
public class Artifact : ICloneable
{
    public string Name;
    public string Text;
    public int Limit = 1;

    public ArtifactStats Stats;
    public ArtifactRuneTrigger RuneTrigger;

    public object Clone()
    {
        return MemberwiseClone();
    }
}
