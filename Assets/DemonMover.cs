using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DemonMover : MonoBehaviour
{
    public Transform Start;
    public Transform End;

    public void SetDemonPosition(float amount)
    {
        gameObject.transform.position = Vector3.Lerp(Start.position, End.position, amount);
    }
}
