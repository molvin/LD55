using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class PauseMenuScript : MonoBehaviour
{



    public Rotator[] rotators;
    public bool appear;
    private float appear_timer;
    public AppearanceAnimationConfig appearanceAnimationConfig;

    public GameObject background;
    public bool enableBackground;

  
    [System.Serializable]
    public struct AppearanceAnimationConfig
    {
        public RectTransform transform;
        public float duration;
        public AnimationCurve curve;
        public float min_scale_factor;
        public float original_scale;
        public GameObject button_group_main;

        public Image image_fade;
        public AnimationCurve curve_fade;
        [Range(0f, 1f)]
        public float max_alpha_fade;
    }

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


    [Range(0f, 1f)]
    public float floatRange;

    [System.Serializable]
    public struct Rotator
    {
        public Image image;
        public float roateSpeed;
        public bool direction;
    }


    public void Restart()
    {
        SceneManager.LoadScene(1);
    }



    public void QuitToMainMenu()
    {
        SceneManager.LoadScene(0);
    }


    public void Start()
    {
        this.GetComponent<Canvas>().enabled = true;
        appear_timer = appear ? appearanceAnimationConfig.duration : 0;
    }



    void Update()
    {

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            toggleAppear();
        }
        AppearAnimation();



        bool boostSeed = appear_timer > 0 && appear_timer  != 1;
        foreach (Rotator r in rotators)
        {
    
            Vector3 angles = r.image.rectTransform.eulerAngles;
            angles.z = angles.z - r.roateSpeed
                * (boostSeed ? 30 : 1)
                * Time.deltaTime
                * (r.direction ? 1 : -1);   
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


    public void AppearAnimation()
    {
        appear_timer += Time.deltaTime * (appear ? 1 : -1);
        appear_timer = Mathf.Clamp(appear_timer, 0, appearanceAnimationConfig.duration);
        floatRange = (appear_timer / appearanceAnimationConfig.duration);


        float scale = appearanceAnimationConfig.original_scale * appearanceAnimationConfig.curve.Evaluate(floatRange);
        if (floatRange > 0.8f && floatRange != 1 && appear)
        {
            appearanceAnimationConfig.button_group_main.SetActive(true);
        }
        else if (floatRange < 0.8f && !appear)
        {
            appearanceAnimationConfig.button_group_main.SetActive(false);

        }

        appearanceAnimationConfig.transform.localScale = new Vector3(
            scale, scale, scale
        );
        appearanceAnimationConfig.image_fade.raycastTarget = floatRange > 0.2f;
        Color c = appearanceAnimationConfig.image_fade.color;
        c.a = appearanceAnimationConfig.curve_fade.Evaluate(floatRange) * appearanceAnimationConfig.max_alpha_fade;
        appearanceAnimationConfig.image_fade.color = c;
    }

    public void toggleAppear()
    {
        appear =!appear;
    }





}
