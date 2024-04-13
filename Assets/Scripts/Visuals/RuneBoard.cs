using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RuneBoard : MonoBehaviour
{
    public RuneVisuals RunePrefab;
    public RuneSlot SlotPrefab;
    public Vector3 Offset;
    public float RotationSpeed;
    public Transform PentagramOrigin;

    private RuneSlot[] slots;
    private RuneVisuals[] runes;
    private RuneVisuals held;
    private Plane playSpace = new (Vector3.up, Vector3.up * 0.1f);

    private void Start()
    {
        slots = new RuneSlot[5];
        for (int i = 0; i < 5; i++)
        {
            slots[i] = Instantiate(SlotPrefab, PentagramOrigin.position, PentagramOrigin.localRotation);    
            slots[i].Rotator.localRotation = Quaternion.Euler(0, 0, 72 * i);
        }

        runes = FindObjectsOfType<RuneVisuals>();
    }

    private void Update()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        playSpace.Raycast(ray, out float enter);
        Vector3 planePoint = ray.GetPoint(enter);

        if (held == null)
        {
            RuneVisuals hovered = null;
            foreach(RuneVisuals vis in runes)
            {
                if(vis.Collider.Raycast(ray, out RaycastHit _, 1000.0f))
                {
                    hovered = vis;
                }
            }

            if (hovered != null)
            {
                if(Input.GetMouseButtonDown(0))
                {
                    held = hovered;
                    held.Rigidbody.useGravity = false;
                }
            }
        }
        else
        {
            if(!Input.GetMouseButton(0))
            {
                held.Rigidbody.useGravity = true;
                held = null;
            }
            else
            {
                // TODO: check if you are hovering a slot, if so rotate
                RuneSlot hovered = null;
                foreach (RuneSlot slot in slots)
                {
                    if (slot.Collider.Raycast(ray, out RaycastHit _, 1000.0f))
                    {
                        hovered = slot;
                    }
                }

                Quaternion targetRot = Quaternion.identity;
                if (hovered != null && hovered.Open)
                {
                    targetRot = hovered.Rotator.localRotation;
                }

                held.Rotator.localRotation = Quaternion.RotateTowards(held.Rotator.localRotation, targetRot, RotationSpeed * Time.deltaTime);
                held.transform.position = planePoint + (held.Rotator.rotation * Offset);

                if (hovered != null)
                {
                    if (Input.GetMouseButtonDown(0))
                    {
                        if (Place(held, hovered))
                        {
                            held = null;
                        }
                    }
                }
            }
        }
    }

    private bool Place(RuneVisuals rune, RuneSlot slot)
    {
        if(slot.Open)
        {
            slot.Set(rune);
            return true;
        }

        return false;
    }

}
