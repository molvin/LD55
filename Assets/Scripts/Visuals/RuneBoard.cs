using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using UnityEngine;

public class RuneBoard : MonoBehaviour
{
    public RuneVisuals RunePrefab;
    public RuneSlot SlotPrefab;
    public float RuneMoveSmoothing;
    public float RotationSpeed;

    public float PentagramMoveSmoothing;
    public float PentagramRotationSpeed;

    public Transform PentagramOrigin;

    private RuneSlot[] slots;
    private List<RuneVisuals> runes;
    private RuneVisuals held;
    public float PlaneHeight;
    private Plane playSpace;
    private Vector3 runeVelocity;
    private Vector3 grabOffset;

    private void Start()
    {
        playSpace = new(Vector3.up, Vector3.up * PlaneHeight);
        slots = new RuneSlot[5];
        for (int i = 0; i < 5; i++)
        {
            slots[i] = Instantiate(SlotPrefab, PentagramOrigin.position, PentagramOrigin.localRotation);    
            slots[i].transform.localRotation = Quaternion.Euler(0, 36 + 72 * i, 0);
        }

        runes = FindObjectsOfType<RuneVisuals>().ToList();
    }

    private void Update()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        playSpace.Raycast(ray, out float enter);
        Vector3 planePoint = ray.GetPoint(enter);

        if (held == null)
        {
            runeVelocity = Vector3.zero;
            RuneVisuals hovered = null;
            foreach(RuneVisuals vis in runes)
            {
                if(vis.Collider.Raycast(ray, out RaycastHit hit, 1000.0f))
                {
                    hovered = vis;
                    grabOffset = hit.point - vis.transform.position;
                    grabOffset.y = 0.0f;
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
            RuneSlot hovered = null;
            foreach (RuneSlot slot in slots)
            {
                if (slot.Collider.Raycast(ray, out RaycastHit _, 1000.0f))
                {
                    hovered = slot;
                }
            }

            if (!Input.GetMouseButton(0))
            {
                // Release held
                if (hovered != null && hovered.Open)
                {
                    hovered.Set(held);
                    runes.Remove(held);
                }
                else
                {
                    held.Rigidbody.useGravity = true;
                    held.Rigidbody.velocity = runeVelocity;
                }
                held = null;
            }
            else
            {
                // Drag held

                Vector3 targetPos = planePoint - (held.transform.rotation * grabOffset);
                Quaternion targetRot = Quaternion.identity;
                float moveSmoothing = RuneMoveSmoothing;
                float rotationSpeed = RotationSpeed;
                if (hovered != null && hovered.Open)
                {
                    targetPos = hovered.transform.position;
                    targetRot = hovered.transform.localRotation;
                    moveSmoothing = PentagramMoveSmoothing;
                    rotationSpeed = PentagramRotationSpeed;
                }

                held.transform.position = Vector3.SmoothDamp(held.transform.position, targetPos, ref runeVelocity, moveSmoothing * Time.deltaTime);
                held.transform.localRotation = Quaternion.RotateTowards(held.transform.localRotation, targetRot, rotationSpeed * Time.deltaTime);
            }
        }
    }
}
