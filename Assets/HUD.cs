using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUD : MonoBehaviour
{
    public static HUD Instance;
    public Button EndTurnButton;

    private void Awake()
    {
        Instance = this;
    }
}
