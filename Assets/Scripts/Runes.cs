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

    // B
    private static Rune Banish => new()
    {
        Name = "Banish",
        Power = 7,
        Rarity = Rarity.Common,
        Text = "On Activate: Exile a random Shard",
        OnActivate = (int selfIndex, Player player) =>
        {
            int numRunes = player.RunesInCircle();
            int rand = UnityEngine.Random.Range(0, numRunes);
            for (int i = 0; i < 5; i++)
            {
                if (player.HasRuneAtIndex(i))
                {
                    if (rand == 0)
                    {
                        player.Exile(i);
                        break;
                    }

                    rand--;
                }
            }
        },
    };
    // C
    private static Rune Cutt => new()
    {
        Name = "Cutt",
        Power = 7,
        Rarity = Rarity.Starter,
        Text = "On Play: Destroy neighbouring Shards",
        OnEnter = (int selfIndex, Player player) =>
        {
            player.Remove(selfIndex - 1);
            player.Remove(selfIndex + 1);
        },
    };
    // D
    private static Rune Drain => new()
    {
        Name = "Drain",
        Power = 16,
        Rarity = Rarity.Common,
        Text = "On Activate: Subtract half the power of all other Shards",
        OnActivate = (int selfIndex, Player player) =>
        {
            for (int i = 0; i < 5; i++)
            {
                if (i == selfIndex)
                    continue;

                Rune rune = player.GetRuneInCircle(i);
                if (rune != null)
                {
                    int currentPower = player.GetRunePower(i);
                    player.AddStats(rune, new() { Power = -(currentPower / 2) });
                }
            }
        },
    };
    // E
    private static Rune Explosive => new()
    {
        Name = "Explosive",
        Power = 0,
        Rarity = Rarity.Common,
        Text = "On Destroy: Add +10 to the summon",
        OnDestroy = (int selfIndex, Player player) =>
        {
            player.AddCirclePower(10);
        },
    };
    // F
    private static Rune Fake => new()
    {
        Name = "Fake",
        Power = 8,
        Rarity = Rarity.Common,
        Text = "On Activate: Transform next Shard to a random Shard",
        OnActivate = (int selfIndex, Player player) =>
        {
            int index = selfIndex + 1;
            Rune otherRune = player.GetRuneInCircle(index);
            if (otherRune != null)
            {
                List<Rune> allRunes = GetAllRunes();
                Rune rune = null;
                while (rune == null)
                {
                    Rune r = allRunes[UnityEngine.Random.Range(0, allRunes.Count)];
                    if (r.Name != otherRune.Name)
                        rune = r;
                }
                rune.Rarity = Rarity.None;

                player.Swap(rune, index);
            }
        },
    };
    private static Rune Focus => new()
    {
        Name  = "Focus",
        Power = 2,
        Rarity = Rarity.Common,
        Text  = "Power is multiplied by 3",
        Aura = new()
        {
            new()
            {
                Multiplier = 3,
                Application = (int selfIndex, int other, Player player) =>
                {
                    return selfIndex == other;
                },
            },
        },
    };
    // G
    private static Rune Growth => new()
    {
        Name = "Growth",
        Power = 5,
        Rarity = Rarity.Common,
        Text = "On Play: Permanently add +1 Power to the previous Shard",
        OnEnter = (int selfIndex, Player player) =>
        {
            Rune prev = player.GetRuneInCircle(selfIndex - 1);
            if (prev != null)
            {
                // Permanent
                prev.Power += 1;
            }
        },
    };
    // I
    private static Rune Inspire => new()
    {
        Name = "Inspire",
        Power = 8,
        Rarity = Rarity.Common,
        Text = "On Activate: Add +7 to the summon if the previous Shard is of higher rarity",
        OnActivate = (int selfIndex, Player player) =>
        {
            Rune self = player.GetRuneInCircle(selfIndex);
            Rune rune = player.GetRuneInCircle(selfIndex - 1);
            if (rune != null)
            {
                if (rune.Rarity != Rarity.None && (int)rune.Rarity > (int)self.Rarity)
                {
                    player.AddCirclePower(7);
                }
            }
        },
    };
    private static Rune Iron => new()
    {
        Name  = "Iron",
        Power = 2,
        Rarity = Rarity.Starter,
        Text  = "Neighbouring Shards has +5 Power",
        Aura = new()
        {
            new()
            {
                Power = 5,
                Application = (int selfIndex, int other, Player player) =>
                {
                    return Player.CircularIndex(selfIndex + 1) == other
                        || Player.CircularIndex(selfIndex - 1) == other;
                },
            },
        },
    };
    // P
    private static Rune Pool => new()
    {
        Name  = "Pool",
        Power = 5,
        Rarity = Rarity.Starter,
        Text  = "On Play: Draw 2 Shards",
        OnEnter = (int selfIndex, Player player) =>
        {
            player.Draw(2);
        },
    };
    private static Rune Prysm => new()
    {
        Name  = "Prysm",
        Power = 3,
        Rarity = Rarity.Starter,
        Text  = "Power is multiplied by 2",
        Aura = new()
        {
            new()
            {
                Multiplier = 2,
                Application = (int selfIndex, int other, Player player) =>
                {
                    return selfIndex == other;
                },
            },
        },
    };
    // R
    private static Rune Ravage => new()
    {
        Name = "Ravage",
        Power = 4,
        Rarity = Rarity.Common,
        Text = "On Activate: Activate the Shard two steps prior to this one",
        OnActivate = (int selfIndex, Player player) =>
        {
            player.Activate(selfIndex - 2);
        },
    };
    private static Rune Rebellious => new()
    {
        Name = "Rebellious",
        Power = 5,
        Rarity = Rarity.Common,
        Text = "Neighbours has +5 and Opposites has -5",
        Aura = new()
        {
            new()
            {
                Power = 5,
                Application = (int selfIndex, int other, Player player) =>
                {
                    return player.AreNeighbours(selfIndex, other);
                },
            },
            new()
            {
                Power = -5,
                Application = (int selfIndex, int other, Player player) =>
                {
                    return player.AreOpposites(selfIndex, other);
                },
            }
        },
    };
    private static Rune Rescue => new()
    {
        Name = "Rescue",
        Power = 7,
        Rarity = Rarity.Common,
        Text = "On Play: Conjure a Pool to your hand",
        OnEnter = (int selfIndex, Player player) =>
        {
            Rune pool = Pool;
            pool.Rarity = Rarity.None;
            player.AddNewRuneToHand(pool);
        },
    };
    private static Rune Restore => new()
    {
        Name = "Restore",
        Power = 0,
        Rarity = Rarity.Common,
        Text = "On Play: Add +2 life then Exile this Shard",
        OnEnter = (int selfIndex, Player player) =>
        {
            player.AddLife(2);
            player.Exile(selfIndex);
        },
    };
    // S
    private static Rune Shore => new()
    {
        Name = "Shore",
        Power = 0,
        Rarity = Rarity.Common,
        Text = "On Activate: Permanently add +2 Power to the previous Shard",
        OnActivate = (int selfIndex, Player player) =>
        {
            Rune prev = player.GetRuneInCircle(selfIndex - 1);
            if (prev != null)
            {
                // Permanent
                prev.Power += 2;
            }
        },
    };
    private static Rune Start => new()
    {
        Name = "Start",
        Power = 5,
        Rarity = Rarity.Common,
        Text = "On Play: Add +5 to the summon",
        OnEnter = (int selfIndex, Player player) =>
        {
            player.AddCirclePower(5);
        },
    };
    private static Rune Strike => new()
    {
        Name  = "Strike",
        Power = 10,
        Rarity = Rarity.Starter,
        Text  = "",
    };
}
