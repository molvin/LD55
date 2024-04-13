using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class RuneBoard : MonoBehaviour
{
    public RuneVisuals RunePrefab;
    public RuneSlot SlotPrefab;
    public float RuneMoveSmoothing;
    public float RotationSpeed;

    public float PentagramMoveSmoothing;
    public float PentagramRotationSpeed;

    public float InspectMoveSmoothing;
    public float InspectRotationSpeed;

    public Transform PentagramOrigin;

    public TextMeshProUGUI ScoreText;

    private RuneSlot[] slots;
    private List<RuneVisuals> runes;
    private RuneVisuals held;
    private RuneVisuals inspect;
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
            slots[i].transform.position += slots[i].transform.forward * 0.2f;

        }

        runes = FindObjectsOfType<RuneVisuals>().ToList();
    }

    private void Update()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        playSpace.Raycast(ray, out float enter);
        Vector3 planePoint = ray.GetPoint(enter);

        if (held == null && inspect == null)
        {
            runeVelocity = Vector3.zero;
            RuneVisuals hovered = null;
            RuneSlot hoveredSlot = null;
            foreach (RuneVisuals vis in runes)
            {
                if(vis.HoverCollider.Raycast(ray, out RaycastHit hit, 1000.0f))
                {
                    hovered = vis;
                    grabOffset = hit.point - vis.transform.position;
                    grabOffset.y = 0.0f;
                }
            }
            foreach(RuneSlot slot in slots)
            {
                if (slot.Held != null && slot.Held.HoverCollider.Raycast(ray, out RaycastHit hit, 1000.0f))
                {
                    hovered = slot.Held;
                    hoveredSlot = slot;
                    grabOffset = hit.point - slot.Held.transform.position;
                    grabOffset.y = 0.0f;
                }
            }


            if (hovered != null)
            {
                if(Input.GetMouseButtonDown(0))
                {
                    // Drag
                    held = hovered;
                    held.Rigidbody.isKinematic = true;
                    if(hoveredSlot != null)
                    {
                        hoveredSlot.Take();
                        runes.Add(held);
                    }
                }
                else if(Input.GetMouseButtonDown(1))
                {
                    // Inspect
                    inspect = hovered;
                    inspect.Rigidbody.isKinematic = true;
                    StartCoroutine(Inspect());
                }
            }
        }
        else if(held != null)
        {
            UpdateDrag(ray, planePoint);
        }
    }

    private void UpdateDrag(Ray ray, Vector3 planePoint)
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
                held.Rigidbody.isKinematic = false;
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

    private IEnumerator Inspect()
    {
        yield return null;

        Vector3 cachedPos = inspect.transform.position;
        Quaternion cachedRot = inspect.transform.rotation;
        while(!Input.GetMouseButtonDown(1))
        {
            Transform target = CameraController.Instance.InspectPoint;
            inspect.transform.position = Vector3.SmoothDamp(inspect.transform.position, target.position, ref runeVelocity, InspectMoveSmoothing * Time.deltaTime);
            inspect.transform.localRotation = Quaternion.RotateTowards(inspect.transform.localRotation, target.rotation, InspectRotationSpeed * Time.deltaTime);
            yield return null;
        }

        while(Vector3.Distance(inspect.transform.position, cachedPos) > 0.1f || Quaternion.Angle(inspect.transform.localRotation, cachedRot) > 1)
        {
            inspect.transform.position = Vector3.SmoothDamp(inspect.transform.position, cachedPos, ref runeVelocity, InspectMoveSmoothing * Time.deltaTime);
            inspect.transform.localRotation = Quaternion.RotateTowards(inspect.transform.localRotation, cachedRot, InspectRotationSpeed * Time.deltaTime);

            yield return null;
        }

        inspect.transform.position = cachedPos;
        inspect.transform.localRotation = cachedRot;

        inspect = null;
    }
}
