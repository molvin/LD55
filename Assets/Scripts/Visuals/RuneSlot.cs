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

        held.Rigidbody.velocity = Vector3.zero;
        held.Rigidbody.useGravity = false;
        held.Collider.enabled = false;
        held.transform.position = transform.position;
        held.transform.localRotation = transform.localRotation;
    }

}
