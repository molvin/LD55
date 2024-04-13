using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using TMPro;

public enum Location
{
    None,
    Bag,
    Hand,
    Circle,
}

public class Player : MonoBehaviour
{
    public static int CircularIndex(int index) => index < 0 ? Settings.NumSlots + index : index >= Settings.NumSlots ? index - Settings.NumSlots : index;
    public static Player Instance;

    public RuneVisuals RuneVisualPrefab;
    public List<RuneRef> BaseDeck;

    private List<Rune> bag = new();
    private List<Rune> hand = new();
    private List<Rune> circle = new(new Rune[Settings.NumSlots]);
    private List<Rune> discardPile = new();
    private int circlePower;
    private int health = Settings.PlayerMaxHealth;

    private RuneBoard runeBoard;


    public bool CircleIsFull => circle.All(rune => rune != null);
    public Rune GetRuneInCircle(int index) => circle[CircularIndex(index)];
    public int GetCirclePower() => circlePower;
    public int GetIndexOfRune(Rune rune) => circle.IndexOf(rune);
    public int GetRunePower(int runeIndex)
    {
        runeIndex = CircularIndex(runeIndex);
        Rune rune = circle[runeIndex];
        int runePower = rune.Power;

        for (int i = 0; i < Settings.NumSlots; i++)
        {
            Rune other = circle[i];
            if (other != null && other.Aura.IsValid)
            {
                if (other.Aura.Application.Invoke(runeIndex, i, this))
                {
                    runePower += other.Aura.Power;
                }
            }
        }

        return runePower;
    }

    public void ForEach(Location location, Action<Rune> action) {
        List<Rune> collection = new();
        switch (location)
        {
            case Location.None:
                return;
            case Location.Bag:
                collection = bag;
                break;
            case Location.Hand:
                collection = hand;
                break;
            case Location.Circle:
                collection = circle;
                break;
        }

        foreach (Rune rune in collection)
            if (rune != null)
                action(rune);
    }

    public void Place(Rune rune, int slot)
    {
        hand.Remove(rune);
        circle[slot] = rune;
        rune.OnEnter?.Invoke(slot, this);
    }

    private IEnumerator ResolveCircle()
    {
        for (int i = 0; i < circle.Count; i++)
        {
            if (circle[i] == null)
                continue;

            circlePower += GetRunePower(i);
            yield return new WaitForSeconds(0.3f);
            runeBoard.RemoveRune(circle[i]);
            yield return new WaitForSeconds(0.3f);
        }

        Debug.Log($"DEALING DAMAGE: {circlePower}");

        ClearCircle();
    }

    private void ClearCircle()
    {
        for (int i = 0; i < circle.Count; i++)
        {
            if (circle[i] != null)
            {
                discardPile.Add(circle[i]);
                circle[i] = null;
            }
        }

        circlePower = 0;
    }

    private IEnumerator DrawHand()
    {
        while (bag.Count > 0 && hand.Count < Settings.HandSize)
        {
            Rune rune = bag[0];
            hand.Add(rune);
            bag.RemoveAt(0);
            /*
            RuneVisuals runeVisual = Instantiate(RuneVisualPrefab);
            runeVisual.Init(rune, this);
            runeBoard.AddRune(runeVisual);

            runeVisual.transform.position = new Vector3(
                UnityEngine.Random.Range(-0.5f, 0.5f),
                1.0f,
                UnityEngine.Random.Range(-2.5f, -1.5f));
            var rigidBody = runeVisual.GetComponent<Rigidbody>();
            rigidBody.AddForce(UnityEngine.Random.onUnitSphere, ForceMode.VelocityChange);
            */
            yield return new WaitForSeconds(0.2f);
        }
    }

    private void Awake()
    {
        Instance = this;
        runeBoard = FindObjectOfType<RuneBoard>();
    }

    private void Start()
    {
        foreach (RuneRef runeRef in BaseDeck)
        {
            Rune rune = runeRef.Get();
            bag.Add(rune);
        }

        bag.Shuffle();
        StartCoroutine(Game());
    }

    private IEnumerator Game()
    {
        while(health > 0)
        {
            List<Rune> hand = Draw();
            yield return runeBoard.Draw(hand);
            yield return runeBoard.Play();

            for (int i = 0; i < circle.Count; i++)
            {
                if (circle[i] == null)
                    continue;

                int power = GetRunePower(i);
                circlePower += power;

                yield return runeBoard.Resolve(i, power, circlePower);
            }

            Debug.Log($"DEALING DAMAGE: {circlePower}");
            ClearCircle();

            // TODO: Discard
        }

        Debug.Log("You lose");

        yield return null;
    }

    private List<Rune> Draw()
    {
        List<Rune> temp = new();
        while (bag.Count > 0 && temp.Count < Settings.HandSize)
        {
            Rune rune = bag[0];
            temp.Add(rune);
            bag.RemoveAt(0);
        }
        return temp;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            foreach (Rune rune in hand)
            {
                runeBoard.RemoveRune(rune);
            }
            hand.Clear();

            StartCoroutine(ResolveCircle());
            StartCoroutine(DrawHand());
        }
    }
}
