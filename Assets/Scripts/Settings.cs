using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Settings
{
    public static int HandSize = 5;
    public static int NumSlots = 5;
    public static int PlayerMaxHealth = 5;
    public static int ShopActions = 2;
    public static int Rounds = 15;

    private static int opponentBaseHealth = 60;
    private static int opponentHealthRamp = 30;

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
