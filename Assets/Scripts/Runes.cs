using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

public static class Runes
{
    public static List<Rune> GetAllRunes()
    {
        var runeImpls = typeof(Runes).GetMethods(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);

        List<Rune> runes = new();
        foreach (var impl in runeImpls)
        {
            if (impl.ReturnType == typeof(Rune))
            {
                Rune rune = impl.Invoke(null, null) as Rune;
                if (rune.Name != null && !rune.Name.StartsWith('_'))
                {
                    runes.Add(rune);
                }
            }
        }

        return runes;
    }


    // ---------------------------------------------------------------------------------------------------------
    // Implemented Runes
    // ---------------------------------------------------------------------------------------------------------

    // I
    private static Rune Iron => new()
    {
        Name  = "Iron",
        Power = 2,
        Text  = "Neighbouring shards has +5 Power",
        Aura =
        {
            Power = 5,
            Application = (int selfIndex, int other, Player player) =>
            {
                return Player.CircularIndex(selfIndex + 1) == other
                    || Player.CircularIndex(selfIndex - 1) == other;
            },
        },
    };
    // P
    private static Rune Prysm => new()
    {
        Name  = "Prysm",
        Power = 3,
        Rarity = Rarity.Starter,
        Text  = "",
        Aura =
        {
            Multiplier = 2,
            Application = (int selfIndex, int other, Player player) =>
            {
                return selfIndex == other;
            },
        },
    };
    // S
    private static Rune Strike => new()
    {
        Name  = "Strike",
        Power = 10,
        Rarity = Rarity.Starter,
        Text  = "",
    };

    // Fehu
    private static Rune Fehu => new()
    {
        Name  = "Fehu",
        Power = 1,
        Text  = "Gives runes to the side +1 power when placed",
        OnEnter = (int selfIndex, Player player) =>
        {
            Rune prev = player.GetRuneInCircle(selfIndex - 1);
            Rune next = player.GetRuneInCircle(selfIndex + 1);

            TempStats stats = new() { Power = 1, Multiplier = 1 };
            if (prev != null)
                player.AddStats(prev, stats);

            if (next != null)
                player.AddStats(next, stats);
        },
    };
    // Uruz
    private static Rune Uruz => new()
    {
        Name  = "Uruz",
        Power = 2,
    };
    // Thurisaz
    private static Rune Thurisaz => new()
    {
        Name  = "Thurisaz",
        Power = 3,
    };
    // Ansuz
    private static Rune Ansuz => new()
    {
        Name  = "Ansuz",
        Power = 4,
    };
    private static Rune Raidho => new()
    {
        Name  = "Raidho",
        Power = 1,
        Text  = "Runes opposite to this has +1 Power",
        Aura =
        {
            Power = 1,
            Application = (int selfIndex, int other, Player player) =>
            {
                return Player.CircularIndex(selfIndex + 2) == other
                    || Player.CircularIndex(selfIndex - 2) == other;
            },
        },
    };
    // Raidho
    // Kenaz
    // Gebo
    // Wunjo
    // Hagal
    // Naudiz
    // Isa
    // Eiwaz
    // Pertho
    // Algiz
    // Sowilo
    // Tyr
    // Ehwaz
    // Mannaz
    // Laguz
    // Ingwaz
    // Dagaz
}
