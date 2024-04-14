using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;

public class MainMenuAudioManager : MonoBehaviour
{
    public AudioSource audioSource;

    public AudioClip[] menuHover;
    void Start()
    {
                
    }


    public void playMenuHoverSounde()
    {
        playOneShot(menuHover);
    }

    public void playOneShot(AudioClip[] clips)
    {
        AudioClip clip = clips[Random.Range(0, clips.Length)];
        audioSource.PlayOneShot(clip);
    }
}
