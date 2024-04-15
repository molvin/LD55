using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallingOutOfBoundsReset : MonoBehaviour
{
    public Transform ShardTransform;
    private void Update()
    {
        /*
        if(ShardTransform.position.x < -3.7f || ShardTransform.position.x > 3.7f)
        {
            ShardTransform.position = new Vector3(0, ShardTransform.position.y, ShardTransform.position.z);
        }
        if (ShardTransform.position.y < -2f || ShardTransform.position.y > 10f)
        {
            ShardTransform.position = new Vector3(ShardTransform.position.x, 1f, ShardTransform.position.z);
        }
        if (ShardTransform.position.z < -2f || ShardTransform.position.z > 2f)
        {
            ShardTransform.position = new Vector3(ShardTransform.position.x, ShardTransform.position.y, 0);
        }
        */
    }
}
