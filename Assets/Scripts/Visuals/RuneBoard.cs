using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class RuneBoard : MonoBehaviour
{
    public RuneVisuals RunePrefab;
    public RuneSlot SlotPrefab;
    public GameObject JointPrefab;
    public float RuneMoveSmoothing;
    public float RotationSpeed;

    public float PentagramMoveSmoothing;
    public float PentagramRotationSpeed;

    public float InspectMoveSmoothing;
    public float InspectRotationSpeed;

    public Transform PentagramOrigin;

    public TextMeshProUGUI ScoreText;

    private RuneSlot[] slots;
    private List<RuneVisuals> runes = new();
    private RuneVisuals held;
    private RuneVisuals inspect;
    private SpringJoint joint;
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
            slots[i].transform.localRotation = Quaternion.Euler(0, 36 + 72 * (i + 2), 0);
        }

        runes = FindObjectsOfType<RuneVisuals>().ToList();
    }

    public void RemoveRune(Rune rune)
    {
        RuneVisuals visual = null;
        foreach (var v in runes)
        {
            if (rune == v.Rune)
            {
                visual = v;
                break;
            }
        }

        foreach (var s in slots)
        {
            if (s.Held != null && s.Held.Rune == rune)
            {
                visual = s.Held;
                s.Take();
                break;
            }
        }

        runes.Remove(visual);
        Destroy(visual.gameObject);
    }


    public IEnumerator Draw(List<Rune> hand)
    {
        foreach(Rune rune in hand)
        {
            RuneVisuals vis = Instantiate(RunePrefab);
            vis.Init(rune, Player.Instance);
            runes.Add(vis);

            vis.transform.position = new Vector3(
                UnityEngine.Random.Range(-0.5f, 0.5f),
                1.0f,
                UnityEngine.Random.Range(-2.5f, -1.5f));
            var rigidBody = vis.GetComponent<Rigidbody>();
            rigidBody.AddForce(UnityEngine.Random.onUnitSphere, ForceMode.VelocityChange);

            yield return new WaitForSeconds(0.2f);
        }

        yield return null;
    }

    public IEnumerator Play()
    {
        bool running = true;
        HUD.Instance.EndTurnButton.onClick.AddListener(() => running = false);
        HUD.Instance.EndTurnButton.interactable = true;

        while (running)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (held == null && inspect == null)
            {
                UpdateHover(ray);
            }
            else if(inspect != null)
            {
                yield return Inspect();
            }
            else
            {
                UpdateDrag(ray);
            }

            yield return null;
        }

        HUD.Instance.EndTurnButton.onClick.RemoveAllListeners();
        HUD.Instance.EndTurnButton.interactable = false;

        // TODO: destroy all runes
    }

    private void UpdateHover(Ray ray)
    {
        runeVelocity = Vector3.zero;
        RuneVisuals hovered = null;
        RuneSlot hoveredSlot = null;

        foreach (RuneVisuals vis in runes)
        {
            if (vis.HoverCollider.Raycast(ray, out RaycastHit hit, 1000.0f))
            {
                hovered = vis;
                grabOffset = vis.transform.InverseTransformPoint(hit.point);
                grabOffset.y = 0.0f;
            }
        }

        if (hovered != null)
        {
            if (Input.GetMouseButtonDown(0))
            {
                // Drag
                held = hovered;
                held.Rigidbody.isKinematic = true;
                if (hoveredSlot != null)
                {
                    hoveredSlot.Take();
                    runes.Add(held);
                }
            }
            else if (Input.GetMouseButtonDown(1))
            {
                // Inspect
                inspect = hovered;
                inspect.Rigidbody.isKinematic = true;
                //StartCoroutine(Inspect());
            }
        }
    }

    private void UpdateDrag(Ray ray)
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
                int index = Array.IndexOf(slots, hovered);
                Player.Instance?.Place(held.Rune, index);
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
            playSpace.Raycast(ray, out float enter);
            Vector3 planePoint = ray.GetPoint(enter);

            Vector3 targetPos = planePoint - grabOffset;
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
        while (!Input.GetMouseButtonDown(1))
        {
            Transform target = CameraController.Instance.InspectPoint;
            inspect.transform.position = Vector3.SmoothDamp(inspect.transform.position, target.position, ref runeVelocity, InspectMoveSmoothing * Time.deltaTime);
            inspect.transform.localRotation = Quaternion.RotateTowards(inspect.transform.localRotation, target.rotation, InspectRotationSpeed * Time.deltaTime);
            yield return null;
        }

        while (Vector3.Distance(inspect.transform.position, cachedPos) > 0.1f || Quaternion.Angle(inspect.transform.localRotation, cachedRot) > 1)
        {
            inspect.transform.position = Vector3.SmoothDamp(inspect.transform.position, cachedPos, ref runeVelocity, InspectMoveSmoothing * Time.deltaTime);
            inspect.transform.localRotation = Quaternion.RotateTowards(inspect.transform.localRotation, cachedRot, InspectRotationSpeed * Time.deltaTime);

            yield return null;
        }

        inspect.transform.position = cachedPos;
        inspect.transform.localRotation = cachedRot;

        inspect = null;
    }

    public IEnumerator Resolve(int i, int power, int circlePower)
    {
        ScoreText.text = $"{circlePower}";
        Destroy(slots[i].Held.gameObject); // TODO: visuals
        yield return new WaitForSeconds(1.0f);
    }

    public IEnumerator EndRound()
    {
        // TODO: visuals

        ScoreText.text = "0";

        foreach(RuneVisuals vis in runes)
        {
            Destroy(vis.gameObject);
        }
        runes.Clear();

        yield return new WaitForSeconds(1.0f);
    }
}
