using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using static MainMenuScript;
using static PauseMenuScript;

public class MainMenuScript : MonoBehaviour
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

    public FadeOutConfig fadeOutConfig;

    [System.Serializable]
    public struct FadeOutConfig
    {
        public Image image;
        public float fadeDuration;
        public AnimationCurve fadeCurve;
        public RectMask2D treeMask;
        public int treeMaskFadeValue;
        public float animDuration;
        public AnimationCurve scaleCurve;
        public Transform menuTransfrom;
        public GameObject button_group_main;
    }
   ;


    public ApearConfig apearConfig;

    [System.Serializable]
    public struct ApearConfig
    {
        public float animDuration;
        public AnimationCurve scaleCurve;
        public Transform menuTransfrom;
        public GameObject button_group_main;
    }
   ;

    bool stopRotation = true;

    void Start()
    {
        //spawnButtons();
        StartCoroutine(Appear());

    }
    public void Play()
    {

        StartCoroutine(FadeOut());
    }
    public IEnumerator FadeOut()
    {

        float time = 0;

        while (time < fadeOutConfig.fadeDuration)
        {
            time += Time.deltaTime;
            Color color = fadeOutConfig.image.color;
            color.a = fadeOutConfig.fadeCurve.Evaluate(time/fadeOutConfig.fadeDuration);
            fadeOutConfig.image.color = color;
            var softnessValue = Mathf.FloorToInt(fadeOutConfig.fadeCurve.Evaluate(time / fadeOutConfig.fadeDuration) * fadeOutConfig.treeMaskFadeValue);
            fadeOutConfig.treeMask.softness = new Vector2Int(softnessValue, softnessValue);
            yield return null;
        }
        stopRotation = true;
        time = 0;
        while (time < fadeOutConfig.animDuration)
        {
            time += Time.deltaTime;
            foreach (Rotator r in rotators)
            {
                Vector3 angles = r.image.rectTransform.eulerAngles;
                angles.z = angles.z - r.roateSpeed * 30
                    * Time.deltaTime
                    * (r.direction ? 1 : -1);
                r.image.rectTransform.eulerAngles = angles;
            }
            var floatRange = (time / fadeOutConfig.animDuration);

            float scale = 1 * fadeOutConfig.scaleCurve.Evaluate(floatRange);
            fadeOutConfig.menuTransfrom.localScale = new Vector3(
                 scale, scale, scale
             );
            if (floatRange > 0.8f )
            {
                fadeOutConfig.button_group_main.SetActive(false);
            }
            yield return null;
        }
        SceneManager.LoadScene(1);

    }

    public IEnumerator Appear()
    {
        float time = 0;

        while (time < apearConfig.animDuration)
        {
            time += Time.deltaTime;
            foreach (Rotator r in rotators)
            {
                Vector3 angles = r.image.rectTransform.eulerAngles;
                angles.z = angles.z - r.roateSpeed * 30
                    * Time.deltaTime
                    * (r.direction ? 1 : -1);
                r.image.rectTransform.eulerAngles = angles;
            }
            var floatRange = (time / apearConfig.animDuration);

            float scale = 1 * apearConfig.scaleCurve.Evaluate(floatRange);
            apearConfig.menuTransfrom.localScale = new Vector3(
                 scale, scale, scale
             );
            if (floatRange > 0.8f)
            {
                apearConfig.button_group_main.SetActive(true);
            }
            yield return null;
        }
        stopRotation = false;
    }
  
    public void Quit()
    {
        Application.Quit();
    }


    // Update is called once per frame
    void Update()
    {
        if(!stopRotation)
        {
            foreach (Rotator r in rotators)
            {
                Vector3 angles = r.image.rectTransform.eulerAngles;
                angles.z = angles.z - r.roateSpeed * Time.deltaTime * (r.direction ? 1 : -1);
                r.image.rectTransform.eulerAngles = angles;
            }
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
            float progress = time / transitionConfig.duration;
            float scale = transitionConfig.original_scale * transitionConfig.curve.Evaluate(progress);
            transitionConfig.image.rectTransform.localScale = new Vector3(
                scale, scale, scale
            );

            if (progress < 0.2)
            {
                transitionConfig.button_group_settings.SetActive(false);
                transitionConfig.button_group_main.SetActive(false);
            }

            if (progress > 0.8)
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
