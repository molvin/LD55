using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class Artifact : ICloneable
{
    public string Name;
    public string Text;

    public EventTrigger OnEnter;
    public EventTrigger OnExit;
    public TriggerTrigger OnOtherRuneTrigger;
    public List<Aura> Aura;

    public object Clone()
    {
        return MemberwiseClone();
    }
}
