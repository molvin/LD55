using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuCameraRotator : MonoBehaviour
{

    public float roateSpeed = 1;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 angles = transform.eulerAngles;
        angles.y = angles.y - roateSpeed * Time.deltaTime;
        transform.eulerAngles = angles;
    }
}
