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


    // M
    private static Artifact Malechite => new()
    {
        Name = "Malechite",
        Text = "Draw an additional Shard each Summon",
        OnEnter = (int selfIndex, Player player) =>
        {
            player.MaxHandSize++;
            return new();
        },
        OnExit = (int selfIndex, Player player) =>
        {
            player.MaxHandSize--;
            return new();
        },
    };
}
