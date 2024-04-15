using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrollAnimationController : MonoBehaviour
{
    public Animation Anim;
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
    public void Play(string name)
    {

        int mat;
        if(name == "OpenScrollStar")
        {
            mat = 0;
        }
        else if(name == "OpenScrollShop")
        {
            mat = 1;
        }
        else if (name == "OpenScrollPath")
        {
            mat = 2;
        }
        else
        {
            mat = -1;
        }

        if(mat != -1)
            PaperRenderer.material = PaperMaterials[mat];
        Anim.Play(name);
    }

    public bool isPlaying => Anim.isPlaying;
}
