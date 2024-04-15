using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandVisualizer : MonoBehaviour
{
    [SerializeField]
    private Animator m_Animator;

    [SerializeField]
    private Animator m_CameraAnimator;

    private int cachedHealth = 5;

    public IEnumerator ViewSelf(int health, bool heal)
    {
        m_CameraAnimator.SetTrigger("ViewSelf");

        yield return new WaitForSeconds(1.0f);

        int delta = health - cachedHealth;
        int dir = (int) Mathf.Sign(delta);
        delta = Mathf.Abs(delta);

        for(int i = 0; i < delta && cachedHealth != 5; i++)
        {
            cachedHealth += dir;
            m_Animator.SetInteger("health", cachedHealth);
            m_Animator.SetTrigger(heal ? "heal" : "damage");

            /*
            while(m_Animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1 || m_Animator.IsInTransition(0))
            {
                yield return null;
            }
            */
            yield return new WaitForSeconds(3.0f);
        }
;

        m_CameraAnimator.SetTrigger("BackFromSelf");
        yield return new WaitForSeconds(1.0f);
        m_CameraAnimator.SetTrigger("Idle");

        cachedHealth = health;

    }
}
