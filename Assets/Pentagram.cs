using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pentagram : MonoBehaviour
{
    public GemSlot[] Slots;
    private Dictionary<GemSlot, bool> isPlaying = new();

    private void Start()
    {
        foreach(GemSlot slot in Slots)
        {
            isPlaying.Add(slot, false);
        }
    }

    public void Cache()
    {
        foreach (GemSlot slot in Slots)
        {
            isPlaying[slot] = slot.ActiveParticles.isPlaying;
        }
    }
    public void Pop()
    {
        foreach (GemSlot slot in Slots)
        {
            if (isPlaying.ContainsKey(slot))
            {
                if (isPlaying[slot])
                    slot.ActiveParticles.Play();
                else
                    slot.ActiveParticles.Stop();
            }
        }
    }
}
