using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Settings
{
    public static int HandSize = 5;
    public static int NumSlots = 5;
    public static int PlayerMaxHealth = 5;
    public static int ShopActions = 3;
    public static int Rounds = 15;
    public static int RandomRunesAtStart = 0;
    public static int MinStartHandSize = 10;

    private static int opponentHealthBase = 48;
    private static int opponentHealthBaseRamp = 20;
    private static int opponentHealthRoundRamp = 4;

    private static int opponentDamageBase = 0;
    private static int opponentDamageRamp = 1;

    public static int GetOpponentHealth(int currentRound)
    {
        int multi = 0;
        for (int i = 1; i <= currentRound; i++)
            multi += i;

        // base + (const * round) + (ramp * multi)
        return opponentHealthBase + (opponentHealthBaseRamp * currentRound) + (opponentHealthRoundRamp * multi);
    }

    public static int GetOpponentDamage(int num) => 1;
}
