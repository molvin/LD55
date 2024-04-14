using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class VolumeSettings : MonoBehaviour
{

    public SliderConfiguration[] sliderConfigurations;
    public AudioMixer AudioMixer;

    public AudioMixer mixer;

    [System.Serializable]
    public struct SliderConfiguration
    {
        public Slider slider;
        public string mixer_value;
    }
    void Start()
    {
        foreach(SliderConfiguration sc in sliderConfigurations)
        {
            float v;
            AudioMixer.GetFloat(sc.mixer_value, out v);
            Debug.Log(v);
            sc.slider.value = Mathf.Pow(10, v) * 20;
            sc.slider.onValueChanged.AddListener(delegate { setMixerValue(sc); });

        }
    }

    public void setMixerValue(SliderConfiguration sc)
    {
        AudioMixer.SetFloat(sc.mixer_value, sc.slider.value);

    }


    public void SetMasterVolume(Slider slider)
    {
        mixer.SetFloat("master", Mathf.Log10(slider.value) * 20);
    }

}
