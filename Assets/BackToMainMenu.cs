using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BackToMainMenu : MonoBehaviour
{
    private void Update()
    {
        if(Input.anyKey)
        {
            ToMainMenu();
        }
    }

    public void ToMainMenu()
    {
       
        SceneManager.LoadScene(0);
    }
}
