using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class RuneVisuals : MonoBehaviour
{
    public TextMeshProUGUI Name;
    public TextMeshProUGUI Power;
    public Image Icon;
    public MeshCollider Collider;
    public BoxCollider HoverCollider;
    public Rigidbody Rigidbody;

    private Rune rune;
    private Player player;
    public Rune Rune => rune;

    public void Init(Rune rune, Player player)
    {
        this.rune = rune;
        this.player = player;
        // TODO: init
    }

    private void Update()
    {
        if (rune != null)
        {
            Name.text = rune.Name;
            int index = player.GetIndexOfRune(rune);
            int power = index >= 0 ? player.GetRunePower(index) : rune.Power;
            Power.text = $"{power}";
        }
    }
}
