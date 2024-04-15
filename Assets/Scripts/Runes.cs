using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

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

    public static Rune GetRestore() => Restore;
    public static Rune GetPrune() => Prune;

    // ---------------------------------------------------------------------------------------------------------
    // Implemented Runes
    // ---------------------------------------------------------------------------------------------------------

    // A
    private static Rune Abundance => new()
    {
        Name = "Abundance",
        Power = 10,
        Rarity = Rarity.Rare,
        Text = "On Activate: Add 10 times the number of Shards in hand to the summon",
        OnActivate = (int selfIndex, Player player) =>
        {
            if (player.HandSize > 0)
            {
                player.AddCirclePower(player.HandSize * 10);
                return new() { EventHistory.PowerToSummon(player.GetCirclePower()) };
            }

            return new();
        },
    };
    private static Rune Ally => new()
    {
        Name = "Ally",
        Power = 0,
        Rarity = Rarity.Common,
        Text = "On Activate: Permanently add +1 Power to neighbouring Shards",
        OnActivate = (int selfIndex, Player player) =>
        {
            List<EventHistory> history = new();
            int idx1 = Player.CircularIndex(selfIndex - 1);
            int idx2 = Player.CircularIndex(selfIndex + 1);
            if (player.HasRuneAtIndex(idx1))
            {
                Rune rune = player.GetRuneInCircle(idx1);
                // Permanent
                rune.Power += 1;
                history.Add(EventHistory.PowerToRune(idx1, 1));
            }
            if (player.HasRuneAtIndex(idx2))
            {
                Rune rune = player.GetRuneInCircle(idx2);
                // Permanent
                rune.Power += 1;
                history.Add(EventHistory.PowerToRune(idx2, 1));
            }

            return history;
        },
    };
    private static Rune Assist => new()
    {
        Name  = "Assist",
        Power = 4,
        Rarity = Rarity.Common,
        Text  = "On Play: Conjure 2 Pain to hand",
        OnEnter = (int selfIndex, Player player) =>
        {
            List<Rune> drawn = new();
            for (int i = 0; i < 2; i++)
            {
                Rune pain = Pain;
                pain.Token = true;
                player.AddNewRuneToHand(pain);
                drawn.Add(pain);
            }

            return new() { EventHistory.Draw(drawn.ToArray()) };
        },
    };
    private static Rune Avarice => new()
    {
        Name = "Avarice",
        Power = -60,
        Rarity = Rarity.Common,
        Text = "On Activate: The summon power is multiplied by 3",
        OnActivate = (int selfIndex, Player player) =>
        {
            int circlePower = player.GetCirclePower();
            player.AddCirclePower(circlePower * 2);
            return new() { EventHistory.PowerToSummon(player.GetCirclePower()) };
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
            int selected = -1;
            for (int i = 0; i < 5; i++)
            {
                if (player.HasRuneAtIndex(i))
                {
                    if (rand == 0)
                    {
                        selected = i;
                        break;
                    }

                    rand--;
                }
            }

            List<EventHistory> history = new() { EventHistory.Exile(selected) };
            history.AddRange(player.Exile(selected));
            return history;
        },
    };
    private static Rune Bolder => new()
    {
        Name = "Bolder",
        Power = 4,
        Rarity = Rarity.Common,
        Text = "On Play: Add +10 Power to a random Shard",
        OnEnter = (int selfIndex, Player player) =>
        {
            int numRunes = player.RunesInCircle();
            int rand = UnityEngine.Random.Range(0, numRunes);
            int selected = -1;
            for (int i = 0; i < 5; i++)
            {
                if (player.HasRuneAtIndex(i))
                {
                    if (rand == 0)
                    {
                        selected = i;
                        break;
                    }

                    rand--;
                }
            }

            player.AddStats(selected, new() { Power = 10 });

            return new() { EventHistory.PowerToRune(selected, 10) };
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
            return new() { EventHistory.PowerToSummon(player.GetCirclePower()) };
        },
    };
    private static Rune Bud => new()
    {
        Name = "Bud",
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
                return new() { EventHistory.PowerToRune(player.GetIndexOfRune(prev), 1) };
            }

            return new();
        },
    };
    private static Rune Bounce => new()
    {
        Name = "Bounce",
        Power = 6,
        Rarity = Rarity.Common,
        Text = "On Play: Return the previous Shard to hand",
        OnEnter = (int selfIndex, Player player) =>
        {
            int prev = Player.CircularIndex(selfIndex - 1);
            if (player.HasRuneAtIndex(prev))
            {
                player.ReturnToHand(prev);
                return new() { EventHistory.ReturnToHand(prev) };
            }

            return new();
        },
    };
    private static Rune Bulking => new()
    {
        Name = "Bulking",
        Power = 5,
        Rarity = Rarity.Common,
        Text = "On Play: Permanently add +1 Power to this shard",
        OnEnter = (int selfIndex, Player player) =>
        {
            Rune rune = player.GetRuneInCircle(selfIndex);
            // Permanent
            rune.Power += 1;
            return new() { EventHistory.PowerToRune(selfIndex, 1) };
        },
    };
    private static Rune Burn => new()
    {
        Name = "Burn",
        Power = 6,
        Rarity = Rarity.Rare,
        Text = "On Play: Add the previous Shards Power to the summon then destroy the previous",
        OnEnter = (int selfIndex, Player player) =>
        {
            List<EventHistory> history = new();

            int prev = Player.CircularIndex(selfIndex - 1);
            if (player.HasRuneAtIndex(prev))
            {
                int power = player.GetRunePower(prev);
                player.AddCirclePower(power);
                history.Add(EventHistory.PowerToSummon(player.GetCirclePower()));

                history.Add(EventHistory.Destroy(prev));
                history.AddRange(player.Remove(prev));
            }

            return history;
        },
    };
    // C
    private static Rune Cut => new()
    {
        Name = "Cut", 
        Power = 7,
        Rarity = Rarity.Starter,
        StartCount = 1,
        Text = "On Play: Destroy neighbouring Shards",
        OnEnter = (int selfIndex, Player player) =>
        {
            List<EventHistory> history = new();
            int idx1 = Player.CircularIndex(selfIndex - 1);
            int idx2 = Player.CircularIndex(selfIndex + 1);

            if (player.HasRuneAtIndex(idx1))
            {
                history.Add(EventHistory.Destroy(idx1));
                history.AddRange(player.Remove(idx1));
            }
            if (player.HasRuneAtIndex(idx2))
            {
                history.Add(EventHistory.Destroy(idx2));
                history.AddRange(player.Remove(idx2));
            }

            return history;
        },
    };
    // D
    private static Rune Dam => new()
    {
        Name = "Dam",
        Power = 0,
        Rarity = Rarity.None,
        Text = "On Play: Add +10 to the summon. Exile this Shard",
        OnEnter = (int selfIndex, Player player) =>
        {
            List<EventHistory> history = new();

            player.AddCirclePower(10);
            history.Add(EventHistory.PowerToSummon(player.GetCirclePower()));
            history.Add(EventHistory.Exile(selfIndex));
            history.AddRange(player.Exile(selfIndex));

            return history;
        },
    };
    private static Rune DevilsLuck => new()
    {
        Name = "Devils Luck",
        Power = 0,
        Rarity = Rarity.Rare,
        Text = "On Play: Transform into a random Legendary",
        OnEnter = (int selfIndex, Player player) =>
        {
            Rune thisRune = player.GetRuneInCircle(selfIndex);
            if (thisRune != null)
            {
                List<Rune> allRunes = GetAllRunes((rune) => { return rune.Rarity > Rarity.Rare; });
                Rune rune = allRunes[UnityEngine.Random.Range(0, allRunes.Count)];
                if(rune != null)
                {
                    player.Replace(rune, selfIndex);
                    return new() { EventHistory.Replace(selfIndex, rune) };
                }
            }

            return null;
        },
    };
    private static Rune Discovery => new()
    {
        Name  = "Discovery",
        Power = -5,
        Rarity = Rarity.Common,
        Text  = "On Play: Draw 3 Shards",
        OnEnter = (int selfIndex, Player player) =>
        {
            List<Rune> drawn = player.Draw(3);
            if (drawn.Count > 0)
            {
                return new() { EventHistory.Draw(drawn.ToArray()) };
            }

            return new();
        },
    };
    private static Rune Displacement => new() //NOT WORK
    {
        Name = "Displacement",
        Power = 16,
        Rarity = Rarity.Rare,
        Text = "On Play: return Opposite Shards to hand",
        OnEnter = (int selfIndex, Player player) =>
        {
            List<EventHistory> history = new();
            int op_1 = Player.CircularIndex(selfIndex - 2);
            int op_2 = Player.CircularIndex(selfIndex + 2);
            if (player.HasRuneAtIndex(op_1))
            {
                player.ReturnToHand(op_1);
                history.Add(EventHistory.ReturnToHand(op_1));
            }
            if (player.HasRuneAtIndex(op_2))
            {
                player.ReturnToHand(op_2);
                history.Add(EventHistory.ReturnToHand(op_2));

            }
            return history;
        },
    };
    private static Rune Divinity => new()
    {
        Name = "Divinity",
        Power = 30,
        Rarity = Rarity.Legendary,
        Text = "On Play: Transform All other Shards in circle to random Shards. All Shards has +5 Power",
        OnEnter = (int selfIndex, Player player) =>
        {
            List<EventHistory> history = new();
            List<Rune> allRunes = GetAllRunes();
            List<int> replacedRunes =
                new List<int>() { 1, 2, -1, -2 }.Select(e =>
                {
                    int index = Player.CircularIndex(selfIndex + e);
                    Rune original = player.GetRuneInCircle(index);
                    if (original != null)
                    {
                        Rune rune = null;
                        while (rune == null)
                        {
                            Rune r = allRunes[UnityEngine.Random.Range(0, allRunes.Count)];
                            if (r.Name != original.Name)
                                rune = r;
                        }
                        rune.Token = true;

                        player.Replace(rune, index);
                        history.Add(EventHistory.Replace(index, rune));

                        return index;
                    }

                    return -1;

                }).Where(e => e >= 0).ToList<int>();

            /* NOTE: Moved to Aura
            TempStats statss = new TempStats();
            statss.Power = 5;
            player.AddStats(selfIndex, statss);
            history.Add(EventHistory.PowerToRune(selfIndex, 5));
            foreach (int r in replacedRunes)
            {
                TempStats stats = new TempStats();
                stats.Power = 5;
                player.AddStats(r, stats);
                history.Add(EventHistory.PowerToRune(r, 5));
            }
            */

            return history;
        },
        Aura = new()
        {
            new()
            {
                Power = 5,
                Application = (int selfIndex, int other, Player player) =>
                {
                    Rune rune = player.GetRuneInCircle(other);
                    return rune != null;
                },
            },
        },
    };
    private static Rune Drain => new()
    {
        Name = "Drain",
        Power = 18,
        Rarity = Rarity.Common,
        Text = "On Activate: Divide the Power of all other Shards by 2",
        OnActivate = (int selfIndex, Player player) =>
        {
            List<EventHistory> history = new();

            for (int i = 0; i < 5; i++)
            {
                if (i == selfIndex)
                    continue;

                if (player.HasRuneAtIndex(i))
                {
                    player.MultiplyPower(i, 0.5f);
                    history.Add(EventHistory.PowerToRune(i, player.GetRunePower(i)));
                }
            }

            return history;
        },
    };
    // E
    private static Rune Energy => new()
    {
        Name  = "Energy",
        Power = 10,
        Rarity = Rarity.Starter,
        StartCount = 5,
        Keywords = { Keywords.Energy },
        Text  = "",
    };
    private static Rune Experiment => new()
    {
        Name = "Experiment",
        Power = 15,
        Rarity = Rarity.Rare,
        Text = "On Play: Discard hand and draw 5",
        OnEnter = (int selfIndex, Player player) =>
        {
            List<EventHistory> history = new List<EventHistory> { EventHistory.Discard(player.GetRunesInHand()), EventHistory.Draw(player.Draw(true, 5).ToArray()) };
            return history;
        },
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
            return new() { EventHistory.PowerToSummon(player.GetCirclePower()) };
        },
    };
    // F
    private static Rune Faces => new()
    {
        Name = "Faces",
        Power = 2,
        Rarity = Rarity.Rare,
        Text = "On Activate: Transform next Shard to the previous Shard",
        OnActivate = (int selfIndex, Player player) =>
        {
            int prev = Player.CircularIndex(selfIndex - 1);
            int next = Player.CircularIndex(selfIndex + 1);

            if (!player.HasRuneAtIndex(next))
            {
                return new();
            }
            if (!player.HasRuneAtIndex(prev))
            {
                player.Remove(next, true);
                // silent
                return new();
            }

            Rune prevRune = player.GetRuneInCircle(prev);
            Rune clone = GetAllRunes(r => r.Name == prevRune.Name).First();
            clone.Token = true;
            TempStats prevStats = player.GetTempStats(prev);
            clone.Power = prevRune.Power + prevStats.Power;

            player.Replace(clone, next);
            return new() { EventHistory.Replace(next, clone) };
        },
    };
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

                player.Replace(rune, index);
                return new() { EventHistory.Replace(index, rune) };
            }

            return new();
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
            List<Rune> drawn = new();
            for (int i = 0; i < 2; i++)
            {
                Rune energy = Energy;
                energy.Token = true;
                player.AddNewRuneToHand(energy);
                drawn.Add(energy);
            }

            return new() { EventHistory.Draw(drawn.ToArray()) };
        },
    };
    private static Rune Flash => new()
    {
        Name  = "Flash",
        Power = 5,
        Rarity = Rarity.Common,
        Text  = "On Play: Add Power to this Shard equal 5 times the summon",
        OnEnter = (int selfIndex, Player player) =>
        {
            int circlePower = player.GetCirclePower();
            player.AddStats(player.GetRuneInCircle(selfIndex), new() { Power = circlePower * 5 });
            return new() { EventHistory.PowerToRune(selfIndex, circlePower * 5) };
        },
    };
    private static Rune Floods => new()
    {
        Name  = "Floods",
        Power = 18,
        Rarity = Rarity.Rare,
        Text  = "On Activate: Conjure 4 Dams to your Shard pouch",
        OnActivate = (int selfIndex, Player player) =>
        {
            for (int i = 0; i < 4; i++)
            {
                Rune dam = Dam;
                dam.Token = true;
                player.AddNewRuneToBag(dam);
            }

            return new();
        },
    };
    private static Rune Flow => new()
    {
        Name  = "Flow",
        Power = 0,
        Rarity = Rarity.None,
        Text  = "On Play: Draw a Shard. Exile this Shard",
        OnEnter = (int selfIndex, Player player) =>
        {
            List<EventHistory> history = new();
            List<Rune> drawn = player.Draw(1);
            if (drawn.Count > 0)
            {
                history.Add(EventHistory.Draw(drawn.ToArray()));
            }

            history.Add(EventHistory.Exile(selfIndex));
            history.AddRange(player.Exile(selfIndex));

            return history;
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
            int runePower = player.GetRunePower(selfIndex);
            player.MultiplyPower(selfIndex, 3);
            return new() { EventHistory.PowerToRune(selfIndex, runePower * 2) };
        },
    };
    private static Rune Force => new()
    {
        Name  = "Force",
        Power = 8,
        Rarity = Rarity.Legendary,
        Text  = "On Activate: Power is multiplied by x, where x is the number of Shards in the summon",
        OnActivate = (int selfIndex, Player player) =>
        {
            int runes = player.RunesInCircle();
            int runePower = player.GetRunePower(selfIndex);
            player.MultiplyPower(selfIndex, runes);
            return new() { EventHistory.PowerToRune(selfIndex, runePower * (runes - 1)) };
        },
    };
    // F
    private static Rune Friend => new()
    {
        Name = "Friend",
        Power = 4,
        Rarity = Rarity.Common,
        Text = "Add +4 to the summon whenever a neighbouring Shard is activated",
        OnOtherRuneTrigger = (TriggerType type, int selfIndex, int other, Player player) =>
        {
            int prev = Player.CircularIndex(selfIndex - 1);
            int next = Player.CircularIndex(selfIndex + 1);
            if (type == TriggerType.OnActivate && (other == prev || other == next))
            {
                player.AddCirclePower(4);
                return new() { EventHistory.PowerToSummon(4) };
            }

            return new();
        },
    };
    // G
    private static Rune Generosity => new()
    {
        Name = "Generosity",
        Power = 0,
        Rarity = Rarity.Common,
        Text = "On Play: Permanently add +1 Power to opposite Shards",
        OnEnter = (int selfIndex, Player player) =>
        {
            List<EventHistory> history = new();

            Rune leftOp = player.GetRuneInCircle(selfIndex - 2);
            if (leftOp != null)
            {
                // Permanent
                leftOp.Power += 1;
                history.Add(EventHistory.PowerToRune(player.GetIndexOfRune(leftOp), 1));
            }
            Rune rightOp = player.GetRuneInCircle(selfIndex + 2);
            if (rightOp != null)
            {
                // Permanent
                rightOp.Power += 1;
                history.Add(EventHistory.PowerToRune(player.GetIndexOfRune(rightOp), 1));
            }

            return history;
        },
    };
    private static Rune Growth => new()
    {
        Name = "Growth",
        Power = 0,
        Rarity = Rarity.Legendary,
        Text = "On Activate: The summon power is multiplied by 2",
        OnActivate = (int selfIndex, Player player) =>
        {
            int circlePower = player.GetCirclePower();
            player.AddCirclePower(circlePower);
            return new() { EventHistory.PowerToSummon(player.GetCirclePower()) };
        },
    };
    private static Rune Guidance => new()
    {
        Name = "Guidance",
        Power = 11,
        Rarity = Rarity.Rare,
        Text = "On Play: Add +5 Power to all Common rarity Shards",
        OnEnter = (int selfIndex, Player player) =>
        {
            List<EventHistory> history = new();

            for (int i = 0; i < 5; i++)
            {
                Rune r = player.GetRuneInCircle(i);
                if (r != null && r.Rarity == Rarity.Common)
                {
                    player.AddStats(i, new() { Power = 5 });
                    history.Add(EventHistory.PowerToRune(i, 5));
                }
            }

            return history;
        },
    };
    // H
    private static Rune Hallowed => new()
    {
        Name = "Hallowed",
        Power = 15,
        Rarity = Rarity.Rare,
        Text = "On Destroy: Exile this Shard and its neighbours",
        OnDestroy = (int selfIndex, Player player) =>
        {
            List<EventHistory> history = new();

            int prev = Player.CircularIndex(selfIndex - 1);
            int next = Player.CircularIndex(selfIndex + 1);

            if (player.HasRuneAtIndex(prev))
            {
                history.Add(EventHistory.Exile(prev));
                history.AddRange(player.Exile(prev));
            }

            history.Add(EventHistory.Exile(selfIndex));
            history.AddRange(player.Exile(selfIndex));

            if (player.HasRuneAtIndex(next))
            {
                history.Add(EventHistory.Exile(next));
                history.AddRange(player.Exile(next));
            }

            return history;
        },
    };
    private static Rune Help => new()
    {
        Name = "Help",
        Power = 5,
        Rarity = Rarity.Common,
        Text = "On Play: Next Shards Power is multiplied by 2",
        OnEnter = (int selfIndex, Player player) =>
        {
            int index = Player.CircularIndex(selfIndex + 1);
            if (player.HasRuneAtIndex(index))
            {
                int runePower = player.GetRunePower(index);
                player.MultiplyPower(index, 2);
                return new() { EventHistory.PowerToRune(index, runePower * 2) };
            }

            return new();
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
                    return new() { EventHistory.PowerToSummon(player.GetCirclePower()) };
                }
            }

            return new();
        },
    };
    private static Rune Investment => new()
    {
        Name = "Investment",
        Power = 2,
        Rarity = Rarity.Common,
        Text = "On Activate: Add +15 to the next summon",
        OnActivate = (int selfIndex, Player player) =>
        {
            player.AddCirclePowerPromise(15);
            return new();
        },
    };
    private static Rune Iron => new()
    {
        Name  = "Iron",
        Power = 3,
        Rarity = Rarity.Starter,
        StartCount = 1,
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
    // K
    private static Rune Kill => new()
    {
        Name  = "Kill",
        Power = -20,
        Rarity = Rarity.Legendary,
        Text  = "On Activate: Exile next Shard",
        OnActivate = (int selfIndex, Player player) =>
        {
            List<EventHistory> history = new();
            int next = Player.CircularIndex(selfIndex + 1);

            if (player.HasRuneAtIndex(next))
            {
                history.Add(EventHistory.Exile(next));
                history.AddRange(player.Exile(next));
            }

            return history;
        },
    };
    // L
    private static Rune Light => new()
    {
        Name = "Light",
        Power = 20,
        Rarity = Rarity.Rare,
        Text = "Counts as Energy",
        Keywords = { Keywords.Energy },
    };
    // M
    private static Rune Martyr => new()
    {
        Name  = "Martyr",
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
                return new() { EventHistory.PowerToRune(player.GetIndexOfRune(prev), 10) };
            }

            return new();
        },
    };
    private static Rune Multi => new()
    {
        Name  = "Multi",
        Power = 5,
        Rarity = Rarity.Common,
        Text  = "On Play: Destroy all Energies; Draw a Shard for each Energy destroyed this way",
        OnEnter = (int selfIndex, Player player) =>
        {
            List<EventHistory> history = new();
            int destroyed = 0;

            for (int i = 0; i < 5; i++)
            {
                Rune rune = player.GetRuneInCircle(i);
                if (rune != null && rune.Keywords != null && rune.Keywords.Contains(Keywords.Energy))
                {
                    history.Add(EventHistory.Destroy(i));
                    history.AddRange(player.Remove(i));
                    destroyed++;
                }
            }

            List<Rune> drawn = player.Draw(destroyed);
            if (drawn.Count > 0)
            {
                history.Add(EventHistory.Draw(drawn.ToArray()));
            }

            return history;
        },
    };
    // N
    private static Rune Next => new()
    {
        Name = "Next",
        Power = 0,
        Rarity = Rarity.Rare,
        Text = "On Play: Permanently Transform this to a random Legendary",
        OnEnter = (int selfIndex, Player player) =>
        {
            List<EventHistory> history = new();
            List<Rune> allLedgendaryRunes = GetAllRunes((Rune r) => r.Rarity == Rarity.Legendary);
            Rune r = allLedgendaryRunes[UnityEngine.Random.Range(0, allLedgendaryRunes.Count)];
            Rune currentRune = player.GetRuneInCircle(selfIndex);
            currentRune.Name = r.Name;
            currentRune.Power = r.Power;
            currentRune.Rarity = r.Rarity;
            currentRune.Text = r.Text;

            history.Add(EventHistory.Replace(selfIndex, currentRune));

            return history;
        },
    };

    private static Rune Noble => new()
    {
        Name = "Noble",
        Power = 20,
        Rarity = Rarity.Common,
        Text  = "Neighbouring Shards has -5 Power",
        Aura = new()
        {
            new()
            {
                Power = -5,
                Application = (int selfIndex, int other, Player player) =>
                {
                    return Player.CircularIndex(selfIndex + 1) == other
                        || Player.CircularIndex(selfIndex - 1) == other;
                },
            },
        },
    };
    // O
    private static Rune Oppression => new()
    {
        Name  = "Oppression",
        Power = -10,
        Rarity = Rarity.Common,
        Text = "On Play: Neighbouring Shards Power is multiplied by 2",
        OnEnter = (int selfIndex, Player player) =>
        {
            List<EventHistory> history = new();

            int prev = Player.CircularIndex(selfIndex - 1);
            if (player.HasRuneAtIndex(prev))
            {
                int power = player.GetRunePower(prev);
                player.MultiplyPower(prev, 2);
                history.Add(EventHistory.PowerToRune(prev, power));
            }
            int next = Player.CircularIndex(selfIndex + 1);
            if (player.HasRuneAtIndex(next))
            {
                int power = player.GetRunePower(next);
                player.MultiplyPower(next, 2);
                history.Add(EventHistory.PowerToRune(next, power));
            }

            return history;
        },
    };
    // P
    private static Rune Pain => new()
    {
        Name = "Pain",
        Power = 0,
        Rarity = Rarity.None,
        Text = "On Play: Destroy the next Shard. Exile this Shard",
        OnEnter = (int selfIndex, Player player) =>
        {
            List<EventHistory> history = new();
            int next = Player.CircularIndex(selfIndex + 1);
            if (player.HasRuneAtIndex(next))
            {
                history.Add(EventHistory.Destroy(next));
                history.AddRange(player.Remove(next));
            }
            history.Add(EventHistory.Exile(selfIndex));
            history.AddRange(player.Exile(selfIndex));

            return history;
        },
    };
    private static Rune Pool => new()
    {
        Name  = "Pool",
        Power = 5,
        Rarity = Rarity.Starter,
        StartCount = 2,
        Text  = "On Play: Draw 2 Shards",
        OnEnter = (int selfIndex, Player player) =>
        {
            List<Rune> drawn = player.Draw(2);
            if (drawn.Count > 0)
            {
                return new() { EventHistory.Draw(drawn.ToArray()) };
            }

            return new();
        },
    };
    private static Rune Power => new()
    {
        Name = "Power",
        Power = 40,
        Rarity = Rarity.Legendary,
        Text = "Counts as Energy",
        Keywords = { Keywords.Energy },
    };
    private static Rune Prune => new()
    {
        Name  = "Prune",
        Power = 0,
        Rarity = Rarity.None,
        Text  = "On Activate: Exile previous Shard. Exile this Shard",
        OnActivate = (int selfIndex, Player player) =>
        {
            List<EventHistory> history = new();
            int prev = Player.CircularIndex(selfIndex - 1);

            if (player.HasRuneAtIndex(prev))
            {
                history.Add(EventHistory.Exile(prev));
                history.AddRange(player.Exile(prev));
            }
            if (player.HasRuneAtIndex(selfIndex))
            {
                history.Add(EventHistory.Exile(selfIndex));
                history.AddRange(player.Exile(selfIndex));
            }

            return history;
        },
    };
    private static Rune Prysm => new()
    {
        Name  = "Prysm",
        Power = 3,
        Rarity = Rarity.Starter,
        StartCount = 1,
        Text  = "On Activate: Power is multiplied by 2",
        OnActivate = (int selfIndex, Player player) =>
        {
            player.MultiplyPower(selfIndex, 2);
            return new() { EventHistory.PowerToRune(selfIndex, player.GetRunePower(selfIndex) / 2) };
        },
    };
    // R
    private static Rune Raid => new()
    {
        Name = "Raid",
        Power = 8,
        Rarity = Rarity.Common,
        Text = "On Activate: Activate 2 random Shards not named Raid",
        OnActivate = (int selfIndex, Player player) =>
        {
            List<EventHistory> history = new();
            Rune self = player.GetRuneInCircle(selfIndex);

            List<Rune> availableRunes = new();
            for (int i = 0; i < 5; i++)
            {
                Rune r = player.GetRuneInCircle(i);
                if (r != null && r.Name != self.Name)
                {
                    availableRunes.Add(r);
                }
            }

            for (int i = 0; i < 2; i++)
            {
                if (availableRunes.Count == 0)
                    break;

                int rand = UnityEngine.Random.Range(0, availableRunes.Count);
                Rune randRune = availableRunes[rand];
                availableRunes.RemoveAt(rand);
                int randIdx = player.GetIndexOfRune(randRune);

                history.AddRange(player.Activate(randIdx));
            }

            return history;
        },
    };
    private static Rune Ravage => new()
    {
        Name = "Ravage",
        Power = 4,
        Rarity = Rarity.Common,
        Text = "On Play: Activate the Shard two steps prior to this one",
        OnEnter = (int selfIndex, Player player) =>
        {
            return player.Activate(selfIndex - 2);
        },
    };
    private static Rune Reap => new()
    {
        Name = "Reap",
        Power = 4,
        Rarity = Rarity.Common,
        Text = "On Play: The summon power is multiplied by 3",
        OnEnter = (int selfIndex, Player player) =>
        {
            int circlePower = player.GetCirclePower();
            player.AddCirclePower(circlePower * 2);
            return new() { EventHistory.PowerToSummon(player.GetCirclePower()) };
        },
    };
    private static Rune Rebellious => new()
    {
        Name = "Rebellious",
        Power = 5,
        Rarity = Rarity.Common,
        Text = "Neighbours has +6 and opposite shards has -4",
        Aura = new()
        {
            new()
            {
                Power = 6,
                Application = (int selfIndex, int other, Player player) =>
                {
                    return player.AreNeighbours(selfIndex, other);
                },
            },
            new()
            {
                Power = -4,
                Application = (int selfIndex, int other, Player player) =>
                {
                    return player.AreOpposites(selfIndex, other);
                },
            }
        },
    };
    private static Rune Reave => new()
    {
        Name = "Reave",
        Power = 5,
        Rarity = Rarity.Common,
        Text = "Permanently add +2 to this Shard when other Shards are destroyed",
        OnOtherRuneTrigger = (TriggerType trigger, int selfIndex, int other, Player player) =>
        {
            if (trigger == TriggerType.OnDestroy)
            {
                Rune rune = player.GetRuneInCircle(selfIndex);
                // Permanent
                rune.Power += 2;
                return new() { EventHistory.PowerToRune(selfIndex, 2) };
            }
            return new();
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
            return new() { EventHistory.Draw(pool) };
        },
    };
    private static Rune Restore => new()
    {
        Name = "Restore",
        Power = 0,
        Rarity = Rarity.None,
        Text = "On Play: Resummon up to 2 fingers. Exile this Shard",
        OnEnter = (int selfIndex, Player player) =>
        {
            List<EventHistory> history = new();
            int life = player.Health;
            player.AddLife(2);
            int delta = player.Health - life;
            history.Add(EventHistory.AddLife(delta));
            history.Add(EventHistory.Exile(selfIndex));
            history.AddRange(player.Exile(selfIndex));
            return history;
        },
    };
    private static Rune Retry => new()
    {
        Name = "Retry",
        Power = 10,
        Rarity = Rarity.Rare,
        Text = "On Play: Activate the Previous Shard",
        OnEnter = (int selfIndex, Player player) =>
        {
            int previusIndex = Player.CircularIndex(selfIndex - 1);
            Rune previus = player.GetRuneInCircle(previusIndex);
            if(previus == null)
                return null;

            if(previus.Name == "Retry")
                return null;

            return player.Activate(previusIndex);
        },
    };
    private static Rune Run => new()
    {
        Name = "Run",
        Power = 0,
        Rarity = Rarity.Common,
        Text = "On Play: Flip a coin; if successful, permanently conjure a Rare or Legendary Shard. Exile this Shard",
        OnEnter = (int selfIndex, Player player) =>
        {
            List<EventHistory> history = new();

            bool success = UnityEngine.Random.value >= 0.5f;
            history.Add(EventHistory.DiceRoll(success));

            if (success)
            {
                List<Rune> runes = GetAllRunes((rune) => rune.Rarity >= Rarity.Rare);
                int idx = UnityEngine.Random.Range(0, runes.Count);
                Rune rune = runes[idx];
                player.Buy(rune);
                player.AddNewRuneToHand(rune);
                history.Add(EventHistory.Draw(rune));
            }

            history.Add(EventHistory.Exile(selfIndex));
            history.AddRange(player.Exile(selfIndex));

            return history;
        },
    };
    // S
    private static Rune Sacrifice => new()
    {
        Name = "Sacrifice",
        Power = -10,
        Rarity = Rarity.Common,
        Text = "On Activate: Next Shards Power is multiplied by 3",
        OnActivate = (int selfIndex, Player player) =>
        {
            int index = Player.CircularIndex(selfIndex + 1);
            if (player.HasRuneAtIndex(index))
            {
                int power = player.GetRunePower(index);
                player.MultiplyPower(index, 3);
                return new() { EventHistory.PowerToRune(index, power * 2) };
            }

            return new();
        },
    };
    private static Rune Seeing => new()
    {
        Name = "Seeing",
        Power = 13,
        Rarity = Rarity.Rare,
        Text = "On Play: Draw 4 Shards then Discard a random one",
        OnEnter = (int selfIndex, Player player) =>
        {
            List<EventHistory> history = new List<EventHistory>();
            history.Add(EventHistory.Draw(player.Draw(4).ToArray()));
            history.Add(EventHistory.Discard(player.DiscardAtIndex()));
            return history;
        },
    };
    private static Rune Serate => new()
    {
        Name  = "Serate",
        Power = 10,
        Rarity = Rarity.Rare,
        Text  = "All Energy Shards has +10 Power",
        Aura = new()
        {
            new()
            {
                Power = 10,
                Application = (int selfIndex, int other, Player player) =>
                {
                    Rune rune = player.GetRuneInCircle(other);
                    return rune != null && rune.Keywords != null && rune.Keywords.Contains(Keywords.Energy);
                },
            },
        },
    };
    private static Rune Shadow => new()
    {
        Name = "Shadow",
        Power = 60,
        Rarity = Rarity.Legendary,
        Text  = "Neighbouring Shards has -15 Power",
        Aura = new()
        {
            new()
            {
                Power = -20,
                Application = (int selfIndex, int other, Player player) =>
                {
                    return Player.CircularIndex(selfIndex + 1) == other
                        || Player.CircularIndex(selfIndex - 1) == other;
                },
            },
        },
    };
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
                return new() { EventHistory.PowerToRune(player.GetIndexOfRune(prev), 2) };
            }

            return new();
        },
    };
    private static Rune Slow => new()
    {
        Name  = "Slow",
        Power = 5,
        Rarity = Rarity.Common,
        Text  = "On Play: Conjure 2 Flow to hand",
        OnEnter = (int selfIndex, Player player) =>
        {
            List<Rune> drawn = new();
            for (int i = 0; i < 2; i++)
            {
                Rune flow = Flow;
                flow.Token = true;
                player.AddNewRuneToHand(flow);
                drawn.Add(flow);
            }

            return new() { EventHistory.Draw(drawn.ToArray()) };
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
            return new() { EventHistory.PowerToSummon(player.GetCirclePower()) };
        },
    };
    private static Rune Strongheart => new()
    {
        Name  = "Strongheart",
        Power = 10,
        Rarity = Rarity.Rare,
        Text  = "On Activate: Conjure 2 Vigor to your Shard pouch",
        OnActivate = (int selfIndex, Player player) =>
        {
            for (int i = 0; i < 2; i++)
            {
                Rune vigor = Vigor;
                vigor.Token = true;
                player.AddNewRuneToBag(vigor);
            }

            return new();
        },
    };
    private static Rune Supporter => new()
    {
        Name = "Supporter",
        Power = 6,
        Rarity = Rarity.Common,
        Text = "On Activate: Add +10 to the summon if this Shard has the lowest Power",
        OnActivate = (int selfIndex, Player player) =>
        {
            bool lowest = true;
            int power = player.GetRunePower(selfIndex);
            for (int i = 0; i < 5; i++)
            {
                if (i == selfIndex)
                    continue;

                if (player.HasRuneAtIndex(i))
                {
                    int p = player.GetRunePower(i);
                    if (p < power)
                    {
                        lowest = false;
                        break;
                    }
                }
            }

            if (lowest)
            {
                player.AddCirclePower(10);
                return new() { EventHistory.PowerToSummon(player.GetCirclePower()) };
            }

            return new();
        },
    };
    // T
    private static Rune Tempest => new()
    {
        Name  = "Tempest",
        Power = 0,
        Rarity = Rarity.Rare,
        Text  = "On Play: Destroy all Shards in the summon",
        OnEnter = (int selfIndex, Player player) =>
        {
            List<EventHistory> history = new();

            for (int i = 0; i < 5; i++)
            {
                if (player.HasRuneAtIndex(i))
                {
                    history.Add(EventHistory.Destroy(i));
                    history.AddRange(player.Remove(i));
                }
            }

            return history;
        },
    };
    private static Rune Tides => new()
    {
        Name  = "Tides",
        Power = 7,
        Rarity = Rarity.Common,
        Text  = "On Play: Swap the next Shard with the previous Shard",
        OnEnter = (int selfIndex, Player player) =>
        {
            int idx1 = Player.CircularIndex(selfIndex - 1);
            int idx2 = Player.CircularIndex(selfIndex + 1);
            player.Swap(idx1, idx2);
            return new() { EventHistory.Swap(idx1, idx2) };
        },
    };
    private static Rune Trickle => new()
    {
        Name  = "Trickle",
        Power = 3,
        Rarity = Rarity.Common,
        Text  = "On Play: Add +5 Power to the next Shard and +2 Power to the Shard after that",
        OnEnter = (int selfIndex, Player player) =>
        {
            List<EventHistory> history = new();

            int idx1 = Player.CircularIndex(selfIndex + 1);
            int idx2 = Player.CircularIndex(selfIndex + 2);

            if (player.HasRuneAtIndex(idx1))
            {
                player.AddStats(idx1, new() { Power = 5 });
                history.Add(EventHistory.PowerToRune(idx1, 5));
            }
            if (player.HasRuneAtIndex(idx2))
            {
                player.AddStats(idx2, new() { Power = 2 });
                history.Add(EventHistory.PowerToRune(idx1, 2));
            }

            return history;
        },
    };
    // U
    private static Rune Unsable => new()
    {
        Name = "Unsable",
        Power = 24,
        Rarity = Rarity.Rare,
        Text = "On Play: Transform neighbouring Shards to random Shards",
        OnEnter = (int selfIndex, Player player) =>
        {
            List<EventHistory> history = new();

            {
                int prev = Player.CircularIndex(selfIndex - 1);
                Rune prevRune = player.GetRuneInCircle(prev);
                if (prevRune != null)
                {
                    List<Rune> allRunes = GetAllRunes();
                    Rune rune = null;
                    while (rune == null)
                    {
                        Rune r = allRunes[UnityEngine.Random.Range(0, allRunes.Count)];
                        if (r.Name != prevRune.Name)
                            rune = r;
                    }
                    rune.Token = true;

                    player.Replace(rune, prev);
                    history.Add(EventHistory.Replace(prev, rune));
                }
            }
            {
                int next = Player.CircularIndex(selfIndex + 1);
                Rune nextRune = player.GetRuneInCircle(next);
                if (nextRune != null)
                {
                    List<Rune> allRunes = GetAllRunes();
                    Rune rune = null;
                    while (rune == null)
                    {
                        Rune r = allRunes[UnityEngine.Random.Range(0, allRunes.Count)];
                        if (r.Name != nextRune.Name)
                            rune = r;
                    }
                    rune.Token = true;

                    player.Replace(rune, next);
                    history.Add(EventHistory.Replace(next, rune));
                }
            }

            return history;
        },
    };
    private static Rune Upgrade => new()
    {
        Name = "Upgrade",
        Power = 32,
        Rarity = Rarity.Legendary,
        Text = "On Activate: Transform the Shard with the lowest Power to a random Rare or Ledgendary Shard",
        OnActivate = (int selfIndex, Player player) =>
        {
            List<EventHistory> history = new();

            Rune weakestRune = Enumerable.Range(0, 5)
                .Select(e => player.GetRuneInCircle(e))
                .Where(e => e != null)
                .OrderBy((e) => e.Power)
                .ToArray()[0];
            
            int weakestRuneIndex = player.GetIndexOfRune(weakestRune);
            Rune rune = null;
            List<Rune> allRunes = GetAllRunes((e) => e.Rarity >= Rarity.Rare);
            while (rune == null)
            {
                Rune r = allRunes[UnityEngine.Random.Range(0, allRunes.Count)];
                if (r.Name != weakestRune.Name)
                    rune = r;
            }
            rune.Token = true;
            player.Replace(rune, weakestRuneIndex);
            history.Add(EventHistory.Replace(weakestRuneIndex, rune));


            return history;
        },
    };

    // V
    private static Rune Vigor => new()
    {
        Name = "Vigor",
        Power = 15,
        Rarity = Rarity.None,
        Text = "Counts as Energy",
        Keywords = { Keywords.Energy },
    };
    // W

    private static Rune Well => new()
    {
        Name = "Well",
        Power = 23,
        Rarity = Rarity.Legendary,
        Text = "On Play: Discard your hand then Conjure 5 random Shards",
        OnEnter = (int selfIndex, Player player) =>
        {
            List<EventHistory> history = new List<EventHistory> { EventHistory.Discard(player.GetRunesInHand()) };
            player.DiscardHand();
            List<Rune> allRunes = GetAllRunes();
            Rune[] newRunes = Enumerable.Range(0, 5).Select((e) =>
            {
                Rune r = allRunes[UnityEngine.Random.Range(0, allRunes.Count)];
                r.Token = true;
                player.AddNewRuneToHand(r);
                return r;
            }).ToArray();
            history.Add(EventHistory.Draw(newRunes));
            return history;
        }
    };

    private static Rune Wound => new()
    {
        Name = "Wound",
        Power = 0,
        Rarity = Rarity.Rare,
        Text = "On Destroy: Permanently add +4 Power to opposite Shards",
        OnDestroy = (int selfIndex, Player player) =>
        {
            List<EventHistory> history = new();

            Rune leftOp = player.GetRuneInCircle(selfIndex - 2);
            if (leftOp != null)
            {
                // Permanent
                leftOp.Power += 4;
                history.Add(EventHistory.PowerToRune(player.GetIndexOfRune(leftOp), 4));
            }
            Rune rightOp = player.GetRuneInCircle(selfIndex + 2);
            if (rightOp != null)
            {
                // Permanent
                rightOp.Power += 4;
                history.Add(EventHistory.PowerToRune(player.GetIndexOfRune(rightOp), 4));
            }

            return history;
        },
    };

  

   

    
}
