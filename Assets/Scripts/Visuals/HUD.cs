using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUD : MonoBehaviour
{
    public static HUD Instance;
    public Button EndTurnButton;
    public Health PlayerHealth;
    //public Health OpponentHealth;

    private void Awake()
    {
        Instance = this;
    }
}
