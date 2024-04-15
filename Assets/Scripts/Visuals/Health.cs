using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Health : MonoBehaviour
{
    public TextMeshProUGUI Text;

    public void Set(int health, int maxHealth)
    {
        Text.text = health < 0 ? "0" : $"{health}";
    }
}
