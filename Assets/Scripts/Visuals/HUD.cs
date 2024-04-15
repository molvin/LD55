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
    public Image FadePanel;

    private void Awake()
    {
        Instance = this;
    }

    public IEnumerator FadeToBlack(float duration)
    {
        float t = 0.0f;
        while (t < duration)
        {
            Color col = FadePanel.color;
            col.a = (t / duration);
            FadePanel.color = col;

            t += Time.deltaTime;
            yield return null;
        }
        FadePanel.color = Color.black;
    }
}
