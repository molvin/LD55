using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class RuneVisuals : Draggable
{
    public TextMeshProUGUI Power;
    public TextMeshProUGUI Description;
    public Image Icon;
    public ParticleSystem HoverParticles;

    public Material[] Edges;
    public Material[] Surfaces;
    public MeshRenderer EdgeRenderer;
    public MeshRenderer SurfaceRenderer;

    private Rune rune;
    private Player player;
    public Transform visual;

    public bool Hover;
    private Vector3 hoverOrigin;
    public Rune Rune => rune;

    public bool InSlot;

    public FakeRune fakeRune;

    [System.Serializable]
    public class RuneRefByName
    {
        public string Name;
        public RuneRef Ref;
    }
    public RuneRefByName[] DescribeRunes;

    private void Start()
    {
        hoverOrigin = transform.GetChild(0).localPosition;
        fakeRune.gameObject.SetActive(false);
    }

    public void Init(Rune rune, Player player)
    {
        this.rune = rune;
        this.player = player;
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
        Icon.sprite = Sprite.Create(RuneIcons.Get(rune.Name), Icon.sprite.rect, Icon.sprite.pivot);
        UpdateStats();
    }

    public void ShowDescription()
    {
        foreach(RuneRefByName runeRef in DescribeRunes)
        {
            if (rune.Text.Contains(runeRef.Name))
            {
                fakeRune.gameObject.SetActive(true);
                fakeRune.Init(runeRef.Ref.Get());
            }
        }
        
    }
    public void HideDescriptions()
    {
        fakeRune.gameObject.SetActive(false);
    }

    public void UpdateStats()
    {
        int index = player.GetIndexOfRune(rune);
        int power = index >= 0 ? player.GetRunePower(index) : rune.Power;
        Power.text = $"{power}";
    }

    private void Update()
    {
        //float height = 0.05f;

        if (Hover)
        {
            //.rotation = Quaternion.identity;
            //transform.GetChild(0).localPosition = hoverOrigin + Vector3.up * height;
        }
        else
        {
            //transform.GetChild(0).localPosition = hoverOrigin;
        }

    }
}
