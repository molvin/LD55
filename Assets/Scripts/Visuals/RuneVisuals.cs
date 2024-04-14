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

    public Material[] Edges;
    public Material[] Surfaces;
    public MeshRenderer EdgeRenderer;
    public MeshRenderer SurfaceRenderer;

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

        //Set material based on rarity
        switch (rune.Rarity)
        {
            case Rarity.Starter:
                EdgeRenderer.material = Edges[0];
                SurfaceRenderer.material = Surfaces[0];
                break;
            case Rarity.Common:
            case Rarity.None:
                EdgeRenderer.material = Edges[1];
                SurfaceRenderer.material = Surfaces[1];
                break;
            case Rarity.Rare:
                EdgeRenderer.material = Edges[2];
                SurfaceRenderer.material = Surfaces[2];
                break;
            case Rarity.Legendary:
                EdgeRenderer.material = Edges[3];
                SurfaceRenderer.material = Surfaces[3];
                break;
        }
        //TODO: SET THE ICON
        UpdateStats();
    }

    public void UpdateStats()
    {
        int index = player.GetIndexOfRune(rune);
        int power = index >= 0 ? player.GetRunePower(index) : rune.Power;
        Power.text = $"{power}";
    }
}
