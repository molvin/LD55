using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;
using UnityEngine.Audio;

public class MainMenuAnimation : MonoBehaviour
{
    // Start is called before the first frame update
    public Rotator[] rotators;
    public TransitionAnimationConfig transitionConfig;

    [System.Serializable]
    public struct TransitionAnimationConfig
    {
        public Image image;
        public float duration;
        public AnimationCurve curve;
        public float min_scale_factor;
        public float original_scale;
        public GameObject button_group_settings;
        public GameObject button_group_main;
    }
    ;

    [System.Serializable]
    public struct Rotator
    {
        public Image image;
        public float roateSpeed;
        public bool direction;
    }
    ;

    void Start()
    {
        //spawnButtons();
    }

    // Update is called once per frame
    void Update()
    {
        foreach (Rotator r in  rotators)
        {
            Vector3 angles = r.image.rectTransform.eulerAngles;
            angles.z = angles.z - r.roateSpeed * Time.deltaTime * (r.direction ? 1 : -1); 
            r.image.rectTransform.eulerAngles = angles;
        }
    }


    public void triggerSettingsAnim(bool show_settings)
    {
        StartCoroutine(runTransitionToSettingsAnimation(show_settings));
    }

    public IEnumerator runTransitionToSettingsAnimation(bool showSettings)
    {
        float time = 0;
        while (time < transitionConfig.duration)
        {
            time += Time.deltaTime;
            float scale = transitionConfig.original_scale * transitionConfig.curve.Evaluate(time / transitionConfig.duration);
            transitionConfig.image.rectTransform.localScale = new Vector3(
                scale, scale, scale
            );


            if (time > (transitionConfig.duration / 2))
            {
                transitionConfig.button_group_settings.SetActive(showSettings);
                transitionConfig.button_group_main.SetActive(!showSettings);
            }
            yield return null;
        }
    }

    public AudioMixer mixer;
    public void SetMasterVolume(Slider slider)
    {
        mixer.SetFloat("master", Mathf.Log10(slider.value) * 20);
    }

}
