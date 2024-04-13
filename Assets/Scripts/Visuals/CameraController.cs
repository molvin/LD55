using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public static CameraController Instance;
    public Transform InspectPoint;

    private void Awake()
    {
        Instance = this;
    }
}
