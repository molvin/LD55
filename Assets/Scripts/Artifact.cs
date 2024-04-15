using System;
using System.Collections;
using System.Collections.Generic;


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

    public ArtifactStats Stats;

    public object Clone()
    {
        return MemberwiseClone();
    }
}
