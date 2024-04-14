using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class VolumeSettings : MonoBehaviour
{

    public SliderConfiguration[] sliderConfigurations;
    public AudioMixer AudioMixer;


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
            sc.slider.value = Mathf.Pow(10, v);
            sc.slider.onValueChanged.AddListener(delegate { setMixerValue(sc); });

        }
    }

    public void setMixerValue(SliderConfiguration sc)
    {
        Debug.Log("Setting " + sc.mixer_value + " to " + sc.slider.value);
        AudioMixer.SetFloat(sc.mixer_value, Mathf.Log10(sc.slider.value) * 20);

    }


}
