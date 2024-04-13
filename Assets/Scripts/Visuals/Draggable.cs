using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Draggable : MonoBehaviour
{
    public MeshCollider Collider;
    public BoxCollider HoverCollider;
    public Rigidbody Rigidbody;
}
