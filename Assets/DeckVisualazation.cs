using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeckVisualazation : MonoBehaviour
{
    public GameObject[] Shards;
    private int lastNumber = -1;

    public void SetDeckSize(int amount)
    {
        if (amount == lastNumber)
            return;

        lastNumber = amount;
        for (int i = 0; i < Shards.Length; i++)
            Shards[i].SetActive(false);

        int max = amount > Shards.Length ? Shards.Length : amount;

        for (int i = 0; i < max; i++)
            Shards[i].SetActive(true);
    }
}
