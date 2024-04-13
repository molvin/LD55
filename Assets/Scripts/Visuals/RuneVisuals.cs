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

    public void Init(Rune rune)
    {
        this.rune = rune;
        // TODO: init
    }

}
