using System.Collections;
using System.Collections.Generic;
using System.Data;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class FakeRune : MonoBehaviour
{
    public TextMeshProUGUI Power;
    public TextMeshProUGUI Description;
    public Image Icon;
    public void Init(Rune rune)
    {
        Description.text = rune.Text;
        Icon.sprite = Sprite.Create(RuneIcons.Get(rune.Name), Icon.sprite.rect, Icon.sprite.pivot);
        Power.text = $"{rune.Power}";
    }
}
