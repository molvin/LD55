using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Health : MonoBehaviour
{
    public TextMeshProUGUI Text;
    public Image Radial;

    public void Set(int health, int maxHealth)
    {
        Text.text = $"{health}";
        Radial.fillAmount = health / (float) maxHealth;
    }
}
