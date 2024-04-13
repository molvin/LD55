using System;
using System.Collections;
using System.Collections.Generic;

public static class Runes
{
    public static List<Rune> GetAllRunes(int minCost = 0, int maxCost = 10)
    {
        var runeImpls = typeof(Runes).GetMethods(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);

        List<Rune> runes = new();
        foreach (var impl in runeImpls)
        {
            if (impl.ReturnType == typeof(Rune))
            {
                Rune rune = impl.Invoke(null, null) as Rune;
                if (rune.Name != null && !rune.Name.StartsWith('_') && rune.Cost >= minCost && rune.Cost <= maxCost)
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

    // Fehu
    private static Rune Fehu => new()
    {
        Name  = "Fehu",
        Power = 1,
        OnEnter = (int selfIndex, Player player) =>
        {
            Rune left = player.GetRuneInCircle(selfIndex - 1);
            Rune right = player.GetRuneInCircle(selfIndex + 1);
            
            if (left != null)
                left.Power += 1;

            if (right != null)
                right.Power += 1;
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

    private static Rune Tal => new()
    {
        /*
        Name = "Tal",
        Type = { Type.Creature },
        Alignment = Alignment.Pure,
        Kind = { Kind.Gargantuon },
        Cost = 5,
        Speed = 2,
        Power = 2,
        Health = 2,
        Text = $"{Trigger.OnDeath}: Summon a 4/4 Paternal Ursidae",
        OnDeath = (Entity ent, Entity _, Board board) =>
        {
            var (ok, index) = board.FindPos(ent);
            if (ok)
            {
                board.PushToStack(CombatStep.Summon(() =>
                {
                    Entity newEntity = board.CreateEntityFromCard(PaternalUrsidae);
                    board.ResolveCard(newEntity, index);
                    return newEntity;
                }));
            }
        },
        */
    };
}
