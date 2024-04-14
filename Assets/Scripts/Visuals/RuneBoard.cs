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

    public BoxCollider InspectDeck;
    public BoxCollider InspectDiscard;

    public StartGem Gem;
    public StartSlot StartSlot;

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
    bool running;
    private Draggable previousHover;

    private List<Draggable> allDragables => shopObjects.Union(runes).Union(new[] {Gem}).ToList();
    private List<Slot> allSlots => slots.Select(s => s as Slot).Union(new[] {StartSlot}).ToList(); //slots.Union(ShopSlots).ToList();

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
            if (draggable is not RuneVisuals && draggable is not StartGem)
                shopObjects.Add(draggable);
        }

        PentagramObject.SetActive(true);
        ShopObject.SetActive(false);
    }

    public IEnumerator Play()
    {
        PentagramObject.SetActive(true);

        running = true;
        Gem.Rigidbody.isKinematic = false;
        Gem.Rigidbody.AddForce(Random.onUnitSphere * 1 + Vector3.up * 1, ForceMode.VelocityChange);

        HUD.Instance.EndTurnButton.gameObject.SetActive(false);

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

        {
            if (hovered != previousHover && previousHover != null && previousHover is RuneVisuals vis)
            {
                if (vis.HoverParticles.isPlaying)
                {
                    vis.HoverParticles.Stop();
                }
            }
            previousHover = hovered;
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
            else
            {
                if(hovered is RuneVisuals vis && !vis.HoverParticles.isPlaying)
                {
                    vis.HoverParticles.Play();
                }
            }
        }
        else
        {
            // Check inspect circle
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

            // Check inspect deck
            if(InspectDeck.Raycast(ray, out RaycastHit _, 1000.0f) && Input.GetMouseButtonDown(1))
            {
                yield return InspectMany(Player.Instance.Bag, InspectDeck.transform);
            }

            // Check inspect discard
            if (InspectDiscard.Raycast(ray, out RaycastHit _, 1000.0f) && Input.GetMouseButtonDown(1))
            {
                yield return InspectMany(Player.Instance.DiscardPile, InspectDiscard.transform);
            }
        }
    }

    private IEnumerator UpdateDrag(Ray ray, bool shopping)
    {
        {
            if (previousHover != null && previousHover is RuneVisuals vis)
            {
                if (vis.HoverParticles.isPlaying)
                {
                    vis.HoverParticles.Stop();
                }
            }
            previousHover = null;
        }

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
            else if(hovered != null && held is RuneVisuals vis && hovered is RuneSlot slot && slot.Open)
            {
                int index = System.Array.IndexOf(slots, hovered);
                slot.Set(vis);
                runes.Remove(vis);
                var events = Player.Instance.Place(vis.Rune, index);
                yield return Resolve(index, events);
            }
            else if(hovered != null && held is StartGem && hovered is StartSlot)
            {
                held.transform.position = hovered.transform.position;
                held.transform.rotation = Quaternion.identity;
                held.Rigidbody.isKinematic = true;
                running = false;
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
            if (hovered != null && (hovered is RuneSlot slot && held is RuneVisuals && slot.Open) || (held is StartGem && hovered is StartSlot))
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

        if(inspect is RuneVisuals vis)
        {
            if (vis.HoverParticles.isPlaying)
                vis.HoverParticles.Stop();
        }

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

    public IEnumerator Resolve(int index, List<EventHistory> events)
    {
        if (events == null || events.Count == 0)
        {
        }
        else
        {
            slots[index].Active.Play();
            foreach(EventHistory e in events)
            {
                switch(e.Type)
                {
                    case EventType.None:
                        break;
                    case EventType.PowerToSummon:
                        yield return UpdateScore(e.Power);
                        break;
                    case EventType.PowerToRune:
                        {
                            RuneVisuals vis = slots[e.Actor].Held;
                            vis.UpdateStats();
                            yield return new WaitForSeconds(0.5f);
                        }
                        break;
                    case EventType.Exile:
                        yield return DestroySlot(e.Actor, true);
                        break;
                    case EventType.Destroy:
                        yield return DestroySlot(e.Actor, false);
                        break;
                    case EventType.Swap:
                        yield return SwapSlot(e.Actor, e.Target);
                        break;
                    case EventType.Replace:
                        yield return ReplaceSlot(e.Others[0], e.Actor);
                        break;
                    case EventType.Draw:
                        foreach (Rune rune in e.Others)
                            yield return Draw(rune);
                        break;
                    case EventType.AddLife:
                        HUD.Instance.PlayerHealth.Set(Player.Instance.Health, Settings.PlayerMaxHealth);
                        yield return new WaitForSeconds(1.0f);
                        break;
                    case EventType.ReturnToHand:
                        {
                            RuneVisuals vis = slots[e.Actor].Held;
                            runes.Add(vis);
                            vis.Collider.enabled = true;
                            vis.Rigidbody.isKinematic = false;
                            vis.Rigidbody.AddForce(Random.onUnitSphere * 3 + Vector3.up * 3, ForceMode.VelocityChange);
                            slots[e.Actor].Set(null);
                            yield return new WaitForSeconds(1.0f);
                        }
                        break;
                    case EventType.Discard:
                        List<RuneVisuals> vises = new();
                        List<IEnumerator> routines = new();
                        foreach(Rune rune in e.Others)
                        {
                            RuneVisuals vis = runes.Find(vis => vis.Rune == rune);
                            routines.Add(DestroyRune(vis, false));
                            vises.Add(vis);
                        }
                        yield return RunConcurently(0.1f, routines.ToArray());
                        foreach (RuneVisuals vis in vises)
                            runes.Remove(vis);

                        break;
                    case EventType.DiceRoll:
                        // TODO:
                        break;
                }

            }
            slots[index].Active.Stop();
        }
    }

    public IEnumerator FinishResolve(int index, int circlePower)
    {
        slots[index].Active.Play();
        yield return UpdateScore(circlePower);
        slots[index].Active.Stop();
    }

    public IEnumerator EndSummon()
    {
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

    public IEnumerator UpdateScore(int circlePower)
    {
        ScoreText.text = $"{circlePower}";
        yield return new WaitForSeconds(1f);
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
        yield return DestroyRune(vis, exile);
    }

    private IEnumerator DestroyRune(RuneVisuals vis, bool exile)
    {
        yield return new WaitForSeconds(0.5f);
        float t = 0;
        float duration = 0.5f;
        Vector3 startPos = vis.transform.position;
        Quaternion startRot = vis.transform.rotation;
        Vector3 endPos = exile ? ExilePile.position : DiscardPile.position;
        while (t < duration)
        {
            vis.transform.position = Vector3.Lerp(startPos, endPos, t / duration);
            vis.transform.rotation = Quaternion.Slerp(startRot, Quaternion.identity, t / duration);
            t += Time.deltaTime;
            yield return null;
        }
        Destroy(vis.gameObject);
        yield return new WaitForSeconds(0.5f);
    }

    private IEnumerator SwapSlot(int first, int second)
    {
        RuneVisuals firstVis = slots[first].Open ? null : slots[first].Held;
        RuneVisuals secondVis = slots[second].Open ? null : slots[second].Held;

        Vector3 firstStartPos = firstVis == null ? slots[first].transform.position : firstVis.transform.position;
        Vector3 secondStartPos = secondVis == null ? slots[second].transform.position : secondVis.transform.position;

        Quaternion firstStartRotation = firstVis == null ? slots[first].transform.rotation : firstVis.transform.rotation;
        Quaternion secondStartRotation = secondVis == null ? slots[second].transform.rotation : secondVis.transform.rotation;

        float t = 0.0f;
        float duration = 0.55f;
        while (t < duration)
        {
            if(firstVis != null)
            {
                firstVis.transform.position = Vector3.Lerp(firstStartPos, secondStartPos, t / duration);
                firstVis.transform.rotation = Quaternion.Slerp(firstStartRotation, secondStartRotation, t / duration);
            }
            if(secondVis != null)
            {
                secondVis.transform.position = Vector3.Lerp(secondStartPos, firstStartPos, t / duration);
                secondVis.transform.rotation = Quaternion.Slerp(secondStartRotation, firstStartRotation, t / duration);
            }


            t += Time.deltaTime;
            yield return null;
        }

        slots[first].Set(secondVis);
        slots[second].Set(firstVis);
    }
    
    private IEnumerator ReplaceSlot(Rune rune, int index)
    {
        RuneVisuals vis = slots[index].Held;
        float t = 0.0f;
        float duration = 0.25f;

        Vector3 startEuler = vis.transform.rotation.eulerAngles;
        bool done = false;
        while(t < duration)
        {
            vis.transform.rotation = Quaternion.Euler(startEuler.x, startEuler.y, Mathf.Lerp(0, 360, t / duration));
            t += Time.deltaTime;

            if (!done && t >= duration / 2)
            {
                vis.Init(rune, Player.Instance);
                done = true;
            }

            yield return null;
        }
        vis.transform.rotation = Quaternion.Euler(startEuler);

        slots[index].Held.Init(rune, Player.Instance);
        yield return null;
    }

    public IEnumerator Shop()
    {
        HUD.Instance.EndTurnButton.gameObject.SetActive(true);

        boughtCount = 0;
        PentagramObject.SetActive(false);
        ShopObject.SetActive(true);
        Gem.gameObject.SetActive(false);

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
        Gem.gameObject.SetActive(true);
        HUD.Instance.EndTurnButton.gameObject.SetActive(false);
    }

    private IEnumerator ShopRunes(Vector3 origin)
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

    public IEnumerator Draw(List<Rune> hand)
    {
        foreach (Rune rune in hand)
        {
            yield return Draw(rune);
        }

        yield return null;
    }
    
    private IEnumerator Draw(Rune rune)
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

    private IEnumerator InspectMany(List<Rune> runes, Transform origin)
    {
        Debug.Log("Inspect many");

        if (runes.Count == 0)
            yield break;
        
        yield return null;
    }

}
