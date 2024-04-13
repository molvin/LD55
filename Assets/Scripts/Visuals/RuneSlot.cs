using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RuneSlot : Slot
{

    private RuneVisuals held;
    public RuneVisuals Held => held;

    public bool Open => held == null;

    public void Set(RuneVisuals rune)
    {
        held = rune;

        if (held != null)
        {
            held.Rigidbody.isKinematic = true;
            held.Collider.enabled = false;
            held.transform.position = transform.position;
            held.transform.localRotation = transform.localRotation;
        }
    }

    public void Take()
    {
        held.Collider.enabled = true;
        held = null;
    }

}
