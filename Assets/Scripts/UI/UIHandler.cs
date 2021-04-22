using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIHandler : MonoBehaviour
{
    public void StartOnClick()
    {
        SceneManager.LoadScene("MainScene");
    }

    public void QuitOnClick()
    {
        Application.Quit();
    }
}
