using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandVisualizer : MonoBehaviour
{
    [SerializeField]
    private Animator m_Animator;

    [SerializeField]
    private Animator m_CameraAnimator;

    [SerializeField]
    private AudioOneShotClipConfiguration m_CutSound;

    private Audioman m_AudioMan;

    void Start()
    {
        m_AudioMan = FindObjectOfType<Audioman>();
    }

    public void PlayCutSound()
    {
        m_AudioMan.PlaySound(m_CutSound, transform.position);
    }

    public IEnumerator ViewSelf(int health, bool heal)
    {
        m_CameraAnimator.SetTrigger("ViewSelf");

        yield return new WaitForSeconds(1.0f);

        m_Animator.SetInteger("health", health);
        m_Animator.SetTrigger(heal ? "heal" : "damage");

        /*
        while(m_Animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1 || m_Animator.IsInTransition(0))
        {
            yield return null;
        }
        */
        yield return new WaitForSeconds(3.0f);

        m_CameraAnimator.SetTrigger("BackFromSelf");
        yield return new WaitForSeconds(1.0f);
        m_CameraAnimator.SetTrigger("Idle");

    }
}
