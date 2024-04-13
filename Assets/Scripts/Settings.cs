using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Settings
{
    public static int HandSize = 5;
    public static int NumSlots = 5;
    public static int PlayerMaxHealth = 5;

    private static int opponentBaseHealth = 25;
    private static int opponentHealthRamp = 25;

    private static int opponentDamageBase = 0;
    private static int opponentDamageRamp = 1;

    public static int GetOpponentHealth(int currentRound)
    {
        return opponentBaseHealth + opponentHealthRamp * currentRound;
    }

    public static int GetOpponentDamage(int num)
    {
        return opponentDamageBase + opponentDamageRamp * num;
    }
}
