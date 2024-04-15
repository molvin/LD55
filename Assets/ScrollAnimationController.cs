using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrollAnimationController : MonoBehaviour
{

    public SkinnedMeshRenderer PaperRenderer;
    public Material[] PaperMaterials;


    public void SetMaterial(int mat)
    {
        PaperRenderer.material = PaperMaterials[mat];
    }

}
