using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

public static class Runes
{
    public static List<Rune> GetAllRunes(Func<Rune, bool> Predicate = null)
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
                    if (Predicate == null || Predicate(rune))
                    {
                        runes.Add(rune);
                    }
                }
            }
        }

        return runes;
    }


    // ---------------------------------------------------------------------------------------------------------
    // Implemented Runes
    // ---------------------------------------------------------------------------------------------------------

    // A
    private static Rune Avarice => new()
    {
        Name = "Avarice",
        Power = -20,
        Rarity = Rarity.Common,
        Text = "On Activate: The summon power is multiplied by 3",
        OnActivate = (int selfIndex, Player player) =>
        {
            int circlePower = player.GetCirclePower();
            player.AddCirclePower(circlePower * 2);
        },
    };
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
    private static Rune Bonfire => new()
    {
        Name = "Bonfire",
        Power = -5,
        Rarity = Rarity.Common,
        Text = "On Activate: For each Shard with greater than 10 Power, add +5 to the summon",
        OnActivate = (int selfIndex, Player player) =>
        {
            int activations = 0;
            for (int i = 0; i < 5; i++)
            {
                if (player.HasRuneAtIndex(i) && player.GetRunePower(i) > 10)
                {
                    activations++;
                }
            }
            player.AddCirclePower(5 * activations);
        },
    };
    // C
    private static Rune Cut => new()
    {
        Name = "Cut",
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
        Text = "On Activate: Divide the Power of all other Shards by 2",
        OnActivate = (int selfIndex, Player player) =>
        {
            for (int i = 0; i < 5; i++)
            {
                if (i == selfIndex)
                    continue;

                if (player.HasRuneAtIndex(i))
                {
                    player.MultiplyPower(i, 0.5f);
                }
            }
        },
    };
    // E
    private static Rune Energy => new()
    {
        Name  = "Energy",
        Power = 10,
        Rarity = Rarity.Starter,
        Text  = "",
    };
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
                rune.Token = true;

                player.Swap(rune, index);
            }
        },
    };
    private static Rune Fetch => new()
    {
        Name  = "Fetch",
        Power = 8,
        Rarity = Rarity.Common,
        Text  = "On Play: Conjure 2 Energy to hand",
        OnEnter = (int selfIndex, Player player) =>
        {
            for (int i = 0; i < 2; i++)
            {
                Rune energy = Energy;
                energy.Token = true;
                player.AddNewRuneToHand(energy);
            }
        },
    };
    private static Rune Flash => new()
    {
        Name  = "Flash",
        Power = 5,
        Rarity = Rarity.Common,
        Text  = "On Play: Add Power equal to 5 times the summon Power",
        OnEnter = (int selfIndex, Player player) =>
        {
            int circlePower = player.GetCirclePower();
            player.AddStats(player.GetRuneInCircle(selfIndex), new() { Power = circlePower * 5 });
        },
    };
    private static Rune Focus => new()
    {
        Name  = "Focus",
        Power = 2,
        Rarity = Rarity.Common,
        Text  = "On Activate: Power is multiplied by 3",
        OnActivate = (int selfIndex, Player player) =>
        {
            player.MultiplyPower(selfIndex, 3);
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
    // H
    private static Rune Help => new()
    {
        Name = "Help",
        Power = 5,
        Rarity = Rarity.Common,
        Text = "On Play: Next Shards Power is multiplied by 2",
        OnEnter = (int selfIndex, Player player) =>
        {
            player.MultiplyPower(selfIndex + 1, 2);
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
                if ((int)rune.Rarity > (int)self.Rarity)
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
    // M
    private static Rune Martyre => new()
    {
        Name  = "Martyre",
        Power = 8,
        Rarity = Rarity.Common,
        Text  = "On Exile: Permanently add +10 Power to the previous Shard",
        OnExile = (int selfIndex, Player player) =>
        {
            Rune prev = player.GetRuneInCircle(selfIndex - 1);
            if (prev != null)
            {
                // Permanent
                prev.Power += 10;
            }
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
            player.Draw(2, true);
        },
    };
    private static Rune Prune => new()
    {
        Name  = "Prune",
        Power = 0,
        Rarity = Rarity.None,
        Text  = "On Activate: Exile previous Shard, then Exile this",
        OnActivate = (int selfIndex, Player player) =>
        {
            player.Exile(selfIndex - 1);
            player.Exile(selfIndex);
        },
    };
    private static Rune Prysm => new()
    {
        Name  = "Prysm",
        Power = 3,
        Rarity = Rarity.Starter,
        Text  = "On Activate: Power is multiplied by 2",
        OnActivate = (int selfIndex, Player player) =>
        {
            player.MultiplyPower(selfIndex, 2);
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
    private static Rune Repeat => new()
    {
        Name = "Repeat",
        Power = 10,
        Rarity = Rarity.Common,
        Text = "Has +10 Power if there is no Shard in the next slot",
        Aura = new()
        {
            new()
            {
                Power = 10,
                Application = (int selfIndex, int other, Player player) =>
                {
                    return selfIndex == other && !player.HasRuneAtIndex(selfIndex + 1);
                },
            },
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
            pool.Token = true;
            player.AddNewRuneToHand(pool);
        },
    };
    private static Rune Restore => new()
    {
        Name = "Restore",
        Power = 0,
        Rarity = Rarity.None,
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
    // T
    private static Rune Tides => new()
    {
        Name  = "Tides",
        Power = 7,
        Rarity = Rarity.Common,
        Text  = "On Play: Swap the next Shard with the previous Shard",
        OnEnter = (int selfIndex, Player player) =>
        {
            player.Swap(selfIndex - 1, selfIndex + 1);
        },
    };
}
