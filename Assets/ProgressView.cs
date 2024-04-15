using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProgressView : MonoBehaviour
{
    public GameObject YouAreHere;
    public GameObject[] Points;

    private void Start()
    {
        foreach (GameObject p in Points)
        {
            p.gameObject.SetActive(false);
        }
        YouAreHere.gameObject.SetActive(false);
    }

    public IEnumerator Set(int progress)
    {
        yield return new WaitForSeconds(.5f);

        for (int i = 0; i < progress; i++)
        {
            Points[i].SetActive(true);
            yield return new WaitForSeconds(0.2f);
        }
        YouAreHere.gameObject.SetActive(true);
        YouAreHere.transform.position = Points[progress].transform.position + Vector3.forward * 0.1f;
        yield return new WaitForSeconds(2);

        foreach (GameObject p in Points)
        {
            p.gameObject.SetActive(false);
        }
        YouAreHere.gameObject.SetActive(false);
    }
}
