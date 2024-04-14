using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class RuneVisuals : Draggable
{
    public TextMeshProUGUI Name;
    public TextMeshProUGUI Power;
    public TextMeshProUGUI Description;
    public Image Icon;

    private Rune rune;
    private Player player;
    public Rune Rune => rune;

    public bool InSlot;

    public void Init(Rune rune, Player player)
    {
        this.rune = rune;
        this.player = player;

        Name.text = rune.Name;
        Description.text = rune.Text;

        UpdateStats();
    }

    public void UpdateStats()
    {
        int index = player.GetIndexOfRune(rune);
        int power = index >= 0 ? player.GetRunePower(index) : rune.Power;
        Power.text = $"{power}";
    }
}
