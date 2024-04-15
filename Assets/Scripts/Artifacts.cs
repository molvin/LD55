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
    private static Artifact Witherite => new()
    {
        Name = "Witherite",
        Text = "Draw a Shard whenever a Shard is destroyed",
        Limit = 2,
        RuneTrigger = (TriggerType trigger, int runeIndex, Player player) =>
        {
            if (trigger == TriggerType.OnDestroy)
            {
                var drawn = player.Draw(1);
                return new() { EventHistory.Draw(drawn.ToArray()) };
            }

            return new();
        },
    };
}
