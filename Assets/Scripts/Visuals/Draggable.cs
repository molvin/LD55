using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Draggable : MonoBehaviour
{
    public MeshCollider Collider;
    public BoxCollider HoverCollider;
    public Rigidbody Rigidbody;
    public Vector3 SlotOffset;
    public Vector3 InspectOffset;

    public void ResetRot()
    {
        StartCoroutine(Do());
        IEnumerator Do()
        {
            Quaternion startRot = transform.rotation;
            float t = 0;
            float duration = 0.15f;
            while (t < duration)
            {
                transform.rotation = Quaternion.Slerp(startRot, Quaternion.identity, t / duration);
                t += Time.deltaTime;
                yield return null;
            }
            transform.rotation = Quaternion.identity;
        }
    }
}
