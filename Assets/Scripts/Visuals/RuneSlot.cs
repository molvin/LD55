using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RuneSlot : MonoBehaviour
{
    public BoxCollider Collider;

    private RuneVisuals held;

    public bool Open => held == null;

    public void Set(RuneVisuals rune)
    {
        held = rune;
        rune.transform.position = transform.position;
        rune.transform.localRotation = transform.localRotation;
    }

}
