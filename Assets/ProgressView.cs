using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProgressView : MonoBehaviour
{
    public ProgressPoint[] Points;

    private void Start()
    {
        foreach (ProgressPoint p in Points)
        {
            p.gameObject.SetActive(false);
            p.Toggle.enabled = false;
        }
    }

    public IEnumerator Set(int progress)
    {
        foreach (ProgressPoint p in Points)
        {
            p.gameObject.SetActive(true);
            p.Toggle.enabled = false;
        }

        yield return new WaitForSeconds(1);

        for (int i = 0; i < progress; i++)
        {
            Points[i].Toggle.enabled = true;
            yield return new WaitForSeconds(0.5f);
        }
        yield return new WaitForSeconds(1);

        foreach (ProgressPoint p in Points)
        {
            p.gameObject.SetActive(false);
            p.Toggle.enabled = false;
        }
    }
}
