using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Artifacts
{
    public static List<Artifact> GetAllArtifacts(Func<Artifact, bool> Predicate = null)
    {
        var artifactImpls = typeof(Artifacts).GetMethods(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);

        List<Artifact> artifacts = new();
        foreach (var impl in artifactImpls)
        {
            if (impl.ReturnType == typeof(Artifact))
            {
                Artifact artifact = impl.Invoke(null, null) as Artifact;
                if (artifact.Name != null && !artifact.Name.StartsWith('_'))
                {
                    if (Predicate == null || Predicate(artifact))
                    {
                        artifacts.Add(artifact);
                    }
                }
            }
        }

        return artifacts;
    }
    public static Artifact GetAquamarine => Aquamarine;
    public static Artifact GetAzurite => Azurite;
    public static Artifact GetMuscovite => Muscovite;
    public static Artifact GetScheelite => Scheelite;
    public static Artifact GetWitherite => Witherite;

    // A
    private static Artifact Acanthite => new()
    {
        Name = "Acanthite",
        Text = "Increase Shop actions by 1",
        Limit = 2,
        Stats = new()
        {
            ShopActions = 1,
        },
    };
    private static Artifact Aquamarine => new()
    {
        Name = "Aquamarine",
        Text  = "When £ activates it's Power is multiplied by 2",
        Limit = 2,
        RuneTrigger = (TriggerType trigger, int runeIndex, Player player) =>
        {
            Rune rune = player.GetRuneInCircle(runeIndex);
            if (trigger == TriggerType.OnActivate && rune != null)
            {
                if (rune.Keywords != null && rune.Keywords.Contains(Keywords.Energy))
                {
                    int runePower = player.GetRunePower(runeIndex);
                    player.MultiplyPower(runeIndex, 2);
                    return new() { EventHistory.PowerToRune(runeIndex, runePower) };
                }
            }

            return new();
        },
    };
    private static Artifact Azurite => new()
    {
        Name = "Azurite",
        Text = "£ Shards has +2 Power for each other £ Shard. Conjure a £ Shard every summon",
        Limit = 4,
        Draw = () =>
        {
            return Runes.GetEnergy();
        },
        Buff = (int runeIndex, Player player) =>
        {
            Rune active = player.GetRuneInCircle(runeIndex);
            if (active == null || active.Keywords == null || !active.Keywords.Contains(Keywords.Energy))
            {
                return 0;
            }

            int buff = 0;
            for (int i = 0; i < 5; i++)
            {
                if (i == runeIndex)
                    continue;

                Rune rune = player.GetRuneInCircle(i);
                if (rune != null && rune.Keywords != null && rune.Keywords.Contains(Keywords.Energy))
                {
                    buff++;
                }
            }
            return buff * 2;
        },
    };
    // M
    private static Artifact Malechite => new()
    {
        Name = "Malechite",
        Text = "Draw an additional Shard each Summon",
        Limit = 3,
        Stats = new()
        {
            HandSize = 1,
        },
    };
    private static Artifact Muscovite => new()
    {
        Name = "Muscovite",
        Text = "Add +10 to the summon whenever a Shard is destroyed",
        Limit = 4,
        RuneTrigger = (TriggerType trigger, int runeIndex, Player player) =>
        {
            if (trigger == TriggerType.OnDestroy)
            {
                player.AddCirclePower(10);
                return new() { EventHistory.PowerToSummon(player.GetCirclePower()) };
            }

            return new();
        },
    };
    // S
    private static Artifact SmokyQuartz => new()
    {
        Name = "Smoky Quartz",
        Text = "Resummon up to 1 finger at the start of each new summon",
        Limit = 2,
        Stats = new()
        {
            Regen = -1,
        },
    };
    private static Artifact Scheelite => new()
    {
        Name = "Scheelite",
        Text = "Permanently conjure 2 random Shards whenever a Shard is exiled",
        Limit = 2,
        RuneTrigger = (TriggerType trigger, int runeIndex, Player player) =>
        {
            List<EventHistory> history = new();
            
            if (trigger == TriggerType.OnExile)
            {
                List<Rune> runes = Runes.GetAllRunes();
                List<Rune> drawn = new();
                for (int i = 0; i < 2; i++)
                {
                    int idx = UnityEngine.Random.Range(0, runes.Count);
                    Rune rune = runes[idx];
                    runes.RemoveAt(idx);

                    player.Buy(rune);
                    player.AddNewRuneToHand(rune);
                    drawn.Add(rune);
                }
                history.Add(EventHistory.Draw(drawn.ToArray()));
            }

            return history;
        },
    };
    // W
    private static Artifact Witherite => new()
    {
        Name = "Witherite",
        Text = "Conjure a random Shard whenever a Shard is destroyed",
        Limit = 2,
        RuneTrigger = (TriggerType trigger, int runeIndex, Player player) =>
        {
            if (trigger == TriggerType.OnDestroy)
            {
                var runes = Runes.GetAllRunes();
                Rune rune = runes[UnityEngine.Random.Range(0, runes.Count)];
                rune.Token = true;
                player.AddNewRuneToHand(rune);
                return new() { EventHistory.Draw(rune) };
            }

            return new();
        },
    };
}
