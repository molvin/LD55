using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProgressView : MonoBehaviour
{
    public GameObject YouAreHere;
    public GameObject[] Points;
    public AudioOneShotClipConfiguration WriteSound;
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
            FindObjectOfType<Audioman>().PlaySound(WriteSound, Points[i].transform.position);
            yield return new WaitForSeconds(0.2f);
        }
        YouAreHere.gameObject.SetActive(true);
        YouAreHere.transform.position = Points[progress].transform.position + Vector3.forward * 0.1f;
        while (!Input.GetMouseButtonDown(0) && !Input.GetMouseButtonDown(1))
            yield return null;

        foreach (GameObject p in Points)
        {
            p.gameObject.SetActive(false);
        }
        YouAreHere.gameObject.SetActive(false);
    }
}
