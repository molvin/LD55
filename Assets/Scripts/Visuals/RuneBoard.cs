using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class RuneBoard : MonoBehaviour
{
    public RuneVisuals RunePrefab;
    public RuneSlot SlotPrefab;
    public Gem GemPrefab;
    public float RuneMoveSmoothing;
    public float RotationSpeed;

    public float PentagramMoveSmoothing;
    public float PentagramRotationSpeed;

    public float InspectMoveSmoothing;
    public float InspectRotationSpeed;

    public Transform PentagramOrigin;

    public Pentagram PentagramObject;
    public GameObject ShopObject;
    public Transform ShopObjectOrigin;
    public CardPack CardPackPrefab;
    public Transform[] CardPackSlots;
    public Transform[] RandomRuneSlots;
    public Transform SellSlot;
    public Transform HealSlot;
    public BoxCollider ShopArea;
    public ParticleSystem ShopFeedback;

    public Transform DiscardPile;
    public Transform ExilePile;
    public Transform RuneSpawn;
    public Transform[] ShopGemPoints;
    public BoxCollider PlayArea;
    public Transform StartGemOrigin;
    public float StartGemUpForce;
    // public BoxCollider InspectDeck;
    // public BoxCollider InspectDiscard;

    public Gem StartGem;
    public List<Gem> Gems = new();
    public GemSlot StartSlot;
    public GemSlot[] GemSlots;
    public ScrollAnimationController ScrollAnimation;
    public TextMeshPro ScoreText;

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

    public Animator CameraAnim;
    public TextMeshProUGUI OpponentHealth;
    public ProgressView Progress;
    public GameObject TutorialObject;

    private List<Draggable> allDragables => shopObjects.Union(runes).Union(Gems).Union(new[] {StartGem}).ToList();
    private List<Slot> allSlots => new Slot[] { StartSlot }.Union(slots).Union(GemSlots).ToList(); //slots.Union(ShopSlots).ToList();


    [Header("animations")]
    public AnimationCurve textPointsResolveAnim;
    public float textPointsResolveDuration = 1;


    [Header("audio")]
    public AudioOneShotClipConfiguration placeInSlotSound;
    public AudioOneShotClipConfiguration dropShardSound;
    public AudioOneShotClipConfiguration startSummonSound;
    public AudioOneShotClipConfiguration replaceSound;
    public AudioOneShotClipConfiguration addPowerToCircleSound;
    public AudioOneShotClipConfiguration raiseShardAnimSound;

    

    private Audioman audioman;

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
            if (draggable is not RuneVisuals && draggable is not Gem)
                shopObjects.Add(draggable);
        }

        PentagramObject.gameObject.SetActive(true);

        ShopObject.SetActive(false);

        audioman = FindObjectOfType<Audioman>();
    }

    private void Update()
    {
        List<Transform> relevantTransforms = runes.Select(r => r.transform)
                                            .Union(Gems.Select(g => g.transform))
                                            .ToList();
        if (StartGem != null)
            relevantTransforms.Add(StartGem.transform);
        foreach(Transform t in relevantTransforms)
        {
            if(!PlayArea.bounds.Contains(t.position))
            {
                t.position = PlayArea.bounds.ClosestPoint(t.position);
            }
        }

        if(StartGem != null && StartGem.isActiveAndEnabled && !StartGem.Rigidbody.isKinematic)
        {
            Vector3 force = (StartGemOrigin.position - StartGem.transform.position);
            float maxForce = 5;
            if(force.magnitude > 0.1)
            {
                force = Vector3.ClampMagnitude(force, maxForce * Time.deltaTime);
                StartGem.Rigidbody.AddForce(force + Vector3.up * (StartGemUpForce * Time.deltaTime), ForceMode.VelocityChange);
            }
        }
    }

    public IEnumerator Tutorial()
    {
        ScrollAnimation.Play("OpenScrollStar");
        while (ScrollAnimation.isPlaying)
            yield return null;

        TutorialObject.SetActive(true);
        while (!Input.GetMouseButtonDown(0) && !Input.GetMouseButtonDown(0))
            yield return null;
        TutorialObject.SetActive(false);

        ScrollAnimation.Play("CloseScroll");
        while (ScrollAnimation.isPlaying)
            yield return null;
    }

    public IEnumerator BeginRound()
    {
        PentagramObject.gameObject.SetActive(true);
        StartGem.gameObject.SetActive(true);
        foreach (Gem gem in Gems)
        {
            gem.gameObject.SetActive(true);
        }
        foreach (GemSlot gemSlot in GemSlots)
        {
            if (gemSlot.Held)
                gemSlot.ActiveParticles.Play();
        }
        ScrollAnimation.Play("OpenScrollStar");
        while (ScrollAnimation.isPlaying)
            yield return null;
    }

    public IEnumerator EndRound()
    {
        PentagramObject.gameObject.SetActive(false);
        StartGem.gameObject.SetActive(false);
        foreach (Gem gem in Gems)
            gem.gameObject.SetActive(false);

        ScrollAnimation.Play("CloseScroll");
        while (ScrollAnimation.isPlaying)
            yield return null;
    }

    public IEnumerator Play()
    {
        Vector3 randomDir = Random.onUnitSphere;
        if (Vector3.Dot(randomDir, Vector3.up) < 0)
            randomDir *= -1;
        StartGem.Rigidbody.isKinematic = false;
        StartGem.Rigidbody.AddForce(randomDir * 3 + Vector3.up * 1, ForceMode.VelocityChange);
        

        running = true;

        HUD.Instance.EndTurnButton.gameObject.SetActive(false);

        while (running)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (held == null)
            {
                yield return UpdateHover(ray, false);
            }
            else
            {
                yield return UpdateDrag(ray, false);
            }
        }

        HUD.Instance.EndTurnButton.onClick.RemoveAllListeners();
        HUD.Instance.EndTurnButton.interactable = false;
    }

    private IEnumerator UpdateHover(Ray ray, bool shopping)
    {
        runeVelocity = Vector3.zero;
        Draggable hovered = null;

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
                vis.Hover = false;
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

                var gemSlots = GemSlots.Where(slot => slot.Held != null && slot.Held == held).ToList();
                if(gemSlots.Count > 0)
                {
                    gemSlots[0].Held = null;
                    TakeArtifact(held as Gem, gemSlots[0]);
                }
            }
            else if (Input.GetMouseButtonDown(1))
            {
                // Inspect
                HUD.Instance.EndTurnButton.interactable = false;
                bool resetPhysics = true;
                if(hovered is Gem gem)
                {
                    var slot = GemSlots.FirstOrDefault(slot => slot.Held == gem);
                    resetPhysics = slot == null;
                }

                if(hovered is RuneVisuals vis && runes.Contains(vis))
                {
                    yield return Inspect(vis, resetPhysics, Quaternion.identity, runes);
                }
                else if (hovered is Gem g && Gems.Contains(g))
                {
                    yield return Inspect(g, resetPhysics, Quaternion.identity, Gems);
                }
                else if(hovered == StartGem)
                {
                    StartGem = null;
                    yield return Inspect(hovered, resetPhysics, Quaternion.identity, null);
                    StartGem = hovered as Gem;
                }
                else
                {
                    yield return Inspect(hovered, resetPhysics, Quaternion.identity, null);
                }
                HUD.Instance.EndTurnButton.interactable = true;
            }
            else
            {
                if(hovered is RuneVisuals vis)
                {
                    vis.Hover = true;
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
                        yield return Inspect(slot.Held, false, slot.Held.transform.rotation, null);
                        HUD.Instance.EndTurnButton.interactable = true;
                    }
                }
            }

            /*
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
            */

            if(!shopping)
            {
                bool withinXSpan = Input.mousePosition.x > Screen.width * 0.4f && Input.mousePosition.x < Screen.width * 0.6f;
                if (withinXSpan && Input.mousePosition.y > (Screen.height * 0.85))
                {
                    yield return ViewOpponent();
                }
                if (withinXSpan && Input.mousePosition.y < (Screen.height * 0.05))
                {
                    yield return ViewSelf();
                }
            }
        }
    }

    private IEnumerator UpdateDrag(Ray ray, bool shopping)
    {
        {
            if (previousHover != null && previousHover is RuneVisuals vis)
            {
                vis.Hover = false;
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
            shopHovered = Input.mousePosition.y < Screen.height * 0.15f;
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
                if (held is RuneVisuals vis)
                {
                    // TODO: animate into box
                    Player.Instance.Buy(vis.Rune);
                    shopObjects.Remove(held);
                    Destroy(vis.gameObject);
                    yield return new WaitForSeconds(0.25f);
                }
                else if (held is CardPack cardPack)
                {
                    Vector3 origin = cardPack.transform.position;
                    shopObjects.Remove(cardPack);
                    Destroy(cardPack.gameObject);
                    yield return ShopRunes(origin);
                }
                else if(held is Gem gem)
                {
                    shopObjects.Remove(gem);
                    Gems.Add(gem);
                    gem.gameObject.SetActive(false);
                    yield return new WaitForSeconds(0.25f);
                }
                boughtCount++;
            }
            else if(hovered != null && held is RuneVisuals vis && hovered is RuneSlot slot && slot.Open)
            {
                int index = System.Array.IndexOf(slots, hovered);
                slot.Set(vis);
                runes.Remove(vis);
                var events = Player.Instance.Place(vis.Rune, index);
                vis.UpdateStats();
                FindAnyObjectByType<Audioman>().PlaySound(placeInSlotSound, slot.transform.position);
                yield return Resolve(index, events);
            }
            else if(hovered != null && held is Gem gem && hovered is GemSlot gemSlot && (gem == StartGem && gemSlot.IsStart || gem != StartGem && !gemSlot.IsStart))
            {
                held.transform.position = hovered.transform.position;
                held.transform.rotation = Quaternion.identity;
                held.Rigidbody.isKinematic = true;

                if(gem == StartGem)
                {
                    running = false;
                    gemSlot.ActiveParticles.Play();
                    FindAnyObjectByType<Audioman>().PlaySound(startSummonSound, gemSlot.transform.position);
                }
                else
                {
                    gemSlot.Held = gem;
                    yield return PlaceArtifact(gem, gemSlot);
                }
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
                    FindAnyObjectByType<Audioman>().PlaySound(dropShardSound, held.transform.position);
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
            if (hovered != null && (hovered is RuneSlot slot && held is RuneVisuals && slot.Open) || (held is Gem gem && hovered is GemSlot gemslot && (gem == StartGem && gemslot.IsStart || gem != StartGem && !gemslot.IsStart)))
            {
                targetPos = hovered.transform.position + held.SlotOffset;
                targetRot = hovered.transform.localRotation;
                moveSmoothing = PentagramMoveSmoothing;
                rotationSpeed = PentagramRotationSpeed;
            }

            held.transform.position = Vector3.SmoothDamp(held.transform.position, targetPos, ref runeVelocity, moveSmoothing);
            held.transform.localRotation = Quaternion.RotateTowards(held.transform.localRotation, targetRot, rotationSpeed * Time.deltaTime);
        }
    }

    private IEnumerator Inspect<T>(T inspect, bool enablePhysicsWhenDone, Quaternion cachedRot, List<T> containingList) where T : Draggable
    {
        inspect.Rigidbody.isKinematic = true;

        if (containingList != null)
        {
            containingList.Remove(inspect);
        }

        if (inspect is RuneVisuals vis)
        {
            vis.Hover = false;
        }
        else if (inspect is Gem g)
        {
            g.ToggleText(true);
        }

        yield return null;

        Vector3 cachedPos = inspect.transform.position;
        while (!Input.GetMouseButtonDown(1))
        {
            Transform target = CameraController.Instance.InspectPoint;
            inspect.transform.position = Vector3.SmoothDamp(inspect.transform.position, target.position + inspect.InspectOffset, ref runeVelocity, InspectMoveSmoothing);
            inspect.transform.localRotation = Quaternion.RotateTowards(inspect.transform.localRotation, target.rotation, InspectRotationSpeed * Time.deltaTime);
            yield return null;
        }
        if (inspect is Gem gem)
        {
            gem.ToggleText(false);
        }
        while (Vector3.Distance(inspect.transform.position, cachedPos) > 0.1f || Quaternion.Angle(inspect.transform.localRotation, cachedRot) > 1)
        {
            inspect.transform.position = Vector3.SmoothDamp(inspect.transform.position, cachedPos, ref runeVelocity, InspectMoveSmoothing);
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

        if (containingList != null)
        {
            containingList.Add(inspect);
        }

    }

    public void ForceUpdateVisuals()
    {
        foreach (var slot in slots)
        {
            if (slot.Held != null)
            {
                slot.Held.UpdateStats();
            }
        }
    }

    public IEnumerator BeginSummon()
    {
        CameraAnim.SetTrigger("ToSummon");
        yield return new WaitForSeconds(1.0f);
    }

    public IEnumerator BeginResolve(int index)
    {
        slots[index].Held.GetComponent<Animator>().enabled = true;

        slots[index].Held.GetComponent<Animator>().SetTrigger("raise");
        FindAnyObjectByType<Audioman>().PlaySound(raiseShardAnimSound, ScoreText.transform.position);

        yield return new WaitForSeconds(0.2f);

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
                        yield return AddPowerToSummonAnim(e.Actor >= 0 ? e.Actor : index, e.Delta);
                        yield return UpdateScore(e.Power); //TODO floaty number animation
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

    public IEnumerator FinishResolve(int index, int circlePower) // TODO floaty number animation
    {
        if(slots[index].Held != null) {
            slots[index].Active.Play();
            yield return SpawnAndAnimateFlyingNumber(index);
            yield return UpdateScore(circlePower);
          
            if (!slots[index].Open)
            {
                slots[index].Held.GetComponent<Animator>().SetTrigger("lower");

            }
            
            yield return new WaitForSeconds(0.3f);
            FindAnyObjectByType<Audioman>().PlaySound(dropShardSound, ScoreText.transform.position);
            slots[index].Active.Stop();
        }
    }

    public IEnumerator SpawnAndAnimateFlyingNumber(int index) //TODO add rotation
    {

        TextMeshProUGUI power = slots[index].Held.Power;
        Vector3 startPoint = power.transform.position;
        Vector3 startPointLocal = power.transform.localPosition;
        Quaternion starRotLocal = power.transform.localRotation;
        Quaternion starRot = power.transform.rotation;

        Debug.Log("pause");
        float time = 0;
        float duration = 0.5f;
        while (time < duration)
        {
            time += Time.deltaTime;
            power.transform.position = startPoint + ((startPoint + new Vector3(0,0.2f,0) - startPoint) * (time / duration));
            yield return null;

        }
        time = 0;
        var targetDirection = ScoreText.transform.position - startPoint;

        var startForward = power.transform.up;
        while (time < duration)
        {
            time += Time.deltaTime;
            //TODO WTFF
            power.transform.rotation = Quaternion.Slerp(starRot.normalized, Quaternion.LookRotation(targetDirection, Vector3.up), time / duration);

            yield return null;

        }

        Vector3 secondStartPoint = power.transform.position;
        yield return null;
        FindAnyObjectByType<Audioman>().PlaySound(addPowerToCircleSound, ScoreText.transform.position);

        time = 0;
        while(time < textPointsResolveDuration)
        {
            time += Time.deltaTime;
            power.transform.position = secondStartPoint + ((ScoreText.transform.position - secondStartPoint) * textPointsResolveAnim.Evaluate(time / textPointsResolveDuration));
            yield return null;

        }
        power.transform.localPosition = startPointLocal;
        power.transform.localRotation = starRotLocal;

        StartCoroutine(FadeInShardPower(power));
    }

    public IEnumerator AddPowerToSummonAnim(int index, int power) //TODO add rotation
    {

        TextMeshProUGUI powerText = slots[index].Held.Power;
        Vector3 startPoint = powerText.transform.position;
        Vector3 startPointLocal = powerText.transform.localPosition;
        string og_power = powerText.text;
        powerText.text = power+"";
        float time = 0;
        float duration = 0.6f;
        while (time < duration)
        {
            time += Time.deltaTime;
            powerText.transform.position = startPoint + ((startPoint + new Vector3(0, 0.1f, 0) - startPoint) * (time / duration));
            yield return null;

        }

        Vector3 secondStartPoint = powerText.transform.position;
        yield return null;

        time = 0;
        while (time < textPointsResolveDuration)
        {
            time += Time.deltaTime;
            powerText.transform.position = secondStartPoint + ((ScoreText.transform.position - secondStartPoint) * textPointsResolveAnim.Evaluate(time / textPointsResolveDuration));
            yield return null;

        }
        powerText.text = og_power;

        powerText.transform.localPosition = startPointLocal;
    }

    public IEnumerator FadeInShardPower(TextMeshProUGUI power)
    {
        Color originalColor = power.color;
        float time = 0;
        float fadeInDuration = 1f;
        while (time < fadeInDuration)
        {
            time += Time.deltaTime;
            float currentAlpha = ((originalColor.a) * (time / fadeInDuration));
            Color newColor = power.color;
            newColor.a = currentAlpha;
            power.color = newColor;
            yield return null;

        }
    }

    public IEnumerator EndSummon()
    {

       


        List<IEnumerator> deathAnims = new();
        foreach (RuneVisuals vis in runes)
        {
            deathAnims.Add(DestroyRune(vis, false));
        }
        runes.Clear();

        yield return RunConcurently(0.1f, deathAnims.ToArray());

        deathAnims = new();
        for (int i = 0; i < 5; i++)
        {
            if (!slots[i].Open)
            {
                slots[i].Held.Rigidbody.isKinematic = true;
                deathAnims.Add(DestroySlot(i, false));
            }
        }
        yield return RunConcurently(0.1f, deathAnims.ToArray());
        
        CameraAnim.SetTrigger("SummonDone");
        yield return new WaitForSeconds(1);
        StartSlot.ActiveParticles.Stop();
        ScoreText.text = "";
    }

    public IEnumerator UpdateScore(int circlePower)
    {
        ScoreText.text = $"{circlePower}";
        yield return new WaitForSeconds(0.2f);
    }

    public IEnumerator EndDamage(int health, int maxHealth)
    {
        OpponentHealth.text = $"{health}";
        yield return new WaitForSeconds(1.5f);
        CameraAnim.SetTrigger("BackToIdle");
        yield return new WaitForSeconds(1.5f);
        CameraAnim.SetTrigger("Idle");
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

    public void ForceDestroyVisuals(Rune rune)
    {
        foreach (var slot in slots)
        {
            if (slot.Held != null && slot.Held.Rune == rune)
            {
                Destroy(slot.Held.gameObject);
                break;
            }
        }
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

        audioman.PlaySound(replaceSound, vis.transform.position);
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

    public IEnumerator ViewProgress(int currentRound)
    {
        ScrollAnimation.Play("OpenScrollPath");
        while (ScrollAnimation.isPlaying)
            yield return null;

        yield return Progress.Set(currentRound);

        ScrollAnimation.Play("CloseScroll");
        while (ScrollAnimation.isPlaying)
            yield return null;
    }

    public IEnumerator Shop()
    {
        ScrollAnimation.Play("OpenScrollShop");
        while (ScrollAnimation.isPlaying)
            yield return null;

        HUD.Instance.EndTurnButton.gameObject.SetActive(true);

        boughtCount = 0;

        ShopObject.SetActive(true);


        // Instantiate Shop Objects
        foreach(Transform origin in CardPackSlots)
        {
            Draggable cardPack = Instantiate(CardPackPrefab, origin.position, Quaternion.identity);
            shopObjects.Add(cardPack);
        }

        List<Rune> randomRuneShop = GetRunesToBuy(RandomRuneSlots.Length);
        for (int i = 0; i < RandomRuneSlots.Length; i++)
        {
            Transform origin = RandomRuneSlots[i];
            RuneVisuals vis = Instantiate(RunePrefab, origin.position, Quaternion.identity);
            Rune rune = randomRuneShop[i];
            vis.Init(rune, Player.Instance);
            shopObjects.Add(vis);
        }
        {
            RuneVisuals vis = Instantiate(RunePrefab, HealSlot.position, Quaternion.identity);
            vis.Init(Runes.GetRestore(), Player.Instance);
            shopObjects.Add(vis);
        }
        {
            RuneVisuals vis = Instantiate(RunePrefab, SellSlot.position, Quaternion.identity);
            vis.Init(Runes.GetPrune(), Player.Instance);
            shopObjects.Add(vis);
        }
        if ((Player.Instance.CurrentRound % 3) == 0)
        {
            for(int i = 0; i < 3; i++)
            {
                var allArtifacts = Artifacts.GetAllArtifacts();
                Gem gem = Instantiate(GemPrefab, ShopGemPoints[i].transform.position, Quaternion.identity);
                gem.Init(allArtifacts[Random.Range(0, allArtifacts.Count)]);
                shopObjects.Add(gem);
            }
        }

        HUD.Instance.EndTurnButton.onClick.AddListener(() => boughtCount = Player.Instance.ShopActions);
        HUD.Instance.EndTurnButton.interactable = true;

        while (boughtCount < Player.Instance.ShopActions)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (held == null)
            {
                if (ShopFeedback.isPlaying)
                    ShopFeedback.Stop();

                yield return UpdateHover(ray, true);
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

        ShopObject.SetActive(false);

        HUD.Instance.EndTurnButton.gameObject.SetActive(false);

        ScrollAnimation.Play("CloseScroll");
        while (ScrollAnimation.isPlaying)
            yield return null;
    }

    private List<Rune> GetRunesToBuy(int num)
    {
        List<Rune> runes = new List<Rune>();
        for (int i = 0; i < num; i++)
        {
            bool rare = Random.value > 0.7f;
            bool legendary = rare && Random.value > 0.7f;
            List<Rune> allRunes = legendary
                ? Runes.GetAllRunes(r => r.Rarity != Rarity.None)
                : rare
                    ? Runes.GetAllRunes(r => r.Rarity != Rarity.None && r.Rarity <= Rarity.Rare)
                    : Runes.GetAllRunes(r => r.Rarity != Rarity.None && r.Rarity <= Rarity.Common);

            List<string> names = runes.Select(r => r.Name).ToList();
            allRunes = allRunes.Where(r => !names.Contains(r.Name) && r.Name != Runes.GetRestore().Name && r.Name != Runes.GetPrune().Name).ToList();

            runes.Add(allRunes[Random.Range(0, allRunes.Count)]);
        }
        return runes;
    }

    private IEnumerator ShopRunes(Vector3 origin)
    {
        if(ShopFeedback.isPlaying)
            ShopFeedback.Stop();
        bool buying = true;

        HUD.Instance.EndTurnButton.onClick.RemoveAllListeners();
        HUD.Instance.EndTurnButton.onClick.AddListener(() => buying = false);


        List<Rune> runes = GetRunesToBuy(5);
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
                        yield return Inspect(vis, false, vis.transform.rotation, null);
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
        HUD.Instance.EndTurnButton.onClick.AddListener(() => boughtCount = Player.Instance.ShopActions);

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
        RuneVisuals vis = Instantiate(RunePrefab, RuneSpawn.position, Quaternion.identity);
        vis.Init(rune, Player.Instance);
        runes.Add(vis);
        /*
        vis.transform.position = new Vector3(
            Random.Range(-0.2f, 0.2f),
            1.0f,
            Random.Range(-2.5f, -1.5f));
        */
        var rigidBody = vis.GetComponent<Rigidbody>();
        rigidBody.AddForce(RuneSpawn.forward * 3 + Random.onUnitSphere * 0.3f, ForceMode.VelocityChange);

        yield return new WaitForSeconds(0.2f);
    }

    private IEnumerator InspectMany(List<Rune> runes, Transform origin)
    {
        Debug.Log("Inspect many");

        if (runes.Count == 0)
            yield break;
        
        yield return null;
    }

    private IEnumerator PlaceArtifact(Gem gem, GemSlot slot)
    {
        int index = Gems.IndexOf(gem);
        Player.Instance.PlaceArtifact(index, gem.Artifact);
        slot.ActiveParticles.Play();
        yield return null;
    }

    private void TakeArtifact(Gem gem, GemSlot slot)
    {
        Player.Instance.TakeArtifact(Gems.IndexOf(gem));
        slot.ActiveParticles.Stop();
    }

    private IEnumerator ViewOpponent()
    {
        CameraAnim.SetTrigger("ViewOpponent");
        yield return new WaitForSeconds(1.0f);

        while (Input.mousePosition.y > Screen.height * 0.75f)
            yield return null;

        CameraAnim.SetTrigger("BackToIdle");
        yield return new WaitForSeconds(1.0f);
        CameraAnim.SetTrigger("Idle");

    }

    private IEnumerator ViewSelf()
    {
        CameraAnim.SetTrigger("ViewSelf");
        yield return new WaitForSeconds(1.0f);

        while (Input.mousePosition.y < Screen.height * 0.25f)
            yield return null;

        CameraAnim.SetTrigger("BackFromSelf");
        yield return new WaitForSeconds(1.0f);
        CameraAnim.SetTrigger("Idle");

    }
}
