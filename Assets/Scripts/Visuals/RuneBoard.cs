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

    public GameObject PentagramObject;
    public GameObject ShopObject;
    public Transform ShopObjectOrigin;
    public CardPack CardPackPrefab;
    public Transform[] CardPackSlots;
    public Transform[] RandomRuneSlots;
    public RuneRef SellRune;
    public Transform SellSlot;
    public RuneRef HealRune;
    public Transform HealSlot;
    public BoxCollider ShopArea;
    public ParticleSystem ShopFeedback;

    public Transform DiscardPile;
    public Transform ExilePile;

    public TextMeshProUGUI ScoreText;

    private RuneSlot[] slots;
    private List<RuneVisuals> runes = new();
    private List<Draggable> shopObjects = new();
    private Draggable held;
    public float PlaneHeight;
    private Plane playSpace;
    private Vector3 runeVelocity;
    private Vector3 grabOffset;
    private int boughtCount;
    private Vector3 mouseDragStartPoint;

    private List<Draggable> allDragables => shopObjects.Union(runes.Select(x => x as Draggable)).ToList();
    private List<Slot> allSlots => slots.Select(s => s as Slot).ToList(); //slots.Union(ShopSlots).ToList();

    private void Start()
    {
        playSpace = new(Vector3.up, Vector3.up * PlaneHeight);
        slots = new RuneSlot[5];
        for (int i = 0; i < 5; i++)
        {
            slots[i] = Instantiate(SlotPrefab, PentagramOrigin.position, PentagramOrigin.localRotation);
            slots[i].transform.parent = PentagramObject.transform;
            slots[i].transform.localRotation = Quaternion.Euler(0, 36 + 72 * (i + 2), 0);
        }

        runes = FindObjectsOfType<RuneVisuals>().ToList();
        var draggables = FindObjectsOfType<Draggable>();
        foreach(Draggable draggable in draggables)
        {
            if (draggable is not RuneVisuals)
                shopObjects.Add(draggable);
        }

        PentagramObject.SetActive(true);
        ShopObject.SetActive(false);
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
            yield return Draw(rune);
        }

        yield return null;
    }
    public IEnumerator Draw(Rune rune)
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

    public IEnumerator Play()
    {
        PentagramObject.SetActive(true);

        bool running = true;
        HUD.Instance.EndTurnButton.onClick.AddListener(() => running = false);
        HUD.Instance.EndTurnButton.interactable = true;

        while (running)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (held == null)
            {
                yield return UpdateHover(ray);
            }
            else
            {
                yield return UpdateDrag(ray, false);
            }
        }

        HUD.Instance.EndTurnButton.onClick.RemoveAllListeners();
        HUD.Instance.EndTurnButton.interactable = false;
    }

    private IEnumerator UpdateHover(Ray ray)
    {
        // TODO: we can also hover and inspect shop objects

        runeVelocity = Vector3.zero;
        Draggable hovered = null;
        RuneSlot hoveredSlot = null;

        foreach (Draggable drag in allDragables)
        {
            if (drag.HoverCollider.Raycast(ray, out RaycastHit hit, 1000.0f))
            {
                hovered = drag;
                grabOffset = drag.transform.InverseTransformPoint(hit.point);
                grabOffset.y = 0.0f;
                mouseDragStartPoint = ray.origin;
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
                    runes.Add((RuneVisuals) held);
                }
            }
            else if (Input.GetMouseButtonDown(1))
            {
                // Inspect
                HUD.Instance.EndTurnButton.interactable = false;
                yield return Inspect(hovered, true, Quaternion.identity);
                HUD.Instance.EndTurnButton.interactable = true;
            }
        }
        else
        {
            foreach(RuneSlot slot in slots)
            {
                if(slot.Held != null && slot.Held.HoverCollider.Raycast(ray, out RaycastHit hit, 1000.0f))
                {
                    if (Input.GetMouseButtonDown(1))
                    {
                        HUD.Instance.EndTurnButton.interactable = false;
                        yield return Inspect(slot.Held, false, slot.Held.transform.rotation);
                        HUD.Instance.EndTurnButton.interactable = true;
                    }
                }
            }
        }
    }

    private IEnumerator UpdateDrag(Ray ray, bool shopping)
    {
        Slot hovered = null;
        bool shopHovered = false;
        foreach (Slot slot in allSlots)
        {
            if (slot.Collider.Raycast(ray, out RaycastHit _, 1000.0f))
            {
                hovered = slot;
            }
        }
        if (shopping)
        {
            shopHovered = ShopArea.Raycast(ray, out RaycastHit _, 1000.0f);
            if (shopHovered && !ShopFeedback.isPlaying)
                ShopFeedback.Play();
            else if (!shopHovered && ShopFeedback.isPlaying)
                ShopFeedback.Stop();
        }

        if (!Input.GetMouseButton(0))
        {
            // Release held
            if(shopping && shopHovered)
            {
                // Sell held shard or buy held shop item
                Debug.Log("Buying");
                if (held is RuneVisuals vis)
                {
                    // TODO: animate into box
                    Player.Instance.Buy(vis.Rune);
                    shopObjects.Remove(held);
                    Destroy(vis.gameObject);
                    yield return new WaitForSeconds(1.0f);
                }
                else if (held is CardPack cardPack)
                {
                    Vector3 origin = cardPack.transform.position;
                    shopObjects.Remove(cardPack);
                    Destroy(cardPack.gameObject);
                    yield return ShopRunes(origin);
                }
                boughtCount++;
            }
            else if(hovered != null && ((RuneSlot)hovered).Open)
            {
                int index = System.Array.IndexOf(slots, hovered);
                var vis = (RuneVisuals)held;
                ((RuneSlot)hovered).Set(vis);
                runes.Remove(vis);
                var events = Player.Instance.Place(vis.Rune, index);
                yield return Resolve(index, events, Player.Instance.GetCirclePower());
            }
            else
            {
                if (Vector3.Distance(ray.origin, mouseDragStartPoint) < 0.01f)
                {
                    held.ResetRot();
                    held.Rigidbody.isKinematic = false;
                    held.Rigidbody.velocity = Vector3.zero;
                }
                else
                {
                    held.Rigidbody.isKinematic = false;
                    held.Rigidbody.velocity = runeVelocity;
                }
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
            if (hovered != null && (hovered is not RuneSlot || ((RuneSlot)hovered).Open))
            {
                targetPos = hovered.transform.position + held.SlotOffset;
                targetRot = hovered.transform.localRotation;
                moveSmoothing = PentagramMoveSmoothing;
                rotationSpeed = PentagramRotationSpeed;
            }

            held.transform.position = Vector3.SmoothDamp(held.transform.position, targetPos, ref runeVelocity, moveSmoothing * Time.deltaTime);
            held.transform.localRotation = Quaternion.RotateTowards(held.transform.localRotation, targetRot, rotationSpeed * Time.deltaTime);
        }
    }

    private IEnumerator Inspect(Draggable inspect, bool enablePhysicsWhenDone, Quaternion cachedRot)
    {
        inspect.Rigidbody.isKinematic = true;

        yield return null;

        Vector3 cachedPos = inspect.transform.position;
        while (!Input.GetMouseButtonDown(1))
        {
            Transform target = CameraController.Instance.InspectPoint;
            inspect.transform.position = Vector3.SmoothDamp(inspect.transform.position, target.position + inspect.InspectOffset, ref runeVelocity, InspectMoveSmoothing * Time.deltaTime);
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
        if(enablePhysicsWhenDone)
        {
            inspect.Rigidbody.isKinematic = false;
            inspect.Rigidbody.velocity = runeVelocity;
        }
    }

    public IEnumerator BeginSummon()
    {
        yield return null;
    }

    public IEnumerator Resolve(int index, List<EventHistory> events, int circlePower)
    {
        // TODO: highlight rune
        if (events == null || events.Count == 0)
        {
        }
        else
        {
            foreach(EventHistory e in events)
            {
                switch(e.Type)
                {
                    case EventType.None:
                        break;
                    case EventType.PowerToSummon:
                        yield return UpdateScore(circlePower);
                        break;
                    case EventType.PowerToRune:
                        RuneVisuals vis = slots[e.Actor].Held;
                        vis.UpdateStats();
                        yield return new WaitForSeconds(0.5f);
                        break;
                    case EventType.Exile:
                        yield return DestroySlot(e.Actor, true);
                        break;
                    case EventType.Destroy:
                        yield return DestroySlot(e.Actor, false);
                        break;
                    case EventType.Swap:
                        break;
                    case EventType.Replace:
                        break;
                    case EventType.Draw:
                        foreach (Rune rune in e.Others)
                            yield return Draw(rune);
                        break;
                    case EventType.AddLife:
                        HUD.Instance.PlayerHealth.Set(Player.Instance.Health, Settings.PlayerMaxHealth);
                        yield return new WaitForSeconds(1.0f);
                        break;
                    case EventType.DiceRoll:
                        break;
                }
            }
        }

        yield return UpdateScore(circlePower);
    }

    public IEnumerator EndSummon()
    {
        // TODO: visuals

        foreach (RuneVisuals vis in runes)
        {
            Destroy(vis.gameObject);
        }
        runes.Clear();

        List<IEnumerator> deathAnims = new();
        for (int i = 0; i < 5; i++)
        {
            if (!slots[i].Open)
            {
                deathAnims.Add(DestroySlot(i, false));
            }
        }
        yield return RunConcurently(0.1f, deathAnims.ToArray());

        yield return new WaitForSeconds(1.0f);

        ScoreText.text = "0";
    }

    private IEnumerator UpdateScore(int circlePower)
    {
        ScoreText.text = $"{circlePower}";
        yield return new WaitForSeconds(0.2f);
    }
    
    private IEnumerator RunConcurently(float delay, params IEnumerator[] corouties)
    {
        int count = corouties.Length;
        foreach(var cor in corouties)
        {
            StartCoroutine(RunWithEndPredicate(cor, () => count--));
            yield return new WaitForSeconds(delay);
        }
        while (count > 0)
            yield return null;
    }

    private IEnumerator RunWithEndPredicate(IEnumerator coroutine, System.Action onEnd)
    {
        yield return coroutine;
        onEnd();
    }

    private IEnumerator DestroySlot(int index, bool exile)
    {
        RuneVisuals vis = slots[index].Held;
        float t = 0;
        float duration = 0.3f;
        Vector3 startPos = vis.transform.position;
        Quaternion startRot = vis.transform.rotation;
        Vector3 endPos = exile ? ExilePile.position : DiscardPile.position;
        while(t < duration)
        {
            vis.transform.position = Vector3.Lerp(startPos, endPos, t / duration);
            vis.transform.rotation = Quaternion.Slerp(startRot, Quaternion.identity, t / duration);
            t += Time.deltaTime;
            yield return null;
        }
        Destroy(slots[index].Held.gameObject); // TODO: visuals
        yield return new WaitForSeconds(0.15f);
    }

    private IEnumerator SwapSlot(int first, int second)
    {
        RuneVisuals firstVis = slots[first].Held;
        RuneVisuals secondVis = slots[second].Held;

        float t = 0.0f;
        float duration = 0.25f;
        while (t < duration)
        {

            t += Time.deltaTime;
            yield return null;
        }


        slots[first].Set(secondVis);
        slots[second].Set(firstVis);
    }
    public void SwapSlot(Rune rune, int index)
    {
        slots[index].Held.Init(rune, Player.Instance);
    }

    public IEnumerator Shop()
    {
        boughtCount = 0;
        PentagramObject.SetActive(false);
        ShopObject.SetActive(true);

        // Instantiate Shop Objects
        foreach(Transform origin in CardPackSlots)
        {
            Draggable cardPack = Instantiate(CardPackPrefab, origin.position, Quaternion.identity);
            shopObjects.Add(cardPack);
        }
        foreach(Transform origin in RandomRuneSlots)
        {
            RuneVisuals vis = Instantiate(RunePrefab, origin.position, Quaternion.identity);
            var allRunes = Runes.GetAllRunes();
            vis.Init(allRunes[Random.Range(0, allRunes.Count)], Player.Instance);
            shopObjects.Add(vis);
        }
        {
            RuneVisuals vis = Instantiate(RunePrefab, HealSlot.position, Quaternion.identity);
            vis.Init(HealRune.Get(), Player.Instance);
            shopObjects.Add(vis);
        }
        {
            RuneVisuals vis = Instantiate(RunePrefab, SellSlot.position, Quaternion.identity);
            vis.Init(SellRune.Get(), Player.Instance);
            shopObjects.Add(vis);
        }

        HUD.Instance.EndTurnButton.onClick.AddListener(() => boughtCount = 2);
        HUD.Instance.EndTurnButton.interactable = true;

        while (boughtCount < 2)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (held == null)
            {
                if (ShopFeedback.isPlaying)
                    ShopFeedback.Stop();

                yield return UpdateHover(ray);
            }
            else
            {
                yield return UpdateDrag(ray, true);
            }
        }

        if (ShopFeedback.isPlaying)
            ShopFeedback.Stop();

        foreach (Draggable obj in shopObjects)
        {
            Destroy(obj.gameObject);
        }
        shopObjects.Clear();

        HUD.Instance.EndTurnButton.onClick.RemoveAllListeners();
        HUD.Instance.EndTurnButton.interactable = false;

        PentagramObject.SetActive(true);
        ShopObject.SetActive(false);
    }

    public IEnumerator ShopRunes(Vector3 origin)
    {
        if(ShopFeedback.isPlaying)
            ShopFeedback.Stop();
        bool buying = true;

        HUD.Instance.EndTurnButton.onClick.RemoveAllListeners();
        HUD.Instance.EndTurnButton.onClick.AddListener(() => buying = false);

        List<Rune> runes = new List<Rune>();
        for (int i = 0; i < 5; i++)
        {
            List<Rune> allRunes = Runes.GetAllRunes();
            runes.Add(allRunes[Random.Range(0, allRunes.Count)]);
        }

        List<RuneVisuals> shopRunes = new();
        foreach (Rune rune in runes)
        {
            RuneVisuals vis = Instantiate(RunePrefab, origin, Quaternion.identity);
            vis.Init(rune, Player.Instance);
            vis.Rigidbody.isKinematic = true;
            shopRunes.Add(vis);
        }

        float t = 0.0f;
        float duration = 0.5f;
        while(t < duration)
        {
            for(int i = 0; i < shopRunes.Count; i++)
            {
                RuneVisuals vis = shopRunes[i];
                Transform target = CameraController.Instance.ShopPoint;
                Quaternion targetRot = Quaternion.Euler(0, 36 + 72 * i, 0);
                vis.transform.position = Vector3.Lerp(origin, target.position, t / duration);
                vis.transform.rotation = Quaternion.Slerp(Quaternion.identity, targetRot, t / duration);
            }
            t += Time.deltaTime;
            yield return null;
        }

        while(buying)
        {
            foreach(RuneVisuals vis in shopRunes)
            {
                if(vis.Collider.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit _, 1000.0f))
                {
                    if(Input.GetMouseButtonDown(0))
                    {
                        Player.Instance.Buy(vis.Rune);
                        buying = false;
                    }
                    if(Input.GetMouseButtonDown(1))
                    {
                        yield return Inspect(vis, false, vis.transform.rotation);
                    }
                }
            }
            yield return null;
        }

        foreach (RuneVisuals vis in shopRunes)
        {
            Destroy(vis.gameObject);
        }

        HUD.Instance.EndTurnButton.onClick.RemoveAllListeners();
        HUD.Instance.EndTurnButton.onClick.AddListener(() => boughtCount = 2);

        yield return null;
    }
}
