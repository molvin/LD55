using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrollAnimationController : MonoBehaviour
{

    public SkinnedMeshRenderer PaperRenderer;
    public Material[] PaperMaterials;

    [SerializeField]
    AudioOneShotClipConfiguration m_RollOutSound;
    [SerializeField]
    AudioOneShotClipConfiguration m_CloseShopSound;
    private Audioman m_AudioMan;

    private void Start()
    {
        m_AudioMan = FindObjectOfType<Audioman>();
    }

    public void PlayRollOutSound(int open)
    {
        if(open > 0)
        {
            m_AudioMan.PlaySound(m_RollOutSound, transform.position);
        }
        else
        {
            m_AudioMan.PlaySound(m_CloseShopSound, transform.position);
        }
    }

    public void SetMaterial(int mat)
    {
        PaperRenderer.material = PaperMaterials[mat];
    }

}
